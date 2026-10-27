"""Offline evaluation harness.

    python -m evals.run routing      # router accuracy, macro-F1, latency, cost
    python -m evals.run retrieval    # recall@k, MRR, nDCG for bm25 / dense / hybrid / +rerank
    python -m evals.run answers      # LLM-judged groundedness of product answers (needs ANTHROPIC_API_KEY)
    python -m evals.run all --gate   # everything, exit non-zero if a threshold in thresholds.json fails

Reports are written to evals/reports/<suite>.json and <suite>.md. Suites that
need a provider key skip the variants they can't run and say so in the report,
so numbers in a report always come from an actual run.
"""

from __future__ import annotations

import argparse
import json
import sys
import time
from datetime import datetime, timezone
from pathlib import Path

from pydantic import BaseModel

from assistant.config import SERVICE_ROOT, Settings
from assistant.datasets import load_retrieval, load_routing
from assistant.llm import LlmUnavailable
from assistant.main import Components, build_components
from assistant.observability import finish_trace, start_trace
from assistant.pipeline import Assistant
from assistant.routing import CascadeRouter
from assistant.schemas import Intent

from .metrics import classification_report, ndcg_at_k, percentile, reciprocal_rank, recall_at_k

REPORT_DIR = SERVICE_ROOT / "evals" / "reports"
THRESHOLDS = SERVICE_ROOT / "evals" / "thresholds.json"


def environment(c: Components) -> dict:
    s = c.settings
    return {
        "run_at": datetime.now(timezone.utc).isoformat(timespec="seconds"),
        "anthropic": c.llm.has_anthropic,
        "openai": c.llm.has_openai,
        "router_model": s.router_model if c.llm.has_anthropic else None,
        "rerank_model": s.rerank_model if c.llm.has_anthropic else None,
        "answer_model": s.answer_model if c.llm.has_anthropic else None,
        "dense_backend": c.assistant.retriever.dense_backend,
        "cascade_threshold": s.cascade_threshold,
    }


def _timed(fn):
    trace = start_trace("eval", "eval")
    started = time.perf_counter()
    result = fn()
    latency_ms = (time.perf_counter() - started) * 1000
    record = finish_trace(trace)
    return result, latency_ms, record


# --------------------------------------------------------------------------- routing

def eval_routing(c: Components) -> dict:
    examples = [e for e in load_routing() if e.split == "test"]
    gold = [e.intent.value for e in examples]
    variants: dict[str, dict] = {}
    skipped = []
    for name in ("rules", "distilled", "cascade", "llm"):
        router = c.routers.get(name)
        if router is None:
            skipped.append({"variant": name, "reason": "ANTHROPIC_API_KEY not set"})
            continue
        if name == "cascade" and isinstance(router, CascadeRouter) and router.teacher is None:
            skipped.append({"variant": name, "reason": "no LLM teacher configured; identical to distilled"})
            continue
        predicted, latencies, costs, sources, errors = [], [], [], [], 0
        for example in examples:
            try:
                decision, latency, record = _timed(lambda: router.route(example.text))
            except LlmUnavailable:
                errors += 1
                predicted.append("error")
                continue
            predicted.append(decision.intent.value)
            sources.append(decision.source)
            latencies.append(latency)
            costs.append(record["cost_usd"])
        report = classification_report(gold, predicted)
        report.update(
            latency_ms_p50=round(percentile(latencies, 50), 3),
            latency_ms_p95=round(percentile(latencies, 95), 3),
            cost_usd_per_call=round(sum(costs) / max(len(costs), 1), 8),
            cost_usd_per_1k_calls=round(1000 * sum(costs) / max(len(costs), 1), 5),
            errors=errors,
        )
        if name == "cascade":
            report["escalation_rate"] = round(sum(s == "cascade:llm" for s in sources) / len(sources), 4)
        variants[name] = report
    return {"suite": "routing", "examples": len(examples), "environment": environment(c),
            "variants": variants, "skipped": skipped}


def routing_markdown(r: dict) -> str:
    lines = [
        f"# Routing eval ({r['examples']} held-out messages, 8 intents)",
        "",
        "| Router | Accuracy (95% CI) | Macro-F1 | p50 ms | p95 ms | $/1k calls | Escalation |",
        "|---|---|---|---|---|---|---|",
    ]
    for name, v in r["variants"].items():
        lo, hi = v["accuracy_ci95"]
        lines.append(
            f"| {name} | {v['accuracy']:.3f} ({lo:.3f}-{hi:.3f}) | {v['macro_f1']:.3f} | {v['latency_ms_p50']:.2f} | "
            f"{v['latency_ms_p95']:.2f} | {v['cost_usd_per_1k_calls']:.4f} | {v.get('escalation_rate', '-')} |"
        )
    lines += _skipped(r)
    for name, v in r["variants"].items():
        lines += ["", f"## Per-intent F1: {name}", "", "| Intent | P | R | F1 | n |", "|---|---|---|---|---|"]
        for label, m in v["per_label"].items():
            if label == "error":
                continue
            lines.append(f"| {label} | {m['precision']:.2f} | {m['recall']:.2f} | {m['f1']:.2f} | {m['support']} |")
    return "\n".join(lines) + "\n"


