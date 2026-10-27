"""Distil the LLM router into the small classifier the cascade runs first.

    python -m distill.train              # teacher labels from the LLM router (needs ANTHROPIC_API_KEY)
    python -m distill.train --labels gold  # supervised on the hand-labelled train split instead

With teacher labels, the student never sees a human label: the LLM router
labels the training messages and the student learns from those. Both students
are scored on the same held-out gold test split, so the report shows how much
accuracy distillation keeps and what it costs to produce.
"""

from __future__ import annotations

import argparse
import json
import time

from assistant.config import SERVICE_ROOT, Settings
from assistant.datasets import load_routing
from assistant.llm import LlmClient, LlmUnavailable
from assistant.observability import finish_trace, start_trace
from assistant.routing import DistilledRouter, LlmRouter
from assistant.routing.distilled import train
from evals.metrics import classification_report

REPORT = SERVICE_ROOT / "evals" / "reports" / "distillation.json"


def teacher_labels(texts: list[str], settings: Settings) -> tuple[list[str], float, int]:
    llm = LlmClient(settings)
    if not llm.has_anthropic:
        raise SystemExit("teacher labelling needs ANTHROPIC_API_KEY; use --labels gold to train without it")
    teacher = LlmRouter(llm, settings.router_model)
    labels, cost, failures = [], 0.0, 0
    for text in texts:
        trace = start_trace("distill", "distill")
        try:
            labels.append(teacher.route(text).intent.value)
        except LlmUnavailable:
            labels.append("")
            failures += 1
        cost += finish_trace(trace)["cost_usd"]
    return labels, cost, failures


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--labels", choices=["teacher", "gold"], default="teacher")
    args = parser.parse_args()
    settings = Settings()

    data = load_routing()
    train_rows = [r for r in data if r.split == "train"]
    test_rows = [r for r in data if r.split == "test"]
    texts = [r.text for r in train_rows]
    gold_train = [r.intent.value for r in train_rows]

    meta: dict = {"labels": args.labels, "train_examples": len(texts)}
    if args.labels == "teacher":
        labels, cost, failures = teacher_labels(texts, settings)
        keep = [i for i, label in enumerate(labels) if label]
        agreement = sum(labels[i] == gold_train[i] for i in keep) / len(keep)
        texts, labels = [texts[i] for i in keep], [labels[i] for i in keep]
        meta.update(teacher_model=settings.router_model, labelling_cost_usd=round(cost, 6),
                    teacher_failures=failures, teacher_gold_agreement=round(agreement, 4))
    else:
        labels = gold_train

    started = time.perf_counter()
    student = DistilledRouter(train(texts, labels))
    meta["train_seconds"] = round(time.perf_counter() - started, 3)
    predicted = [student.route(r.text).intent.value for r in test_rows]
    meta["test"] = classification_report([r.intent.value for r in test_rows], predicted)

    student.save(settings.router_model_path, meta)
    REPORT.parent.mkdir(parents=True, exist_ok=True)
    REPORT.write_text(json.dumps(meta, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({k: v for k, v in meta.items() if k != "test"} | {
        "test_accuracy": meta["test"]["accuracy"], "test_macro_f1": meta["test"]["macro_f1"]}, indent=2))


if __name__ == "__main__":
    main()
