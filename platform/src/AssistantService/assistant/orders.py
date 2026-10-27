from __future__ import annotations

import httpx

from .auth import Customer
from .observability import span


class OrderServiceError(RuntimeError):
    pass


class OrderClient:
    def __init__(self, base_url: str, timeout: float = 5.0, transport: httpx.BaseTransport | None = None):
        self._client = httpx.Client(base_url=base_url, timeout=timeout, transport=transport)

    def recent_orders(self, customer: Customer, limit: int = 3) -> list[dict]:
        with span("orders.lookup") as s:
            try:
                response = self._client.get(
                    f"/api/v1/orders/user/{customer.user_id}",
                    headers={"Authorization": f"Bearer {customer.token}"},
                )
            except httpx.HTTPError as exc:
                raise OrderServiceError(str(exc)) from exc
            s.attributes["status"] = response.status_code
            if response.status_code != 200:
                raise OrderServiceError(f"order-service returned {response.status_code}")
            orders = response.json()
        # Defence in depth: never surface an order that isn't the caller's,
        # even if the upstream service returned one.
        return [o for o in orders if str(o.get("userId", customer.user_id)) == customer.user_id][:limit]
