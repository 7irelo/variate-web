"""Per-request tracing, LLM cost accounting and Prometheus metrics.

Every request gets a Trace bound to a context variable. Code anywhere in the
call path opens spans with `span("name")` and records model calls with
`record_llm_call(...)`. When the request ends, the trace is logged as a single
JSON line (and optionally appended to a JSONL sink that the eval harness and
offline analysis read), and its numbers feed the Prometheus metrics.
"""

from __future__ import annotations

import json
import logging
import re
import secrets
import time
from contextlib import contextmanager
from contextvars import ContextVar
from dataclasses import asdict, dataclass, field
from pathlib import Path
from typing import Any, Iterator

from prometheus_client import CollectorRegistry, Counter, Histogram

logger = logging.getLogger("assistant.trace")

REGISTRY = CollectorRegistry()

REQUESTS = Counter(
    "assistant_requests_total",
    "Requests handled, by tenant, endpoint, routed intent and router that decided it.",
    ["tenant", "endpoint", "intent", "route_source"],
    registry=REGISTRY,
)
REQUEST_LATENCY = Histogram(
    "assistant_request_latency_seconds",
    "End-to-end request latency.",
    ["endpoint"],
    buckets=(0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2, 4, 8),
    registry=REGISTRY,
)
STAGE_LATENCY = Histogram(
    "assistant_stage_latency_seconds",
    "Latency of named pipeline stages (routing, bm25, dense, rerank, answer...).",
    ["stage"],
    buckets=(0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2, 4),
    registry=REGISTRY,
)
LLM_TOKENS = Counter(
    "assistant_llm_tokens_total",
    "Tokens sent to and received from model providers.",
    ["provider", "model", "purpose", "direction"],
    registry=REGISTRY,
)
LLM_COST = Counter(
    "assistant_llm_cost_usd_total",
    "Estimated model spend in USD, from the pricing table in assistant.llm.",
    ["tenant", "model", "purpose"],
    registry=REGISTRY,
)
LLM_ERRORS = Counter(
    "assistant_llm_errors_total",
    "Model calls that failed and fell back to a non-LLM path.",
    ["provider", "purpose", "error"],
    registry=REGISTRY,
)
ROUTER_ESCALATIONS = Counter(
    "assistant_router_escalations_total",
    "Cascade decisions that escalated from the distilled classifier to the LLM router.",
    ["tenant"],
    registry=REGISTRY,
)
BUDGET_DEGRADATIONS = Counter(
    "assistant_budget_degradations_total",
    "Requests served without LLM calls because the tenant's daily budget was spent.",
    ["tenant"],
    registry=REGISTRY,
)


@dataclass
class Span:
    name: str
    start_ms: float
    duration_ms: float = 0.0
    attributes: dict[str, Any] = field(default_factory=dict)


@dataclass
class LlmCall:
    provider: str
    model: str
    purpose: str
    input_tokens: int
    output_tokens: int
    cost_usd: float
    latency_ms: float


@dataclass
class Trace:
    trace_id: str
    tenant: str
    endpoint: str
    started: float = field(default_factory=time.perf_counter)
    spans: list[Span] = field(default_factory=list)
    llm_calls: list[LlmCall] = field(default_factory=list)
    attributes: dict[str, Any] = field(default_factory=dict)

    @property
    def cost_usd(self) -> float:
        return sum(call.cost_usd for call in self.llm_calls)

    @property
    def elapsed_ms(self) -> float:
        return (time.perf_counter() - self.started) * 1000

    def to_dict(self) -> dict[str, Any]:
        return {
            "trace_id": self.trace_id,
            "tenant": self.tenant,
            "endpoint": self.endpoint,
            "latency_ms": round(self.elapsed_ms, 2),
            "cost_usd": round(self.cost_usd, 8),
            "attributes": self.attributes,
            "spans": [asdict(s) for s in self.spans],
            "llm_calls": [asdict(c) for c in self.llm_calls],
        }


_current: ContextVar[Trace | None] = ContextVar("assistant_trace", default=None)
_TRACEPARENT = re.compile(r"^[0-9a-f]{2}-([0-9a-f]{32})-[0-9a-f]{16}-[0-9a-f]{2}$")


def trace_id_from_traceparent(header: str | None) -> str:
    """Reuse the W3C trace id from an upstream `traceparent` header when present."""
    if header and (match := _TRACEPARENT.match(header.strip())):
        return match.group(1)
    return secrets.token_hex(16)


def start_trace(tenant: str, endpoint: str, traceparent: str | None = None) -> Trace:
    trace = Trace(trace_id=trace_id_from_traceparent(traceparent), tenant=tenant, endpoint=endpoint)
    _current.set(trace)
    return trace


def current_trace() -> Trace | None:
    return _current.get()


@contextmanager
def span(name: str, **attributes: Any) -> Iterator[Span]:
    trace = _current.get()
    start = time.perf_counter()
    item = Span(name=name, start_ms=0.0, attributes=dict(attributes))
    if trace is not None:
        item.start_ms = round((start - trace.started) * 1000, 3)
    try:
        yield item
    finally:
        elapsed = time.perf_counter() - start
        item.duration_ms = round(elapsed * 1000, 3)
        STAGE_LATENCY.labels(stage=name).observe(elapsed)
        if trace is not None:
            trace.spans.append(item)


def record_llm_call(
    provider: str,
    model: str,
    purpose: str,
    input_tokens: int,
    output_tokens: int,
    cost_usd: float,
    latency_ms: float,
) -> None:
    LLM_TOKENS.labels(provider, model, purpose, "input").inc(input_tokens)
    LLM_TOKENS.labels(provider, model, purpose, "output").inc(output_tokens)
    trace = _current.get()
    tenant = trace.tenant if trace else "none"
    LLM_COST.labels(tenant, model, purpose).inc(cost_usd)
    if trace is not None:
        trace.llm_calls.append(
            LlmCall(provider, model, purpose, input_tokens, output_tokens, cost_usd, round(latency_ms, 2))
        )


def finish_trace(trace: Trace, sink: Path | None = None) -> dict[str, Any]:
    """Close a trace: emit metrics, log it and append it to the JSONL sink."""
    REQUEST_LATENCY.labels(endpoint=trace.endpoint).observe(trace.elapsed_ms / 1000)
    REQUESTS.labels(
        trace.tenant,
        trace.endpoint,
        str(trace.attributes.get("intent", "none")),
        str(trace.attributes.get("route_source", "none")),
    ).inc()
    record = trace.to_dict()
    logger.info(json.dumps(record))
    if sink is not None:
        sink.parent.mkdir(parents=True, exist_ok=True)
        with sink.open("a", encoding="utf-8") as handle:
            handle.write(json.dumps(record) + "\n")
    _current.set(None)
    return record
