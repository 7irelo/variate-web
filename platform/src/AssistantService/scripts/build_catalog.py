"""Build data/catalog.json from the storefront seed script (data.sql).

The seed file stores products in category blocks of five, with a `-- Category`
comment heading each block. Category ids come from the row tuples, category
names from those comments.

Usage: python scripts/build_catalog.py [path/to/data.sql]
"""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

SERVICE_ROOT = Path(__file__).resolve().parents[1]
DEFAULT_SQL = SERVICE_ROOT.parents[2] / "data.sql"
OUTPUT = SERVICE_ROOT / "data" / "catalog.json"

ROW = re.compile(
    r"\((\d+), '((?:[^']|'')*)', '((?:[^']|'')*)', (\d+), '([\d-]+)', '((?:[^']|'')*)', ([01])\)"
)
HEADING = re.compile(r"^--\s*(.+?)\s*$")


def read_sql(path: Path) -> str:
    raw = path.read_bytes()
    try:
        return raw.decode("utf-8")
    except UnicodeDecodeError:
        # The seed file was saved from SSMS as Windows-1252.
        return raw.decode("cp1252")


def parse(sql: str) -> dict:
    categories: dict[int, str] = {}
    products = []
    heading = None
    for line in sql.splitlines():
        line = line.strip()
        if match := HEADING.match(line):
            heading = match.group(1)
            continue
        if not (match := ROW.match(line)):
            continue
        category_id = int(match.group(1))
        if heading:
            categories.setdefault(category_id, heading)
        products.append(
            {
                "id": len(products) + 1,
                "category_id": category_id,
                "name": match.group(2).replace("''", "'"),
                "description": match.group(3).replace("''", "'"),
                "price": int(match.group(4)),
                "release_date": match.group(5),
                "image_url": match.group(6),
                "on_sale": match.group(7) == "1",
            }
        )
    for product in products:
        product["category"] = categories[product["category_id"]]
    return {
        "currency": "ZAR",
        "categories": [{"id": k, "name": v} for k, v in sorted(categories.items())],
        "products": products,
    }


def main() -> None:
    sql_path = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_SQL
    catalog = parse(read_sql(sql_path))
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text(json.dumps(catalog, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"wrote {len(catalog['products'])} products / {len(catalog['categories'])} categories to {OUTPUT}")


if __name__ == "__main__":
    main()
