from __future__ import annotations

import math
import random
from collections import Counter, defaultdict


def percentile(values: list[float], pct: float) -> float:
    if not values:
        return 0.0
    ordered = sorted(values)
    index = (len(ordered) - 1) * pct / 100
    low, high = math.floor(index), math.ceil(index)
    return ordered[low] + (ordered[high] - ordered[low]) * (index - low)


def bootstrap_ci(outcomes: list[float], iterations: int = 2000, seed: int = 7) -> tuple[float, float]:
    """95% percentile-bootstrap interval for the mean of per-example outcomes."""
    if not outcomes:
        return (0.0, 0.0)
    rng = random.Random(seed)
    n = len(outcomes)
    means = sorted(sum(rng.choices(outcomes, k=n)) / n for _ in range(iterations))
    return (means[int(0.025 * iterations)], means[int(0.975 * iterations) - 1])


def classification_report(gold: list[str], predicted: list[str]) -> dict:
    labels = sorted(set(gold) | set(predicted))
    confusion: dict[str, Counter] = defaultdict(Counter)
    for g, p in zip(gold, predicted):
        confusion[g][p] += 1
    per_label = {}
    for label in labels:
        tp = confusion[label][label]
        fp = sum(confusion[g][label] for g in labels if g != label)
        fn = sum(confusion[label][p] for p in labels if p != label)
        precision = tp / (tp + fp) if tp + fp else 0.0
        recall = tp / (tp + fn) if tp + fn else 0.0
        f1 = 2 * precision * recall / (precision + recall) if precision + recall else 0.0
        per_label[label] = {"precision": round(precision, 4), "recall": round(recall, 4), "f1": round(f1, 4),
                            "support": sum(confusion[label].values())}
    gold_labels = sorted(set(gold))
    correct = [1.0 if g == p else 0.0 for g, p in zip(gold, predicted)]
    low, high = bootstrap_ci(correct)
    return {
        "accuracy": round(sum(correct) / len(correct), 4),
        "accuracy_ci95": [round(low, 4), round(high, 4)],
        "macro_f1": round(sum(per_label[l]["f1"] for l in gold_labels) / len(gold_labels), 4),
        "per_label": per_label,
        "confusion": {g: dict(confusion[g]) for g in gold_labels},
    }


def recall_at_k(ranked: list[str], relevant: set[str], k: int) -> float:
    return len(set(ranked[:k]) & relevant) / len(relevant)


def reciprocal_rank(ranked: list[str], relevant: set[str]) -> float:
    for rank, item in enumerate(ranked, start=1):
        if item in relevant:
            return 1.0 / rank
    return 0.0


def ndcg_at_k(ranked: list[str], relevant: set[str], k: int) -> float:
    dcg = sum(1.0 / math.log2(rank + 1) for rank, item in enumerate(ranked[:k], start=1) if item in relevant)
    ideal = sum(1.0 / math.log2(rank + 1) for rank in range(1, min(len(relevant), k) + 1))
    return dcg / ideal if ideal else 0.0
