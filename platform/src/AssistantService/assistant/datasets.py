"""Loaders for the labelled eval datasets under evals/datasets."""

from __future__ import annotations

import json
from pathlib import Path

from pydantic import BaseModel, Field

from .config import SERVICE_ROOT
from .schemas import Intent

DATASET_DIR = SERVICE_ROOT / "evals" / "datasets"
ROUTING_PATH = DATASET_DIR / "routing.jsonl"
RETRIEVAL_PATH = DATASET_DIR / "retrieval.jsonl"


class RoutingExample(BaseModel):
    text: str
    intent: Intent
    split: str = Field(pattern="^(train|test)$")


class RetrievalExample(BaseModel):
    query: str
    relevant: list[str] = Field(min_length=1, description="Product names that satisfy the query")
    kind: str = Field(description="lexical | semantic | constraint")


def _read_jsonl(path: Path) -> list[dict]:
    with path.open(encoding="utf-8") as handle:
        return [json.loads(line) for line in handle if line.strip()]


def load_routing(path: Path = ROUTING_PATH) -> list[RoutingExample]:
    return [RoutingExample(**row) for row in _read_jsonl(path)]


def load_retrieval(path: Path = RETRIEVAL_PATH) -> list[RetrievalExample]:
    return [RetrievalExample(**row) for row in _read_jsonl(path)]
