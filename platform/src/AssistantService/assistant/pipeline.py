"""Turns a routed message into a reply.

Only two intents involve a model after routing: reranking for product search
and a grounded answer for product questions. Order status is an API lookup,
and policy questions have fixed, reviewed answers; generating those with a
model would add cost, latency and a chance of inventing a policy, and buy
nothing in return.
"""

from __future__ import annotations

import json
import logging
from dataclasses import dataclass

from .auth import Customer
from .catalog import format_price
from .config import Settings
from .llm import LlmClient, LlmUnavailable
from .observability import span
from .orders import OrderClient, OrderServiceError
from .retrieval import Retriever
from .routing import CascadeRouter, Router, timed_route
from .schemas import Intent, RouteDecision, SearchHit

logger = logging.getLogger(__name__)

ANSWER_SYSTEM = """You are Variate's shopping assistant. Answer the customer's question using only the
product facts provided. If the facts don't answer the question, say so plainly and suggest what
they could check instead; never invent specifications, stock levels or delivery promises.
Prices are in South African rand. Reply in at most three sentences."""


@dataclass
class Reply:
    decision: RouteDecision
    text: str
    products: list[SearchHit]
    handoff: bool = False


class Assistant:
    def __init__(
        self,
        settings: Settings,
        router: Router,
        retriever: Retriever,
        llm: LlmClient,
        orders: OrderClient,
        policies: dict[str, str],
    ):
        self.settings = settings
        self.router = router
        self.retriever = retriever
        self.llm = llm
        self.orders = orders
        self.policies = policies

    def route(self, message: str, allow_llm: bool) -> RouteDecision:
        if isinstance(self.router, CascadeRouter):
            return timed_route(self.router, message, allow_llm=allow_llm)
        return timed_route(self.router, message)

    def handle(self, message: str, customer: Customer | None, allow_llm: bool) -> Reply:
        decision = self.route(message, allow_llm)
        intent = decision.intent
        if intent == Intent.product_search:
            return self._product_search(decision, message, allow_llm)
        if intent == Intent.product_question:
            return self._product_question(decision, message, allow_llm)
        if intent == Intent.order_status:
            return self._order_status(decision, customer)
        return Reply(decision, self.policies[intent.value], [], handoff=intent == Intent.human_handoff)

    def search(self, query: str, top_k: int, rerank: bool, allow_llm: bool) -> list[SearchHit]:
        use_rerank = rerank and allow_llm and self.settings.enable_rerank
        return self.retriever.search(query, top_k=top_k, rerank=use_rerank)

    def _product_search(self, decision: RouteDecision, message: str, allow_llm: bool) -> Reply:
        hits = self.search(message, self.settings.retrieval_top_k, rerank=True, allow_llm=allow_llm)
        if any("rerank" in h.stages for h in hits):
            hits = [h for h in hits if "rerank" in h.stages]
        if not hits:
            return Reply(decision, "I couldn't find anything matching that. Could you describe it another way?", [])
        lines = [f"- {h.product.name} ({format_price(h.product.price)})" for h in hits[:3]]
        return Reply(decision, "Here's what I found:\n" + "\n".join(lines), hits)

    def _product_question(self, decision: RouteDecision, message: str, allow_llm: bool) -> Reply:
        hits = self.retriever.search(message, top_k=3, rerank=False)
        if not hits:
            return Reply(decision, "I couldn't tell which product you mean. Could you give me its name?", [])
        if allow_llm:
            facts = json.dumps(
                [
                    {
                        "name": h.product.name,
                        "description": h.product.description,
                        "category": h.product.category,
                        "price": format_price(h.product.price),
                        "on_sale": h.product.on_sale,
                    }
                    for h in hits
                ],
                ensure_ascii=False,
            )
            try:
                with span("answer", model=self.settings.answer_model):
                    text = self.llm.answer(
                        model=self.settings.answer_model,
                        purpose="answer",
                        system=ANSWER_SYSTEM,
                        user=f"<products>{facts}</products>\n<question>{message}</question>",
                        max_tokens=400,
                    )
                return Reply(decision, text, hits)
            except LlmUnavailable as exc:
                logger.warning("answer generation unavailable, using template: %s", exc)
        top = hits[0].product
        text = f"{top.name}: {top.description} It costs {format_price(top.price)}."
        return Reply(decision, text, hits)

    def _order_status(self, decision: RouteDecision, customer: Customer | None) -> Reply:
        if customer is None:
            return Reply(decision, "Please sign in so I can look up your orders.", [])
        try:
            orders = self.orders.recent_orders(customer)
        except OrderServiceError as exc:
            logger.warning("order lookup failed: %s", exc)
            return Reply(decision, "I can't reach our order system right now. Please try again in a few minutes.", [])
        if not orders:
            return Reply(decision, "I can't see any orders on your account yet.", [])
        lines = [
            f"- Order {str(o.get('id', ''))[:8]}: {o.get('status', 'unknown')}, placed {str(o.get('createdAtUtc', ''))[:10]}"
            for o in orders
        ]
        return Reply(decision, "Your most recent orders:\n" + "\n".join(lines), [])
