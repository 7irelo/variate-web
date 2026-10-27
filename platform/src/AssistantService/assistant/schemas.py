from __future__ import annotations

from enum import Enum

from pydantic import BaseModel, Field


class Intent(str, Enum):
    product_search = "product_search"
    product_question = "product_question"
    order_status = "order_status"
    returns_refunds = "returns_refunds"
    payment_issue = "payment_issue"
    account_help = "account_help"
    human_handoff = "human_handoff"
    out_of_scope = "out_of_scope"


INTENT_DESCRIPTIONS: dict[Intent, str] = {
    Intent.product_search: "Looking for products to buy, browsing, asking for recommendations or gift ideas.",
    Intent.product_question: "A question about a specific named product: features, specs, compatibility, price.",
    Intent.order_status: "Where an existing order is, delivery dates, tracking, order history.",
    Intent.returns_refunds: "Returning, exchanging or cancelling an order, refund timing, damaged or wrong items.",
    Intent.payment_issue: "Card declined, double charge, payment methods, failed checkout, invoices.",
    Intent.account_help: "Login, password reset, registration, email or profile changes, deleting an account.",
    Intent.human_handoff: "The customer explicitly asks for a person, agent or to escalate a complaint.",
    Intent.out_of_scope: "Anything unrelated to shopping at Variate.",
}


class Product(BaseModel):
    id: int
    category_id: int
    category: str
    name: str
    description: str
    price: int = Field(description="Price in ZAR")
    release_date: str
    image_url: str
    on_sale: bool

    @property
    def document(self) -> str:
        return f"{self.name}. {self.description} Category: {self.category}."


class RouteDecision(BaseModel):
    intent: Intent
    confidence: float = Field(ge=0.0, le=1.0)
    source: str = Field(description="Which router produced the decision")


class SearchHit(BaseModel):
    product: Product
    score: float
    stages: dict[str, int] = Field(default_factory=dict, description="Rank of the hit in each retrieval stage")


class ChatRequest(BaseModel):
    message: str = Field(min_length=1, max_length=2000)


class ChatResponse(BaseModel):
    trace_id: str
    intent: Intent
    route_source: str
    reply: str
    products: list[SearchHit] = Field(default_factory=list)
    handoff: bool = False


class SearchRequest(BaseModel):
    query: str = Field(min_length=1, max_length=500)
    top_k: int = Field(default=5, ge=1, le=20)
    rerank: bool = True


class SearchResponse(BaseModel):
    trace_id: str
    hits: list[SearchHit]


class RouteRequest(BaseModel):
    message: str = Field(min_length=1, max_length=2000)
    strategy: str | None = None


class RouteResponse(BaseModel):
    trace_id: str
    decision: RouteDecision
