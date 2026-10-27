from __future__ import annotations

import json
from pathlib import Path

from .schemas import Product


class Catalog:
    def __init__(self, products: list[Product], currency: str = "ZAR"):
        self.products = products
        self.currency = currency
        self._by_id = {p.id: p for p in products}

    @classmethod
    def load(cls, path: Path) -> "Catalog":
        data = json.loads(path.read_text(encoding="utf-8"))
        return cls([Product(**p) for p in data["products"]], data.get("currency", "ZAR"))

    def get(self, product_id: int) -> Product:
        return self._by_id[product_id]

    def find_by_name(self, name: str) -> Product | None:
        needle = name.casefold()
        return next((p for p in self.products if p.name.casefold() == needle), None)

    def __len__(self) -> int:
        return len(self.products)


def format_price(amount: int) -> str:
    return f"R{amount:,}".replace(",", " ")
