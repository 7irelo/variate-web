from __future__ import annotations

import json
import logging
from contextlib import asynccontextmanager
from dataclasses import dataclass

import httpx
from fastapi import Depends, FastAPI, Header, HTTPException, Request, Response
from prometheus_client import CONTENT_TYPE_LATEST, generate_latest

from .auth import customer_from_header
from .catalog import Catalog
from .config import Settings, get_settings
from .datasets import ROUTING_PATH
from .llm import LlmClient
from .observability import BUDGET_DEGRADATIONS, REGISTRY, Trace, finish_trace, start_trace
from .orders import OrderClient
from .pipeline import Assistant
from .retrieval import Retriever
from .retrieval.dense import Embedder, LsaEmbedder, OpenAIEmbedder
from .retrieval.rerank import LlmReranker
from .routing import CascadeRouter, DistilledRouter, LlmRouter, Router, RulesRouter
from .schemas import (
    ChatRequest,
    ChatResponse,
    RouteRequest,
    RouteResponse,
    SearchRequest,
    SearchResponse,
)
from .tenancy import BudgetLedger, RateLimiter, TenantError, validate_tenant

logging.basicConfig(level=logging.INFO, format="%(message)s")
logger = logging.getLogger(__name__)


@dataclass
class Components:
    settings: Settings
    llm: LlmClient
    assistant: Assistant
    routers: dict[str, Router]
    ledger: BudgetLedger
    limiter: RateLimiter


def build_components(
    settings: Settings,
    llm: LlmClient | None = None,
    order_transport: httpx.BaseTransport | None = None,
) -> Components:
    settings.validate_for_production()
    llm = llm or LlmClient(settings)
    catalog = Catalog.load(settings.catalog_path)
    embedder: Embedder = (
        OpenAIEmbedder(llm, settings.embedding_cache_path) if llm.has_openai else LsaEmbedder()
    )
    reranker = LlmReranker(llm, settings.rerank_model) if llm.has_anthropic else None
    retriever = Retriever(catalog, embedder, reranker, settings.rrf_k, settings.rerank_candidates)

    student = DistilledRouter.load(settings.router_model_path, fallback_dataset=ROUTING_PATH)
    teacher = LlmRouter(llm, settings.router_model) if llm.has_anthropic else None
    routers: dict[str, Router] = {
        "rules": RulesRouter(),
        "distilled": student,
        "cascade": CascadeRouter(student, teacher, settings.cascade_threshold),
    }
    if teacher is not None:
        routers["llm"] = teacher
    strategy = settings.router_strategy if settings.router_strategy in routers else "cascade"

    policies = json.loads(settings.policies_path.read_text(encoding="utf-8"))
    assistant = Assistant(
        settings, routers[strategy], retriever, llm, OrderClient(settings.order_service_url, transport=order_transport),
        policies,
    )
    logger.info(json.dumps({
        "event": "startup",
        "router": strategy,
        "dense_backend": retriever.dense_backend,
        "rerank": reranker is not None,
        "anthropic": llm.has_anthropic,
        "openai": llm.has_openai,
    }))
    return Components(
        settings, llm, assistant, routers,
        BudgetLedger(settings.tenant_daily_budget_usd), RateLimiter(settings.rate_limit_per_minute),
    )


