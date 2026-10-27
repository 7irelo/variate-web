"""Small supervised classifier distilled from the LLM router.

TF-IDF over word and character n-grams feeding a logistic regression. It runs
in well under a millisecond on CPU and costs nothing per call, so the cascade
uses it first and only escalates low-confidence messages to the LLM.
"""

from __future__ import annotations

import logging
from pathlib import Path

import joblib
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.pipeline import FeatureUnion, Pipeline

from ..schemas import Intent, RouteDecision

logger = logging.getLogger(__name__)


def build_pipeline() -> Pipeline:
    features = FeatureUnion(
        [
            ("words", TfidfVectorizer(ngram_range=(1, 2), sublinear_tf=True, min_df=1, lowercase=True)),
            ("chars", TfidfVectorizer(analyzer="char_wb", ngram_range=(2, 5), sublinear_tf=True, lowercase=True)),
        ]
    )
    classifier = LogisticRegression(C=10.0, max_iter=2000, class_weight="balanced")
    return Pipeline([("features", features), ("clf", classifier)])


def train(texts: list[str], labels: list[str]) -> Pipeline:
    pipeline = build_pipeline()
    pipeline.fit(texts, labels)
    return pipeline


class DistilledRouter:
    name = "distilled"

    def __init__(self, pipeline: Pipeline):
        self.pipeline = pipeline
        self.classes = list(pipeline.classes_)

    @classmethod
    def load(cls, path: Path, fallback_dataset: Path | None = None) -> "DistilledRouter":
        if path.exists():
            return cls(joblib.load(path)["pipeline"])
        if fallback_dataset is None:
            raise FileNotFoundError(path)
        # No trained artefact yet (fresh checkout or container): fit on the
        # gold training split so the service still starts. `python -m
        # distill.train` produces the real artefact.
        from ..datasets import load_routing

        logger.warning("router model %s missing; training on gold train split", path)
        rows = [r for r in load_routing(fallback_dataset) if r.split == "train"]
        return cls(train([r.text for r in rows], [r.intent.value for r in rows]))

    def save(self, path: Path, meta: dict) -> None:
        path.parent.mkdir(parents=True, exist_ok=True)
        joblib.dump({"pipeline": self.pipeline, "meta": meta}, path)

    def route(self, text: str) -> RouteDecision:
        probabilities = self.pipeline.predict_proba([text])[0]
        best = int(probabilities.argmax())
        return RouteDecision(
            intent=Intent(self.classes[best]),
            confidence=round(float(probabilities[best]), 4),
            source=self.name,
        )