# --------------------------------------------------------------------------- retrieval

def eval_retrieval(c: Components) -> dict:
    examples = load_retrieval()
    retriever = c.assistant.retriever
    configs = [
        ("bm25", dict(mode="bm25", rerank=False, use_filters=False)),
        ("dense", dict(mode="dense", rerank=False, use_filters=False)),
        ("hybrid", dict(mode="hybrid", rerank=False, use_filters=False)),
        ("hybrid+filters", dict(mode="hybrid", rerank=False, use_filters=True)),
        ("hybrid+filters+rerank", dict(mode="hybrid", rerank=True, use_filters=True)),
    ]
    variants, skipped = {}, []
    for name, kwargs in configs:
        if kwargs["rerank"] and retriever.reranker is None:
            skipped.append({"variant": name, "reason": "ANTHROPIC_API_KEY not set"})
            continue
        rows, latencies, costs = [], [], []
        for example in examples:
            hits, latency, record = _timed(lambda: retriever.search(example.query, top_k=10, **kwargs))
            ranked = [h.product.name for h in hits]
            relevant = set(example.relevant)
            rows.append({
                "kind": example.kind,
                "recall@5": recall_at_k(ranked, relevant, 5),
                "recall@10": recall_at_k(ranked, relevant, 10),
                "hit@1": 1.0 if ranked[:1] and ranked[0] in relevant else 0.0,
                "mrr": reciprocal_rank(ranked, relevant),
                "ndcg@5": ndcg_at_k(ranked, relevant, 5),
            })
            latencies.append(latency)
            costs.append(record["cost_usd"])
        metrics = ("recall@5", "recall@10", "hit@1", "mrr", "ndcg@5")
        summary = {m: round(sum(r[m] for r in rows) / len(rows), 4) for m in metrics}
        by_kind = {}
        for kind in sorted({r["kind"] for r in rows}):
            subset = [r for r in rows if r["kind"] == kind]
            by_kind[kind] = {m: round(sum(r[m] for r in subset) / len(subset), 4) for m in metrics} | {"n": len(subset)}
        summary.update(
            by_kind=by_kind,
            latency_ms_p50=round(percentile(latencies, 50), 3),
            latency_ms_p95=round(percentile(latencies, 95), 3),
            cost_usd_per_1k_queries=round(1000 * sum(costs) / len(costs), 5),
        )
        variants[name] = summary
    return {"suite": "retrieval", "examples": len(examples), "catalog_size": len(retriever.products),
            "environment": environment(c), "variants": variants, "skipped": skipped}


def retrieval_markdown(r: dict) -> str:
    lines = [
        f"# Retrieval eval ({r['examples']} queries over {r['catalog_size']} products, "
        f"dense backend: {r['environment']['dense_backend']})",
        "",
        "| Variant | Recall@5 | Recall@10 | Hit@1 | MRR | nDCG@5 | p50 ms | p95 ms | $/1k queries |",
        "|---|---|---|---|---|---|---|---|---|",
    ]
    for name, v in r["variants"].items():
        lines.append(
            f"| {name} | {v['recall@5']:.3f} | {v['recall@10']:.3f} | {v['hit@1']:.3f} | {v['mrr']:.3f} | "
            f"{v['ndcg@5']:.3f} | {v['latency_ms_p50']:.2f} | {v['latency_ms_p95']:.2f} | {v['cost_usd_per_1k_queries']:.4f} |"
        )
    lines += _skipped(r)
    lines += ["", "## Recall@5 by query type", "", "| Variant | " + " | ".join(
        f"{k} (n={n})" for k, n in _kind_counts(r).items()) + " |", "|---" * (1 + len(_kind_counts(r))) + "|"]
    for name, v in r["variants"].items():
        lines.append(f"| {name} | " + " | ".join(f"{v['by_kind'][k]['recall@5']:.3f}" for k in _kind_counts(r)) + " |")
    return "\n".join(lines) + "\n"


def _kind_counts(r: dict) -> dict:
    first = next(iter(r["variants"].values()))
    return {k: v["n"] for k, v in first["by_kind"].items()}


# --------------------------------------------------------------------------- answers

class Judgement(BaseModel):
    grounded: bool
    answers_question: bool
    reason: str


