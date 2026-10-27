from __future__ import annotations

from pydantic import BaseModel, Field

from ..catalog import format_price
from ..llm import LlmClient
from ..schemas import Product

SYSTEM_PROMPT = """You rank products from the Variate catalog for a shopper's search.

Return the ids of the candidates that genuinely fit the search, best match first.
Leave out candidates that do not fit. Judge fit on what the product is and who it is for,
not on shared words: a "watch for running" is a GPS sports watch, not a luxury dress watch."""


class RerankOutput(BaseModel):
    ranked_ids: list[int] = Field(description="Ids of relevant candidates, most relevant first")


class LlmReranker:
    def __init__(self, llm: LlmClient, model: str):
        self.llm = llm
        self.model = model

    def rerank(self, query: str, candidates: list[Product]) -> list[int]:
        lines = "\n".join(
            f"[{p.id}] {p.name} | {p.category} | {format_price(p.price)}{' | on sale' if p.on_sale else ''} | {p.description}"
            for p in candidates
        )
        result = self.llm.parse(
            model=self.model,
            purpose="rerank",
            system=SYSTEM_PROMPT,
            user=f"<search>{query}</search>\n<candidates>\n{lines}\n</candidates>",
            output=RerankOutput,
            max_tokens=256,
        )
        allowed = {p.id for p in candidates}
        seen: set[int] = set()
        ranked = []
        for product_id in result.ranked_ids:
            if product_id in allowed and product_id not in seen:
                seen.add(product_id)
                ranked.append(product_id)
        return ranked
