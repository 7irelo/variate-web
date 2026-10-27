"""Deterministic query constraints.

"Under R3000" and "on sale" are exact constraints with one right answer. They
are parsed with regular expressions and applied as filters, not left for a
model to approximate.
"""

from __future__ import annotations

import re
from dataclasses import dataclass

from ..schemas import Product

_MAX_PRICE = re.compile(
    r"\b(?:under|below|less than|cheaper than|max(?:imum)?|up to|no more than)\s*r\s?(\d[\d\s,]*k?)\b", re.I
)
_MIN_PRICE = re.compile(r"\b(?:over|above|more than|at least)\s*r\s?(\d[\d\s,]*k?)\b", re.I)
_ON_SALE = re.compile(r"\b(on sale|discount(?:ed)?|deals?|specials?)\b", re.I)


def _amount(raw: str) -> int:
    raw = raw.replace(" ", "").replace(",", "").lower()
    return int(raw[:-1]) * 1000 if raw.endswith("k") else int(raw)


@dataclass(frozen=True)
class Constraints:
    max_price: int | None = None
    min_price: int | None = None
    on_sale: bool = False

    @property
    def active(self) -> bool:
        return self.max_price is not None or self.min_price is not None or self.on_sale

    def allows(self, product: Product) -> bool:
        if self.max_price is not None and product.price > self.max_price:
            return False
        if self.min_price is not None and product.price < self.min_price:
            return False
        return not self.on_sale or product.on_sale


def parse_constraints(query: str) -> Constraints:
    max_match = _MAX_PRICE.search(query)
    min_match = _MIN_PRICE.search(query)
    return Constraints(
        max_price=_amount(max_match.group(1)) if max_match else None,
        min_price=_amount(min_match.group(1)) if min_match else None,
        on_sale=bool(_ON_SALE.search(query)),
    )
