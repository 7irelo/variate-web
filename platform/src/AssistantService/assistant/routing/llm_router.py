from __future__ import annotations

from pydantic import BaseModel

from ..llm import LlmClient
from ..schemas import INTENT_DESCRIPTIONS, Intent, RouteDecision

SYSTEM_PROMPT = """You route customer messages for Variate, a South African online store.

Pick exactly one intent for the customer's message:
{intents}

Guidance for close calls:
- A message naming a specific product and asking about it is product_question; asking what to buy is product_search.
- Cancelling an order counts as returns_refunds. A problem with being charged is payment_issue even if it mentions an order.
- Only choose human_handoff when the customer asks for a person or to escalate, not merely because they are upset.
- Choose out_of_scope for anything unrelated to shopping at Variate."""


class RouterOutput(BaseModel):
    intent: Intent


class LlmRouter:
    name = "llm"

    def __init__(self, llm: LlmClient, model: str):
        self.llm = llm
        self.model = model
        intents = "\n".join(f"- {i.value}: {d}" for i, d in INTENT_DESCRIPTIONS.items())
        self.system = SYSTEM_PROMPT.format(intents=intents)

    def route(self, text: str) -> RouteDecision:
        result = self.llm.parse(
            model=self.model,
            purpose="routing",
            system=self.system,
            user=f"<message>{text}</message>",
            output=RouterOutput,
            max_tokens=64,
        )
        return RouteDecision(intent=result.intent, confidence=1.0, source=self.name)
