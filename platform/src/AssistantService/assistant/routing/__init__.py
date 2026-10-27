from __future__ import annotations

import logging
from typing import Protocol

from ..llm import LlmUnavailable
from ..observability import ROUTER_ESCALATIONS, current_trace, span
from ..schemas import RouteDecision
from .distilled import DistilledRouter
from .llm_router import LlmRouter
from .rules import RulesRouter

logger = logging.getLogger(__name__)


class Router(Protocol):
    name: str

    def route(self, text: str) -> RouteDecision: ...


class CascadeRouter:
    """Distilled classifier first; escalate to the LLM only when it is unsure.

    If the LLM is unavailable (no key, outage, or the tenant's budget is
    spent) the distilled answer is used as-is, so routing never hard-fails.
    """

    name = "cascade"

    def __init__(self, student: DistilledRouter, teacher: LlmRouter | None, threshold: float):
        self.student = student
        self.teacher = teacher
        self.threshold = threshold

    def route(self, text: str, allow_llm: bool = True) -> RouteDecision:
        decision = self.student.route(text)
        if decision.confidence >= self.threshold or self.teacher is None or not allow_llm:
            return decision
        trace = current_trace()
        ROUTER_ESCALATIONS.labels(trace.tenant if trace else "none").inc()
        try:
            escalated = self.teacher.route(text)
        except LlmUnavailable as exc:
            logger.warning("cascade escalation failed, keeping distilled decision: %s", exc)
            return decision
        return escalated.model_copy(update={"source": "cascade:llm"})


def timed_route(router: Router, text: str, **kwargs) -> RouteDecision:
    with span("routing", router=router.name) as s:
        decision = router.route(text, **kwargs) if kwargs else router.route(text)
        s.attributes.update(intent=decision.intent.value, confidence=decision.confidence, source=decision.source)
    return decision


__all__ = ["CascadeRouter", "DistilledRouter", "LlmRouter", "Router", "RulesRouter", "timed_route"]
