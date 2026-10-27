from __future__ import annotations

import time

import httpx
import jwt
import pytest
from fastapi.testclient import TestClient

from assistant.config import Settings
from assistant.llm import LlmClient, LlmUnavailable
from assistant.main import build_components, create_app
from assistant.observability import finish_trace, start_trace, trace_id_from_traceparent
from assistant.retrieval import reciprocal_rank_fusion
from assistant.retrieval.bm25 import tokenize
from assistant.retrieval.filters import parse_constraints
from assistant.routing import CascadeRouter
from assistant.schemas import Intent, RouteDecision
from assistant.tenancy import BudgetLedger, TenantError, validate_tenant

TENANT = {"X-Tenant-Id": "variate"}


def offline_settings(**overrides) -> Settings:
    return Settings(ANTHROPIC_API_KEY=None, OPENAI_API_KEY=None, **overrides)


def token(sub: str, settings: Settings) -> str:
    return jwt.encode(
        {"sub": sub, "iss": settings.jwt_issuer, "aud": settings.jwt_audience, "exp": int(time.time()) + 600},
        settings.jwt_signing_key,
        algorithm="HS256",
    )


def order_transport(seen: list[str]) -> httpx.MockTransport:
    def handler(request: httpx.Request) -> httpx.Response:
        seen.append(request.url.path)
        user = request.url.path.rsplit("/", 1)[-1]
        return httpx.Response(200, json=[
            {"id": "aaaaaaaa-0000", "userId": user, "status": "Shipped", "createdAtUtc": "2026-10-20T10:00:00Z"},
            {"id": "bbbbbbbb-0000", "userId": "someone-else", "status": "Paid", "createdAtUtc": "2026-10-21T10:00:00Z"},
        ])
    return httpx.MockTransport(handler)


@pytest.fixture(scope="module")
def seen_paths() -> list[str]:
    return []


@pytest.fixture(scope="module")
def client(seen_paths):
    components = build_components(offline_settings(), order_transport=order_transport(seen_paths))
    with TestClient(create_app(components)) as test_client:
        yield test_client


# ---------------------------------------------------------------- unit


def test_tokenize_folds_plurals_and_drops_stopwords():
    assert tokenize("Show me the guitars and glass") == ["guitar", "glass"]


def test_constraints_parse_rand_amounts():
    c = parse_constraints("toys under R1 000 on sale")
    assert c.max_price == 1000 and c.on_sale
    assert parse_constraints("watch below r5k").max_price == 5000
    assert not parse_constraints("garmin watch").active


def test_rrf_rewards_agreement():
    fused = reciprocal_rank_fusion({"a": [1, 2, 3], "b": [2, 1, 4]}, k=60)
    assert {fused[0][0], fused[1][0]} == {1, 2}
    assert fused[-1][0] in (3, 4)


def test_tenant_validation():
    assert validate_tenant(" Variate ", ["variate"]) == "variate"
    with pytest.raises(TenantError):
        validate_tenant("other", ["variate"])
    with pytest.raises(TenantError):
        validate_tenant("../etc", ["variate"])


def test_budget_ledger_blocks_after_spend():
    ledger = BudgetLedger(daily_budget_usd=0.01)
    assert ledger.allows_llm("variate")
    ledger.add("variate", 0.02)
    assert not ledger.allows_llm("variate")
    assert ledger.allows_llm("other")


def test_traceparent_is_reused():
    tid = "0af7651916cd43dd8448eb211c80319c"
    assert trace_id_from_traceparent(f"00-{tid}-b7ad6b7169203331-01") == tid
    assert len(trace_id_from_traceparent("garbage")) == 32


class _Student:
    name = "distilled"

    def __init__(self, confidence):
        self.confidence = confidence

    def route(self, text):
        return RouteDecision(intent=Intent.product_search, confidence=self.confidence, source="distilled")


class _Teacher:
    name = "llm"

    def __init__(self, fail=False):
        self.calls = 0
        self.fail = fail

    def route(self, text):
        self.calls += 1
        if self.fail:
            raise LlmUnavailable("down")
        return RouteDecision(intent=Intent.order_status, confidence=1.0, source="llm")


def test_cascade_escalates_only_when_unsure():
    teacher = _Teacher()
    assert CascadeRouter(_Student(0.9), teacher, 0.55).route("x").source == "distilled"
    assert teacher.calls == 0
    decision = CascadeRouter(_Student(0.3), teacher, 0.55).route("x")
    assert decision.source == "cascade:llm" and decision.intent == Intent.order_status
    assert CascadeRouter(_Student(0.3), teacher, 0.55).route("x", allow_llm=False).source == "distilled"


