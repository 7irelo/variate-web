"""Hybrid product search: BM25 + dense, fused with RRF, filtered, then reranked."""

from __future__ import annotations

import logging

from ..catalog import Catalog
from ..llm import LlmUnavailable
from ..observability import span
from ..schemas import SearchHit
from .bm25 import Bm25Index
from .dense import DenseIndex, Embedder
from .filters import Constraints, parse_constraints
from .rerank import LlmReranker

logger = logging.getLogger(__name__)

MODES = ("bm25", "dense", "hybrid")


def reciprocal_rank_fusion(rankings: dict[str, list[int]], k: int) -> list[tuple[int, float]]:
    """Fuse ranked id lists. RRF ignores raw scores, so BM25 and cosine scales never need calibrating."""
    scores: dict[int, float] = {}
    for ranking in rankings.values():
        for rank, doc_id in enumerate(ranking, start=1):
            scores[doc_id] = scores.get(doc_id, 0.0) + 1.0 / (k + rank)
    return sorted(scores.items(), key=lambda kv: kv[1], reverse=True)


class Retriever:
    def __init__(
        self,
        catalog: Catalog,
        embedder: Embedder,
        reranker: LlmReranker | None = None,
        rrf_k: int = 60,
        candidates: int = 12,
    ):
        self.catalog = catalog
        self.products = catalog.products
        documents = [p.document for p in self.products]
        self.bm25 = Bm25Index(documents)
        self.dense = DenseIndex(embedder, documents)
        self.reranker = reranker
        self.rrf_k = rrf_k
        self.candidates = candidates

    @property
    def dense_backend(self) -> str:
        return self.dense.embedder.name

    def search(
        self,
        query: str,
        top_k: int = 5,
        mode: str = "hybrid",
        rerank: bool = True,
        use_filters: bool = True,
    ) -> list[SearchHit]:
        if mode not in MODES:
            raise ValueError(f"mode must be one of {MODES}")
        pool = len(self.products)
        rankings: dict[str, list[int]] = {}
        if mode in ("bm25", "hybrid"):
            with span("retrieval.bm25"):
                rankings["bm25"] = [i for i, _ in self.bm25.search(query, pool)]
        if mode in ("dense", "hybrid"):
            with span("retrieval.dense", backend=self.dense_backend):
                rankings["dense"] = [i for i, _ in self.dense.search(query, pool)]

        if mode == "hybrid":
            fused = reciprocal_rank_fusion(rankings, self.rrf_k)
        else:
            only = rankings[mode]
            fused = [(i, 1.0 / (self.rrf_k + r)) for r, i in enumerate(only, start=1)]

        constraints = parse_constraints(query) if use_filters else Constraints()
        if constraints.active:
            with span("retrieval.filter", max_price=constraints.max_price, min_price=constraints.min_price,
                      on_sale=constraints.on_sale) as s:
                before = len(fused)
                fused = [(i, score) for i, score in fused if constraints.allows(self.products[i])]
                s.attributes["removed"] = before - len(fused)

        stage_ranks = {name: {doc: r for r, doc in enumerate(ids, start=1)} for name, ids in rankings.items()}
        hits = [
            SearchHit(
                product=self.products[i],
                score=round(score, 6),
                stages={name: ranks[i] for name, ranks in stage_ranks.items() if i in ranks},
            )
            for i, score in fused[: max(self.candidates, top_k)]
        ]

        if rerank and self.reranker is not None and len(hits) > 1:
            hits = self._rerank(query, hits)
        return hits[:top_k]

    def _rerank(self, query: str, hits: list[SearchHit]) -> list[SearchHit]:
        with span("retrieval.rerank", candidates=len(hits)) as s:
            try:
                ranked_ids = self.reranker.rerank(query, [h.product for h in hits])
            except LlmUnavailable as exc:
                logger.warning("rerank skipped: %s", exc)
                s.attributes["skipped"] = str(exc)[:120]
                return hits
            s.attributes["kept"] = len(ranked_ids)
        by_id = {h.product.id: h for h in hits}
        reranked = []
        for rank, product_id in enumerate(ranked_ids, start=1):
            hit = by_id.pop(product_id)
            hit.stages["rerank"] = rank
            reranked.append(hit)
        # Candidates the reranker judged irrelevant stay at the back in fused
        # order; callers that want precision keep only hits with a rerank rank.
        return reranked + [h for h in hits if h.product.id in by_id]


__all__ = ["MODES", "Retriever", "reciprocal_rank_fusion", "parse_constraints"]
