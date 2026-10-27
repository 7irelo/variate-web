"""Thin wrappers around the Anthropic and OpenAI SDKs.

The wrappers do three things the call sites should not repeat: time the call,
convert token usage into an estimated cost, and record both on the current
trace. Prices are USD per million tokens and live here so a price change is a
one-line diff; check them against the providers' pricing pages before quoting
cost numbers.
"""

from __future__ import annotations

import time
from typing import TypeVar

import anthropic
import openai
from pydantic import BaseModel

from .config import Settings
from .observability import LLM_ERRORS, record_llm_call

T = TypeVar("T", bound=BaseModel)

# (input, output) USD per 1M tokens.
PRICING: dict[str, tuple[float, float]] = {
    "claude-haiku-5-5": (0.10, 0.50),
    "claude-sonnet-5-5": (2.00, 10.00),
    "claude-opus-5-5": (4.00, 20.00),
    "text-embedding-3-small": (0.02, 0.0),
    "text-embedding-3-large": (0.13, 0.0),
}


def estimate_cost(model: str, input_tokens: int, output_tokens: int) -> float:
    input_price, output_price = PRICING.get(model, (0.0, 0.0))
    return (input_tokens * input_price + output_tokens * output_price) / 1_000_000


class LlmUnavailable(RuntimeError):
    """Raised when a provider is not configured or a call fails; callers fall back."""


class LlmClient:
    def __init__(self, settings: Settings):
        self.settings = settings
        self._anthropic = (
            anthropic.Anthropic(api_key=settings.anthropic_api_key, max_retries=2, timeout=20.0)
            if settings.anthropic_enabled
            else None
        )
        self._openai = (
            openai.OpenAI(api_key=settings.openai_api_key, max_retries=2, timeout=20.0)
            if settings.openai_enabled
            else None
        )

    @property
    def has_anthropic(self) -> bool:
        return self._anthropic is not None

    @property
    def has_openai(self) -> bool:
        return self._openai is not None

    def parse(
        self,
        *,
        model: str,
        purpose: str,
        system: str,
        user: str,
        output: type[T],
        max_tokens: int = 1024,
        effort: str = "low",
        thinking: bool = False,
    ) -> T:
        """Structured-output call that returns a validated Pydantic object."""
        if self._anthropic is None:
            raise LlmUnavailable("ANTHROPIC_API_KEY is not set")
        kwargs = {}
        if not thinking:
            # Classification-style calls don't benefit from thinking; turning it
            # off keeps them fast and cheap. Haiku accepts this at effort <= high.
            kwargs["thinking"] = {"type": "disabled"}
        started = time.perf_counter()
        try:
            response = self._anthropic.messages.parse(
                model=model,
                max_tokens=max_tokens,
                system=system,
                messages=[{"role": "user", "content": user}],
                output_format=output,
                output_config={"effort": effort},
                **kwargs,
            )
        except anthropic.APIConnectionError as exc:
            LLM_ERRORS.labels("anthropic", purpose, "connection").inc()
            raise LlmUnavailable(str(exc)) from exc
        except anthropic.RateLimitError as exc:
            LLM_ERRORS.labels("anthropic", purpose, "rate_limit").inc()
            raise LlmUnavailable(str(exc)) from exc
        except anthropic.APIStatusError as exc:
            LLM_ERRORS.labels("anthropic", purpose, f"status_{exc.status_code}").inc()
            raise LlmUnavailable(str(exc)) from exc
        self._record("anthropic", model, purpose, response.usage, started)
        if response.stop_reason == "refusal" or response.parsed_output is None:
            LLM_ERRORS.labels("anthropic", purpose, response.stop_reason or "unparsed").inc()
            raise LlmUnavailable(f"no parsed output (stop_reason={response.stop_reason})")
        return response.parsed_output

    def answer(self, *, model: str, purpose: str, system: str, user: str, max_tokens: int = 1024) -> str:
        """Free-text answer. Sonnet 5.5 refusals are re-served by the server-side fallback."""
        if self._anthropic is None:
            raise LlmUnavailable("ANTHROPIC_API_KEY is not set")
        started = time.perf_counter()
        try:
            response = self._anthropic.beta.messages.create(
                model=model,
                max_tokens=max_tokens,
                system=system,
                messages=[{"role": "user", "content": user}],
                output_config={"effort": "low"},
                betas=["server-side-fallback-2026-07-01"],
                fallbacks="default",
            )
        except anthropic.APIConnectionError as exc:
            LLM_ERRORS.labels("anthropic", purpose, "connection").inc()
            raise LlmUnavailable(str(exc)) from exc
        except anthropic.RateLimitError as exc:
            LLM_ERRORS.labels("anthropic", purpose, "rate_limit").inc()
            raise LlmUnavailable(str(exc)) from exc
        except anthropic.APIStatusError as exc:
            LLM_ERRORS.labels("anthropic", purpose, f"status_{exc.status_code}").inc()
            raise LlmUnavailable(str(exc)) from exc
        self._record("anthropic", response.model, purpose, response.usage, started)
        if response.stop_reason == "refusal":
            LLM_ERRORS.labels("anthropic", purpose, "refusal").inc()
            raise LlmUnavailable("model declined the request")
        return "".join(block.text for block in response.content if block.type == "text").strip()

    def embed(self, texts: list[str], purpose: str = "embedding") -> list[list[float]]:
        if self._openai is None:
            raise LlmUnavailable("OPENAI_API_KEY is not set")
        model = self.settings.embedding_model
        started = time.perf_counter()
        try:
            response = self._openai.embeddings.create(model=model, input=texts)
        except openai.APIConnectionError as exc:
            LLM_ERRORS.labels("openai", purpose, "connection").inc()
            raise LlmUnavailable(str(exc)) from exc
        except openai.RateLimitError as exc:
            LLM_ERRORS.labels("openai", purpose, "rate_limit").inc()
            raise LlmUnavailable(str(exc)) from exc
        except openai.APIStatusError as exc:
            LLM_ERRORS.labels("openai", purpose, f"status_{exc.status_code}").inc()
            raise LlmUnavailable(str(exc)) from exc
        tokens = response.usage.prompt_tokens
        record_llm_call(
            "openai", model, purpose, tokens, 0, estimate_cost(model, tokens, 0),
            (time.perf_counter() - started) * 1000,
        )
        return [item.embedding for item in sorted(response.data, key=lambda d: d.index)]

    @staticmethod
    def _record(provider: str, model: str, purpose: str, usage, started: float) -> None:
        input_tokens = usage.input_tokens + (getattr(usage, "cache_read_input_tokens", 0) or 0)
        output_tokens = usage.output_tokens
        record_llm_call(
            provider, model, purpose, input_tokens, output_tokens,
            estimate_cost(model, input_tokens, output_tokens),
            (time.perf_counter() - started) * 1000,
        )
