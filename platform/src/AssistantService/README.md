# Assistant Service

Python (FastAPI + Pydantic) shopping assistant for Variate. It routes customer messages to an intent, searches the catalog with hybrid retrieval, answers product questions from catalog facts, and looks up the signed-in customer's orders. Every request is traced, costed and counted.

The design write-up with measured results is in [docs/assistant-writeup.md](../../../docs/assistant-writeup.md).

## Endpoints

| Method | Path | Purpose |
|---|---|---|
| POST | `/api/v1/assistant/chat` | Route a message and reply (`{"message": "..."}`) |
| POST | `/api/v1/assistant/search` | Hybrid product search (`{"query": "...", "top_k": 5, "rerank": true}`) |
| POST | `/api/v1/assistant/route` | Route only; `strategy` can be `rules`, `distilled`, `cascade` or `llm` |
| GET | `/health` | Liveness and active backends |
| GET | `/metrics` | Prometheus metrics (cluster-internal; not routed by the gateway) |

All `/api/v1/assistant/*` calls need an `X-Tenant-Id` header. Behind the gateway it is set by the gateway, so clients can't choose it. Order lookups also need `Authorization: Bearer <jwt>` from the auth service.

## Models

| Job | Model | Why |
|---|---|---|
| Intent routing (cascade escalation) | `claude-haiku-5-5`, thinking off, low effort | Runs on every unsure message, so it needs to be the cheapest and fastest model. Structured output keeps it to a valid intent. |
| Search reranking | `claude-haiku-5-5` | Short list-ranking calls; output limited to candidate ids. |
| Product answers | `claude-sonnet-5-5`, low effort, server-side refusal fallback | Customer-facing text has to stay grounded and well phrased. |
| Eval judge | `claude-opus-5-5` | Runs offline only, where the grader's reliability matters more than its cost. |
| Embeddings | OpenAI `text-embedding-3-small` | Anthropic has no embeddings endpoint. |

Every model is configurable (`ASSISTANT_ROUTER_MODEL`, `ASSISTANT_ANSWER_MODEL`, ...). With no keys set, the service still runs fully offline: distilled routing, BM25 plus a local LSA dense index, and templated answers.

## Run

```bash
pip install -r requirements-dev.txt
python scripts/build_catalog.py            # regenerate data/catalog.json from ../../../data.sql
python -m distill.train --labels gold      # or --labels teacher with ANTHROPIC_API_KEY
uvicorn assistant.main:app --port 8080
python -m pytest -q
```

Docker: `docker compose up assistant-service` from `platform/` (exposed on port 7005, or through the gateway on 7000).

## Evals

```bash
python -m evals.run routing      # accuracy + 95% bootstrap CI, macro-F1, per-intent F1, p50/p95 latency, $/1k calls
python -m evals.run retrieval    # recall@5/10, hit@1, MRR, nDCG@5 for bm25, dense, hybrid, +filters, +rerank
python -m evals.run answers      # groundedness judged by Opus (needs ANTHROPIC_API_KEY)
python -m evals.run all --gate   # fail on regression below evals/thresholds.json (runs in CI)
```

The datasets are in `evals/datasets/`: 228 labelled routing messages (138 train / 90 held-out test) and 50 retrieval queries split into lexical, semantic and constraint types. Reports go to `evals/reports/`. Any variant that needs a missing key is listed as "not run", never estimated.

## Observability

- **Traces:** one JSON log line per request with spans (routing, bm25, dense, filter, rerank, answer, orders.lookup), every model call's tokens, cost and latency, and the trace id (taken from an incoming W3C `traceparent` when present). The trace id is also returned in `X-Trace-Id`. Set `ASSISTANT_TRACE_SINK_PATH` to also append traces to a JSONL file.
- **Metrics:**
  - `assistant_requests_total{tenant,endpoint,intent,route_source}`
  - `assistant_request_latency_seconds`
  - `assistant_stage_latency_seconds{stage}`
  - `assistant_llm_tokens_total`
  - `assistant_llm_cost_usd_total{tenant,model,purpose}`
  - `assistant_llm_errors_total`
  - `assistant_router_escalations_total`
  - `assistant_budget_degradations_total`

Traces never contain the customer's message text.

## Security

- **Identity:** the JWT is verified in the service (HS256 pinned; issuer, audience, `exp` and `sub` required). Orders are only fetched for the token's `sub`, and results are filtered again by owner. The model never chooses whose data is read.
- **Tenancy:** a strict tenant allow-list. The gateway sets `X-Tenant-Id`, and a NetworkPolicy only admits traffic from gateway pods.
- **Prompt injection:** customer text is passed as delimited data. Routing and reranking use structured outputs, so the model can only return a known intent or ids from the candidate list. Policy answers (returns, payments, account) are fixed text, not generated.
- **Abuse and cost:** per-tenant, per-caller rate limit (keyed on the socket peer, not the spoofable `X-Forwarded-For`) and a daily per-tenant model budget. When the budget is spent, the service degrades to non-LLM paths instead of failing.
- **Production guardrails:** with `ASSISTANT_ENVIRONMENT=production` the service refuses to start with the placeholder signing key, a key shorter than 32 characters, or the API docs enabled. Docs are off by default.
- **Runtime:**
  - the container runs as a non-root user (uid 10001) on a slim base image with pinned dependencies
  - Kubernetes runs it with a read-only root filesystem, all capabilities dropped, no privilege escalation, seccomp `RuntimeDefault` and no service-account token
  - egress is limited to DNS, the order service and HTTPS to public addresses; private ranges and the cloud metadata IP are blocked
- **Secrets:** provider keys come from the Kubernetes secret or `.env` (git-ignored). CI only passes them to the eval step, and pull requests from forks never receive them.