def create_app(components: Components | None = None) -> FastAPI:
    @asynccontextmanager
    async def lifespan(app: FastAPI):
        app.state.components = components or build_components(get_settings())
        yield

    settings = components.settings if components else get_settings()
    docs = settings.expose_docs
    app = FastAPI(
        title="Variate Assistant Service",
        version="1.0.0",
        lifespan=lifespan,
        docs_url="/docs" if docs else None,
        redoc_url=None,
        openapi_url="/openapi.json" if docs else None,
    )

    def get_components(request: Request) -> Components:
        return request.app.state.components

    def tenant(request: Request, x_tenant_id: str | None = Header(default=None)) -> str:
        components = get_components(request)
        try:
            tenant_id = validate_tenant(x_tenant_id, components.settings.allowed_tenants)
        except TenantError as exc:
            raise HTTPException(status_code=403 if x_tenant_id else 400, detail=str(exc)) from exc
        # Keyed on the socket peer, not X-Forwarded-For, which callers can forge.
        caller = request.client.host if request.client else "unknown"
        if not components.limiter.allow(f"{tenant_id}:{caller}"):
            raise HTTPException(status_code=429, detail="rate limit exceeded", headers={"Retry-After": "60"})
        return tenant_id

    def begin(request: Request, endpoint: str, tenant_id: str) -> tuple[Trace, bool]:
        trace = start_trace(tenant_id, endpoint, request.headers.get("traceparent"))
        allow_llm = get_components(request).ledger.allows_llm(tenant_id)
        if not allow_llm:
            BUDGET_DEGRADATIONS.labels(tenant_id).inc()
            trace.attributes["degraded"] = "budget"
        return trace, allow_llm

    def end(request: Request, response: Response, trace: Trace) -> None:
        components = get_components(request)
        components.ledger.add(trace.tenant, trace.cost_usd)
        finish_trace(trace, components.settings.trace_sink_path)
        response.headers["X-Trace-Id"] = trace.trace_id

    @app.get("/health")
    def health(request: Request) -> dict:
        c = get_components(request)
        return {
            "service": "assistant-service",
            "status": "ok",
            "dense_backend": c.assistant.retriever.dense_backend,
            "router": c.assistant.router.name,
        }

    @app.get("/metrics")
    def metrics() -> Response:
        return Response(generate_latest(REGISTRY), media_type=CONTENT_TYPE_LATEST)

    @app.post("/api/v1/assistant/chat", response_model=ChatResponse)
    def chat(
        body: ChatRequest,
        request: Request,
        response: Response,
        tenant_id: str = Depends(tenant),
        authorization: str | None = Header(default=None),
    ) -> ChatResponse:
        c = get_components(request)
        trace, allow_llm = begin(request, "chat", tenant_id)
        try:
            customer = customer_from_header(authorization, c.settings)
            reply = c.assistant.handle(body.message, customer, allow_llm)
            trace.attributes.update(intent=reply.decision.intent.value, route_source=reply.decision.source)
            return ChatResponse(
                trace_id=trace.trace_id,
                intent=reply.decision.intent,
                route_source=reply.decision.source,
                reply=reply.text,
                products=reply.products,
                handoff=reply.handoff,
            )
        finally:
            end(request, response, trace)

    @app.post("/api/v1/assistant/search", response_model=SearchResponse)
    def search(body: SearchRequest, request: Request, response: Response, tenant_id: str = Depends(tenant)) -> SearchResponse:
        c = get_components(request)
        trace, allow_llm = begin(request, "search", tenant_id)
        try:
            hits = c.assistant.search(body.query, body.top_k, body.rerank, allow_llm)
            return SearchResponse(trace_id=trace.trace_id, hits=hits)
        finally:
            end(request, response, trace)

    @app.post("/api/v1/assistant/route", response_model=RouteResponse)
    def route(body: RouteRequest, request: Request, response: Response, tenant_id: str = Depends(tenant)) -> RouteResponse:
        c = get_components(request)
        trace, allow_llm = begin(request, "route", tenant_id)
        try:
            if body.strategy is None:
                decision = c.assistant.route(body.message, allow_llm)
            elif body.strategy not in c.routers:
                raise HTTPException(status_code=400, detail=f"strategy must be one of {sorted(c.routers)}")
            elif body.strategy == "llm" and not allow_llm:
                raise HTTPException(status_code=429, detail="tenant LLM budget spent for today")
            else:
                decision = c.routers[body.strategy].route(body.message)
            trace.attributes.update(intent=decision.intent.value, route_source=decision.source)
            return RouteResponse(trace_id=trace.trace_id, decision=decision)
        finally:
            end(request, response, trace)

    return app


app = create_app()
