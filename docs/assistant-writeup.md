# Measuring a shopping assistant before trusting it

Variate's assistant does four jobs: it routes a customer message to an intent, finds products, answers questions about them, and looks up orders. This write-up covers how each part is measured, what the numbers show, the bugs the evals caught, and where a language model was deliberately left out.

All numbers come from the eval harness in `platform/src/AssistantService/evals` (`python -m evals.run all`), run on 2026-10-27 with Claude and OpenAI embeddings.

## The shape of the system

```
message ──► distilled classifier ──confident──► intent
                 │ unsure (p < 0.55)
                 └──► Claude Haiku 5.5 (structured output) ──► intent

product_search ──► OpenAI embeddings ──► price/sale filters ──► results
                  (no embeddings: BM25 + LSA ─► RRF ─► filters ─► Haiku rerank)
product_question ──► retrieval ──► Claude Sonnet 5.5 answer, grounded in catalog facts only
order_status ──► order-service API, scoped to the JWT subject (no model)
returns / payments / account / handoff / out of scope ──► fixed, reviewed answers (no model)
```

## Where a model is the wrong tool

Three of the eight intents never reach an LLM after routing, on purpose:

- **Order status is a database lookup.** The answer already exists in the order service; a model could only add latency or invent a delivery date. The user id comes from the verified JWT, never from the model.
- **Policy answers must be exact.** Return windows and refund timings are commitments. They are served as fixed text that someone has reviewed, so no model can paraphrase them into a policy the business didn't write.
- **Price and sale constraints are exact filters.** "Under R5 000" has one correct answer, so it is parsed with a regular expression and applied as a filter. That took constraint-query recall@5 from 0.958 to 1.000 at zero cost.

## Routing

The eval set has 228 hand-labelled messages across 8 intents: 138 for training and 90 held out. The table shows held-out results with 95% bootstrap confidence intervals.

| Router | Accuracy (95% CI) | Macro-F1 | p50 latency | p95 latency | Cost / 1k calls |
|---|---|---|---|---|---|
| Keyword rules | 0.722 (0.633–0.811) | 0.747 | 0.02 ms | 0.03 ms | $0 |
| Distilled classifier | 0.589 (0.489–0.689) | 0.570 | 2 ms | 5 ms | $0 |
| Claude Haiku 5.5 | **0.989** (0.967–1.000) | 0.989 | 1.09 s | 1.66 s | $0.084 |
| Cascade (classifier → Haiku when unsure) | 0.967 (0.922–1.000) | 0.967 | 0.96 s | 2.25 s | $0.060 |

What the numbers show:

- **Haiku is close to perfect and still cheap.** One misrouted message out of 90, at about 8 cents per thousand messages. The cost of a routing mistake (a refund request treated as a product search) is far higher than that.
- **The cascade only saves 29% because the student is weak.** At the 0.55 threshold it escalates 71% of messages, so it pays for most Haiku calls anyway and gives up 2.2 points of accuracy. The overlapping confidence intervals mean that accuracy gap isn't significant on 90 examples, but the direction is consistent.
- **Teacher labels weren't the bottleneck.** Haiku labelled the 138 training messages for $0.012 and agreed with the human labels 98.6% of the time. A student trained on those labels scores exactly the same 0.589 as one trained on the human labels, and cross-validation across feature and model choices stays between 0.57 and 0.65. The student needs more data, not better labels: distilling from a few thousand real customer messages is the next step, and the cascade isn't worth turning on until then.
- **The keyword baseline is optimistic.** The rules were written by someone who had seen the dataset, so 0.72 is a ceiling for hand-written rules here, not an estimate on new traffic. Its failures are silent: 6 of 13 product searches fell through to out-of-scope.

**Decision:** route with Haiku directly (`ASSISTANT_ROUTER_STRATEGY=llm`) until the student is retrained on more data. Keep the cascade code and its eval, because they will show when the student is good enough to switch.

## Product search

The eval set has 50 queries over the 85-product catalog: 15 lexical ("garmin watch"), 27 semantic ("something to keep my beers cold at a braai") and 8 with price or sale constraints. Each pipeline was run against two dense backends: OpenAI `text-embedding-3-small`, and a local TF-IDF + SVD (LSA) index used when no embedding provider is available.

**With OpenAI embeddings:**

| Variant | Recall@5 | Hit@1 | MRR | nDCG@5 | Cost / 1k queries |
|---|---|---|---|---|---|
| BM25 | 0.643 | 0.540 | 0.598 | 0.594 | $0 |
| Dense | 0.993 | 0.840 | 0.901 | 0.927 | $0.0001 |
| Hybrid (BM25 + dense, RRF) | 0.943 | 0.680 | 0.792 | 0.825 | $0.0001 |
| Hybrid + filters | 0.950 | 0.800 | 0.864 | 0.881 | $0.0001 |
| Hybrid + filters + Haiku rerank | 0.970 | 0.960 | 0.970 | 0.962 | $0.112 |
| **Dense + filters** | **1.000** | **0.960** | **0.974** | **0.981** | **$0.0001** |
| Dense + filters + Haiku rerank | 1.000 | 0.980 | 0.990 | 0.993 | $0.112 |