def test_cascade_survives_teacher_outage():
    decision = CascadeRouter(_Student(0.3), _Teacher(fail=True), 0.55).route("x")
    assert decision.source == "distilled"


def test_llm_client_without_keys_raises_unavailable():
    llm = LlmClient(offline_settings())
    with pytest.raises(LlmUnavailable):
        llm.embed(["x"])


def test_trace_accumulates_spans():
    from assistant.observability import record_llm_call, span

    trace = start_trace("variate", "test")
    with span("stage"):
        record_llm_call("anthropic", "claude-haiku-5-5", "routing", 100, 10, 0.001, 5.0)
    record = finish_trace(trace)
    assert record["spans"][0]["name"] == "stage"
    assert record["cost_usd"] == pytest.approx(0.001)


# ---------------------------------------------------------------- api


def test_health(client):
    body = client.get("/health").json()
    assert body["status"] == "ok" and body["dense_backend"] == "lsa-local"


def test_requests_need_a_known_tenant(client):
    assert client.post("/api/v1/assistant/search", json={"query": "guitar"}).status_code == 400
    response = client.post("/api/v1/assistant/search", json={"query": "guitar"}, headers={"X-Tenant-Id": "acme"})
    assert response.status_code == 403


def test_search_returns_filtered_hits_and_trace_header(client):
    response = client.post("/api/v1/assistant/search", json={"query": "guitar under R20000", "top_k": 3}, headers=TENANT)
    assert response.status_code == 200
    assert response.headers["X-Trace-Id"] == response.json()["trace_id"]
    hits = response.json()["hits"]
    assert hits[0]["product"]["name"] == "Fender American Professional II Stratocaster"
    assert all(h["product"]["price"] <= 20000 for h in hits)


def test_chat_policy_intent_uses_fixed_answer(client):
    body = client.post("/api/v1/assistant/chat", json={"message": "I forgot my password"}, headers=TENANT).json()
    assert body["intent"] == "account_help"
    assert "Forgot password" in body["reply"]


def test_order_status_requires_sign_in(client):
    body = client.post("/api/v1/assistant/chat", json={"message": "where is my order"}, headers=TENANT).json()
    assert body["intent"] == "order_status"
    assert "sign in" in body["reply"].lower()


def test_order_status_only_shows_the_callers_orders(client, seen_paths):
    settings = offline_settings()
    headers = TENANT | {"Authorization": f"Bearer {token('user-123', settings)}"}
    body = client.post("/api/v1/assistant/chat", json={"message": "where is my order"}, headers=headers).json()
    assert seen_paths[-1] == "/api/v1/orders/user/user-123"
    assert "aaaaaaaa" in body["reply"] and "bbbbbbbb" not in body["reply"]


def test_forged_token_is_ignored(client):
    forged = jwt.encode({"sub": "victim", "exp": int(time.time()) + 600}, "wrong-key", algorithm="HS256")
    headers = TENANT | {"Authorization": f"Bearer {forged}"}
    body = client.post("/api/v1/assistant/chat", json={"message": "where is my order"}, headers=headers).json()
    assert "sign in" in body["reply"].lower()


def test_route_endpoint_rejects_unknown_strategy(client):
    response = client.post("/api/v1/assistant/route", json={"message": "hi", "strategy": "magic"}, headers=TENANT)
    assert response.status_code == 400


def test_metrics_expose_request_counters(client):
    client.post("/api/v1/assistant/route", json={"message": "track my parcel", "strategy": "rules"}, headers=TENANT)
    text = client.get("/metrics").text
    assert "assistant_requests_total" in text
    assert 'tenant="variate"' in text


def test_rate_limiter_window():
    from assistant.tenancy import RateLimiter

    limiter = RateLimiter(per_minute=2)
    assert limiter.allow("k", now=0) and limiter.allow("k", now=1)
    assert not limiter.allow("k", now=2)
    assert limiter.allow("k", now=61)


def test_production_refuses_placeholder_signing_key():
    with pytest.raises(RuntimeError):
        offline_settings(environment="production").validate_for_production()
    offline_settings(environment="production", jwt_signing_key="x" * 40).validate_for_production()


def test_docs_are_off_by_default(client):
    assert client.get("/docs").status_code == 404
    assert client.get("/openapi.json").status_code == 404
