from __future__ import annotations

from functools import lru_cache
from pathlib import Path

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict

SERVICE_ROOT = Path(__file__).resolve().parents[1]


class Settings(BaseSettings):
    """Runtime configuration. Every field can be set via an ASSISTANT_* env var."""

    model_config = SettingsConfigDict(env_prefix="ASSISTANT_", env_file=".env", extra="ignore")

    # Provider keys are read under their conventional names so the SDKs and this
    # service agree on where credentials come from.
    anthropic_api_key: str | None = Field(default=None, validation_alias="ANTHROPIC_API_KEY")
    openai_api_key: str | None = Field(default=None, validation_alias="OPENAI_API_KEY")

    # Model choices. Each one is picked for its job:
    #  - routing and reranking are short classification-style calls on every
    #    request, so they use the cheapest current Claude model at low effort;
    #  - grounded answers to product questions need better judgement and
    #    phrasing, so they use Sonnet;
    #  - the offline eval judge runs rarely and must be the most reliable grader,
    #    so it uses Opus;
    #  - Anthropic has no embeddings endpoint, so dense retrieval uses OpenAI.
    router_model: str = "claude-haiku-5-5"
    rerank_model: str = "claude-haiku-5-5"
    answer_model: str = "claude-sonnet-5-5"
    judge_model: str = "claude-opus-5-5"
    embedding_model: str = "text-embedding-3-small"

    # Router cascade: the distilled classifier answers when it is confident,
    # otherwise the request escalates to the LLM router.
    router_strategy: str = "cascade"  # rules | distilled | llm | cascade
    cascade_threshold: float = 0.55

    # Retrieval
    retrieval_top_k: int = 5
    rerank_candidates: int = 12
    rrf_k: int = 60
    enable_rerank: bool = True

    # "production" turns on startup checks that refuse insecure defaults.
    environment: str = "development"
    expose_docs: bool = False

    # Tenancy, budgets and rate limits
    rate_limit_per_minute: int = 60
    allowed_tenants: list[str] = ["variate"]
    tenant_daily_budget_usd: float = 5.0

    # Integrations
    order_service_url: str = "http://order-service:8080"
    jwt_issuer: str = "variate.auth"
    jwt_audience: str = "variate.services"
    jwt_signing_key: str = "replace-this-in-real-deployments-with-a-long-secret"

    # Paths
    catalog_path: Path = SERVICE_ROOT / "data" / "catalog.json"
    policies_path: Path = SERVICE_ROOT / "data" / "policies.json"
    router_model_path: Path = SERVICE_ROOT / "models" / "router.joblib"
    embedding_cache_path: Path = SERVICE_ROOT / "models" / "embeddings.npz"
    trace_sink_path: Path | None = None

    def validate_for_production(self) -> None:
        if self.environment != "production":
            return
        problems = []
        if self.jwt_signing_key.startswith("replace-this") or len(self.jwt_signing_key) < 32:
            problems.append("ASSISTANT_JWT_SIGNING_KEY must be a real secret of at least 32 characters")
        if self.expose_docs:
            problems.append("ASSISTANT_EXPOSE_DOCS must be false in production")
        if problems:
            raise RuntimeError("refusing to start: " + "; ".join(problems))

    @property
    def anthropic_enabled(self) -> bool:
        return bool(self.anthropic_api_key)

    @property
    def openai_enabled(self) -> bool:
        return bool(self.openai_api_key)


@lru_cache
def get_settings() -> Settings:
    return Settings()