**With the local LSA index:**

| Variant | Recall@5 | Hit@1 | MRR | nDCG@5 | Cost / 1k queries |
|---|---|---|---|---|---|
| Dense | 0.797 | 0.580 | 0.680 | 0.696 | $0 |
| Hybrid | 0.797 | 0.600 | 0.696 | 0.707 | $0 |
| Hybrid + filters | 0.803 | 0.720 | 0.765 | 0.763 | $0 |
| **Hybrid + filters + Haiku rerank** | **0.840** | **0.860** | **0.860** | **0.845** | $0.111 |

What the numbers show:

- **Hybrid search lost to dense search on this catalog.** With a strong embedding model, fusing in BM25 lowered recall@5 from 0.993 to 0.943 and hit@1 from 0.84 to 0.68. BM25 only finds 41% of the semantic queries, so RRF keeps promoting its wrong matches. Hybrid search is the common default, and the eval says it is the wrong one here.
- **Reranking is worth paying for only when retrieval is weak.** On top of real embeddings it adds 0.02 hit@1 for about 1 s of latency and 11 cents per thousand searches. On the local index it lifts hit@1 from 0.72 to 0.86.
- **The filters pay off with either backend.** Exact price and sale filters bring constraint queries to 1.000 and raise hit@1 every time, at no cost.
- **Latency:** an uncached query embedding costs about 320 ms at p50. Reranking adds about 1.0–1.3 s. Cached repeat queries are sub-millisecond, which is why some rows in the raw report look free.

**Decision:** `retrieval_mode=auto` and `rerank=auto` pick the measured winner for whichever backend is active: dense + filters with no rerank when OpenAI embeddings are available, and hybrid + filters + rerank on the local fallback.

**Caveat:** with OpenAI embeddings this eval set is close to saturated (recall@5 of 1.000). It can't separate the top variants any further, and the 85-product catalog is small. The next step is harder queries and a larger catalog.

## Answer quality, and the bugs the eval caught

Product answers come from Sonnet 5.5, which is told to use only the retrieved catalog facts. An Opus 5.5 judge grades each of 30 questions for groundedness (no claim beyond the facts) and for whether it answers the question. CI requires a grounded rate of at least 0.95.

| Run | Grounded | Answers question | Cause |
|---|---|---|---|
| 1 | 0.300 | 1.000 | Harness bug: the judge saw fewer facts (no sale flag or category) than the answer model, so correct "it's on sale" claims were marked as invented |
| 2 | 0.933 | 1.000 | Two real failures, below |
| 3 | **1.000** | **1.000** | p50 1.67 s, $0.0019 per answer |

Run 1 is a reminder that the eval is code too. The fix was to give the answer model and the judge the same facts from one function, so they can't drift apart again.

Run 2 found two genuine problems:

- **A filter bug.** "Is the Rolex Submariner on special?" triggered the on-sale filter, which removed the (not on sale) Rolex before the model saw it. Question answering now retrieves without shopping filters.
- **Outside knowledge.** Sonnet added true-but-unsourced claims ("the PS5 comes in different editions"). On a store's assistant, anything the model says reads as a promise from the store, so the instruction now rules out general brand knowledge.

One known retrieval gap remains: "PS5" doesn't match "PlayStation 5" in this catalog. The answer correctly says it can't find the product, but a customer would expect it to.

## Observability

Each request emits one JSON trace containing:

- spans for every stage
- token counts, cost and latency for each model call
- the routed intent and which router decided it
- the tenant

The trace never includes the customer's message. Prometheus counters track the same data by tenant, model and purpose: spend, escalations, model errors and budget-driven degradations.

The numbers above came from the same instrumentation that runs in production; the eval harness reads cost from the traces.

## Security

- **Identity:** the service verifies JWTs itself and only ever looks up the token subject's orders. Building it surfaced an existing bug in the order service: any signed-in user could read or cancel another user's orders by id. Ownership checks are now enforced there too, and another customer's order returns 404 so ids can't be probed.
- **Tenancy:** tenants come from a strict allow-list. The gateway sets the tenant header, and a network policy only admits traffic from the gateway.
- **Prompt injection:** routing and reranking use structured outputs, so a model can only return a known intent or ids from the candidate list.
- **Cost and abuse:** each tenant has a daily model budget and each caller has a rate limit. When a provider fails, the service degrades to its non-LLM paths rather than failing. The first run with real keys hit this case: an OpenAI account with no credit crashed startup, and the service now falls back to the local index.
- **Runtime:** the container runs as non-root with a read-only root filesystem and no Linux capabilities, and egress is limited to DNS, the order service and HTTPS to public addresses.

## Next measurements

1. Build a harder retrieval set on a larger catalog; the current one is saturated with real embeddings.
2. Distil the student from a larger pool of real messages, then sweep the cascade threshold (accuracy against cost per 1k calls) to see whether the cascade ever beats routing everything through Haiku.
3. Add catalog aliases ("PS5") and measure their effect on retrieval.
4. Move the budget ledger and rate limiter to Redis so the limits hold across replicas.
