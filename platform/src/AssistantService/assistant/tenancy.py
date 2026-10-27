"""Tenant validation and per-tenant daily LLM budgets.

Each request names its tenant in the X-Tenant-Id header. Tenants are labels on
every trace and metric, and each has a daily model-spend budget. When a tenant
has spent its budget the assistant keeps working without LLM calls (distilled
routing, unreranked search, templated answers) rather than failing.

Budgets are tracked in process memory, so with N replicas the effective cap is
up to N times the configured value. Move the ledger to Redis before relying on
it as a hard limit.
"""

from __future__ import annotations

import re
import threading
import time
from datetime import date, datetime, timezone

_TENANT = re.compile(r"^[a-z0-9][a-z0-9-]{1,40}$")


class TenantError(ValueError):
    pass


class BudgetLedger:
    def __init__(self, daily_budget_usd: float):
        self.daily_budget_usd = daily_budget_usd
        self._spend: dict[tuple[str, date], float] = {}
        self._lock = threading.Lock()

    @staticmethod
    def _today() -> date:
        return datetime.now(timezone.utc).date()

    def spent(self, tenant: str) -> float:
        with self._lock:
            return self._spend.get((tenant, self._today()), 0.0)

    def allows_llm(self, tenant: str) -> bool:
        return self.spent(tenant) < self.daily_budget_usd

    def add(self, tenant: str, cost_usd: float) -> None:
        if cost_usd <= 0:
            return
        key = (tenant, self._today())
        with self._lock:
            self._spend[key] = self._spend.get(key, 0.0) + cost_usd


def validate_tenant(raw: str | None, allowed: list[str]) -> str:
    tenant = (raw or "").strip().lower()
    if not _TENANT.match(tenant):
        raise TenantError("X-Tenant-Id header is missing or malformed")
    if tenant not in allowed:
        raise TenantError(f"unknown tenant '{tenant}'")
    return tenant


class RateLimiter:
    """Sliding one-minute window per key (tenant + client address).

    Like the budget ledger this is per process; the ingress rate limit is the
    first line of defence and this one stops a single noisy caller from
    spending a tenant's model budget.
    """

    def __init__(self, per_minute: int):
        self.per_minute = per_minute
        self._hits: dict[str, list[float]] = {}
        self._lock = threading.Lock()

    def allow(self, key: str, now: float | None = None) -> bool:
        now = time.monotonic() if now is None else now
        with self._lock:
            window = [t for t in self._hits.get(key, []) if now - t < 60]
            if len(window) >= self.per_minute:
                self._hits[key] = window
                return False
            window.append(now)
            self._hits[key] = window
            return True
