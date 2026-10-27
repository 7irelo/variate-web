"""Okapi BM25 over the product documents.

The catalog is small enough that an in-memory inverted index is the right
tool; there is no reason to run a search cluster for 85 documents.
"""

from __future__ import annotations

import math
import re
from collections import Counter, defaultdict

STOPWORDS = frozenset(
    "a an and are as at be but by for from has have i in is it its me my of on or our so that the this to "
    "was we what with you your any some something can do does show want need looking".split()
)
_TOKEN = re.compile(r"[a-z0-9]+")


def tokenize(text: str) -> list[str]:
    tokens = []
    for token in _TOKEN.findall(text.casefold()):
        if token in STOPWORDS:
            continue
        # Light plural folding: "guitars" -> "guitar", but keep "glass", "bus".
        if len(token) > 3 and token.endswith("s") and not token.endswith("ss"):
            token = token[:-1]
        tokens.append(token)
    return tokens


class Bm25Index:
    def __init__(self, documents: list[str], k1: float = 1.2, b: float = 0.75):
        self.k1 = k1
        self.b = b
        self.doc_tokens = [tokenize(d) for d in documents]
        self.doc_len = [len(t) for t in self.doc_tokens]
        self.avg_len = sum(self.doc_len) / max(len(self.doc_len), 1)
        self.postings: dict[str, list[tuple[int, int]]] = defaultdict(list)
        for doc_id, tokens in enumerate(self.doc_tokens):
            for term, freq in Counter(tokens).items():
                self.postings[term].append((doc_id, freq))
        n = len(documents)
        self.idf = {
            term: math.log(1 + (n - len(p) + 0.5) / (len(p) + 0.5)) for term, p in self.postings.items()
        }

    def search(self, query: str, limit: int) -> list[tuple[int, float]]:
        scores: dict[int, float] = defaultdict(float)
        for term in set(tokenize(query)):
            idf = self.idf.get(term)
            if idf is None:
                continue
            for doc_id, freq in self.postings[term]:
                norm = self.k1 * (1 - self.b + self.b * self.doc_len[doc_id] / self.avg_len)
                scores[doc_id] += idf * freq * (self.k1 + 1) / (freq + norm)
        return sorted(scores.items(), key=lambda kv: kv[1], reverse=True)[:limit]
