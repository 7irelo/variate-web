"""Keyword router: the zero-cost baseline every other router has to beat."""

from __future__ import annotations

import re

from ..schemas import Intent, RouteDecision

# Checked in order; the first pattern that matches wins.
RULES: list[tuple[Intent, re.Pattern[str]]] = [
    (Intent.human_handoff, re.compile(r"\b(human|real person|agent|manager|supervisor|speak to|talk to someone|escalate)\b", re.I)),
    (Intent.returns_refunds, re.compile(r"\b(return|refund|exchange|send (it )?back|cancel|broken|damaged|wrong item)\b", re.I)),
    (Intent.payment_issue, re.compile(r"\b(card|payment|paid|charged|declined|eft|paypal|invoice|billing)\b", re.I)),
    (Intent.order_status, re.compile(r"\b(order|delivery|deliver|tracking|track|shipped|courier|arrive)\b", re.I)),
    (Intent.account_help, re.compile(r"\b(password|log ?in|sign ?in|account|email address|profile|register)\b", re.I)),
    (Intent.product_question, re.compile(r"\b(does|is|can|will|how (big|long|heavy|much)|warranty|compatible|spec)\b.*\b(the|this|it)\b", re.I)),
    (Intent.product_search, re.compile(r"\b(looking for|need|want|buy|recommend|gift|show me|cheap|best|under r?\d+)\b", re.I)),
]


class RulesRouter:
    name = "rules"

    def route(self, text: str) -> RouteDecision:
        for intent, pattern in RULES:
            if pattern.search(text):
                return RouteDecision(intent=intent, confidence=1.0, source=self.name)
        return RouteDecision(intent=Intent.out_of_scope, confidence=0.0, source=self.name)
