"""Dense retrieval backends.

`OpenAIEmbedder` is the production backend. `LsaEmbedder` (TF-IDF + truncated
SVD) is a local stand-in so the service, tests and evals run without network
access; it is reported separately in the evals and is not a substitute for a
real embedding model on semantic queries.
"""

from __future__ import annotations

import hashlib
import logging
from functools import lru_cache
from pathlib import Path
from typing import Protocol

import numpy as np
from sklearn.decomposition import TruncatedSVD
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.pipeline import FeatureUnion

from ..llm import LlmClient

logger = logging.getLogger(__name__)


def _normalise(matrix: np.ndarray) -> np.ndarray:
    norms = np.linalg.norm(matrix, axis=1, keepdims=True)
    return matrix / np.where(norms == 0, 1, norms)


class Embedder(Protocol):
    name: str

    def embed_documents(self, documents: list[str]) -> np.ndarray: ...

    def embed_query(self, query: str) -> np.ndarray: ...


class LsaEmbedder:
    name = "lsa-local"

    def __init__(self, dimensions: int = 64):
        self.dimensions = dimensions
        self.vectorizer = FeatureUnion(
            [
                ("words", TfidfVectorizer(ngram_range=(1, 2), sublinear_tf=True, stop_words="english")),
                ("chars", TfidfVectorizer(analyzer="char_wb", ngram_range=(3, 5), sublinear_tf=True)),
            ]
        )
        self.svd: TruncatedSVD | None = None

    def embed_documents(self, documents: list[str]) -> np.ndarray:
        tfidf = self.vectorizer.fit_transform(documents)
        self.svd = TruncatedSVD(n_components=min(self.dimensions, tfidf.shape[0] - 1), random_state=7)
        return _normalise(self.svd.fit_transform(tfidf))

    def embed_query(self, query: str) -> np.ndarray:
        assert self.svd is not None, "embed_documents must run first"
        return _normalise(self.svd.transform(self.vectorizer.transform([query])))[0]


class OpenAIEmbedder:
    def __init__(self, llm: LlmClient, cache_path: Path | None):
        self.llm = llm
        self.cache_path = cache_path
        self.name = f"openai:{llm.settings.embedding_model}"
        self._query = lru_cache(maxsize=2048)(self._embed_query_uncached)

    def embed_documents(self, documents: list[str]) -> np.ndarray:
        key = hashlib.sha256(("\n".join(documents) + self.name).encode()).hexdigest()
        if self.cache_path and self.cache_path.exists():
            cached = np.load(self.cache_path)
            if str(cached["key"]) == key:
                return cached["vectors"]
        vectors = _normalise(np.asarray(self.llm.embed(documents, purpose="index"), dtype=np.float32))
        if self.cache_path:
            self.cache_path.parent.mkdir(parents=True, exist_ok=True)
            np.savez(self.cache_path, key=key, vectors=vectors)
        return vectors

    def _embed_query_uncached(self, query: str) -> np.ndarray:
        return _normalise(np.asarray(self.llm.embed([query], purpose="query"), dtype=np.float32))[0]

    def embed_query(self, query: str) -> np.ndarray:
        return self._query(query)


class DenseIndex:
    def __init__(self, embedder: Embedder, documents: list[str]):
        self.embedder = embedder
        self.vectors = embedder.embed_documents(documents)

    def search(self, query: str, limit: int) -> list[tuple[int, float]]:
        scores = self.vectors @ self.embedder.embed_query(query)
        order = np.argsort(-scores)[:limit]
        return [(int(i), float(scores[i])) for i in order]