JUDGE_SYSTEM = """You grade a shopping assistant's answer.
grounded: every factual claim in the answer is supported by the product facts given (saying that
the facts don't cover something is fine). answers_question: the answer addresses what was asked,
or correctly says the information isn't available. Be strict about invented specifications."""


def eval_answers(c: Components) -> dict:
    if not c.llm.has_anthropic:
        return {"suite": "answers", "environment": environment(c), "variants": {},
                "skipped": [{"variant": "answers", "reason": "ANTHROPIC_API_KEY not set"}]}
    assistant: Assistant = c.assistant
    examples = [e for e in load_routing() if e.intent == Intent.product_question]
    rows, latencies, costs = [], [], []
    for example in examples:
        def run():
            decision = assistant.route(example.text, allow_llm=True)
            forced = decision.model_copy(update={"intent": Intent.product_question})
            return assistant._product_question(forced, example.text, allow_llm=True)

        reply, latency, record = _timed(run)
        facts = "\n".join(f"{h.product.name}: {h.product.description} ({h.product.price} ZAR)" for h in reply.products)
        judgement = c.llm.parse(
            model=c.settings.judge_model,
            purpose="judge",
            system=JUDGE_SYSTEM,
            user=f"<facts>{facts}</facts>\n<question>{example.text}</question>\n<answer>{reply.text}</answer>",
            output=Judgement,
            max_tokens=2048,
            effort="medium",
            thinking=True,
        )
        rows.append({"question": example.text, "answer": reply.text, **judgement.model_dump()})
        latencies.append(latency)
        costs.append(record["cost_usd"])
    n = len(rows)
    return {
        "suite": "answers",
        "environment": environment(c) | {"judge_model": c.settings.judge_model},
        "variants": {
            "answer": {
                "grounded_rate": round(sum(r["grounded"] for r in rows) / n, 4),
                "answers_question_rate": round(sum(r["answers_question"] for r in rows) / n, 4),
                "latency_ms_p50": round(percentile(latencies, 50), 1),
                "latency_ms_p95": round(percentile(latencies, 95), 1),
                "cost_usd_per_answer": round(sum(costs) / n, 6),
            }
        },
        "rows": rows,
        "skipped": [],
    }


def answers_markdown(r: dict) -> str:
    lines = ["# Answer quality eval (LLM judge)", ""]
    if r["variants"]:
        v = r["variants"]["answer"]
        lines += [
            f"Judge: `{r['environment']['judge_model']}`, answers: `{r['environment']['answer_model']}`",
            "",
            "| Grounded | Answers question | p50 ms | p95 ms | $/answer (excl. judge) |",
            "|---|---|---|---|---|",
            f"| {v['grounded_rate']:.3f} | {v['answers_question_rate']:.3f} | {v['latency_ms_p50']} | "
            f"{v['latency_ms_p95']} | {v['cost_usd_per_answer']:.6f} |",
        ]
    return "\n".join(lines + _skipped(r)) + "\n"


# --------------------------------------------------------------------------- plumbing

def _skipped(r: dict) -> list[str]:
    if not r.get("skipped"):
        return []
    return ["", "Not run in this report:"] + [f"- `{s['variant']}`: {s['reason']}" for s in r["skipped"]]


SUITES = {
    "routing": (eval_routing, routing_markdown),
    "retrieval": (eval_retrieval, retrieval_markdown),
    "answers": (eval_answers, answers_markdown),
}


def check_gate(results: dict[str, dict]) -> list[str]:
    thresholds = json.loads(THRESHOLDS.read_text(encoding="utf-8"))
    failures = []
    for suite, variants in thresholds.items():
        for variant, metrics in variants.items():
            measured = results.get(suite, {}).get("variants", {}).get(variant)
            if measured is None:
                continue
            for metric, minimum in metrics.items():
                if measured[metric] < minimum:
                    failures.append(f"{suite}/{variant}/{metric} = {measured[metric]} < {minimum}")
    return failures


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("suite", choices=[*SUITES, "all"])
    parser.add_argument("--out", type=Path, default=REPORT_DIR)
    parser.add_argument("--gate", action="store_true", help="fail if a metric is below evals/thresholds.json")
    args = parser.parse_args(argv)

    components = build_components(Settings())
    names = list(SUITES) if args.suite == "all" else [args.suite]
    results = {}
    args.out.mkdir(parents=True, exist_ok=True)
    for name in names:
        run, render = SUITES[name]
        result = run(components)
        results[name] = result
        (args.out / f"{name}.json").write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
        markdown = render(result)
        (args.out / f"{name}.md").write_text(markdown, encoding="utf-8")
        print(markdown)

    if args.gate:
        failures = check_gate(results)
        for failure in failures:
            print(f"GATE FAILED: {failure}", file=sys.stderr)
        return 1 if failures else 0
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
