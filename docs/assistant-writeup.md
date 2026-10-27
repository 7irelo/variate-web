# Measuring a shopping assistant before trusting it

Variate's assistant does four jobs: it routes a customer message to an intent, finds products, answers questions about them, and looks up orders. This write-up covers how each part is measured, what the numbers say, and where a language model was deliberately left out.

All numbers come from the eval harness in `platform/src/AssistantService/evals` (`python -m evals.run all`). Results that need provider API keys are marked as not yet run rather than estimated.

## The shape of the system

```
message ──► distilled classifier ──confident──► intent
                 │ unsure (p < 0.55)
                 └──► Claude Haiku 5.5 (structured output) ──► intent

product_search ──► BM25 ─┐
                  dense ─┴─► reciprocal rank fusion ──► price/sale filters ──► Haiku rerank ──► results
product_question ──► retrieval ──► Claude Sonnet 5.5 answer, grounded in catalog facts only
order_status ──► order-service API, scoped to the JWT subject (no model)
returns / payments / account / handoff / out of scope ──► fixed, reviewed answers (no model)
```

## Where a model is the wrong tool

Three of the eight intents never reach an LLM after routing, on purpose:

- **Order status is a database lookup.** The answer already exists in the order service. A model could only add latency or invent a delivery date. A model also never picks whose orders are read: the user id comes from the verified JWT.
- **Policy answers must be exact.** Return windows and refund timings are commitments. They are served as fixed text that someone has reviewed, so a model never paraphrases a policy into one the business didn't write.
- **Price and sale constraints are exact filters.** "Under R5 000" has one correct answer. It is parsed with a regular expression and applied as a filter, which took constraint-query recall@5 from 0.958 to 1.000 at zero cost.

A model is used where language understanding matters: routing ambiguous messages, judging whether a product fits a fuzzy need, and phrasing a grounded answer.

## Routing

The eval set has 228 hand-labelled messages across 8 intents. 138 are for training and 90 are held out. The table shows held-out results with 95% bootstrap confidence intervals.

| Router | Accuracy (95% CI) | Macro-F1 | p95 latency | Cost / 1k calls |
|---|---|---|---|---|
| Keyword rules | 0.722 (0.633–0.811) | 0.747 | < 0.1 ms | $0 |
| Distilled classifier (gold labels) | 0.589 (0.489–0.689) | 0.570 | ~2–9 ms | $0 |
| Claude Haiku 5.5 router | not yet run | | | |
| Cascade (classifier → Haiku when unsure) | not yet run | | | |

What these numbers say:

- **The keyword baseline is strong and slightly optimistic.** The rules were written by someone who had seen the dataset, which is a form of leakage. Its 0.72 is a ceiling for hand-written rules here, not a fair estimate on new traffic. Its weak spots are telling: product-search recall is 0.46 (6 of 13 searches fell through to out-of-scope) and out-of-scope precision is 0.33. Rules fail silently on phrasing they didn't anticipate.
- **The distilled classifier is data-limited.** TF-IDF plus logistic regression on 138 examples reaches 0.59. Five-fold cross-validation on the training split puts every reasonable configuration (word or char n-grams, LR or linear SVM, a wide range of regularisation) between 0.57 and 0.65. Tuning won't fix this; more labelled data will. That is exactly what distillation provides: the LLM router labels traffic, and the student learns from those labels (`python -m distill.train --labels teacher`).
- **The cascade is the production design.** The classifier answers when it is confident, and only unsure messages pay for a Haiku call. Its value comes down to two measured numbers: accuracy against the LLM-only router, and the escalation rate, which sets cost per call. Both appear in the report once the eval runs with an API key.

## Product search

The eval set has 50 queries over the 85-product catalog: 15 lexical ("garmin watch"), 27 semantic ("something to keep my beers cold at a braai") and 8 with price or sale constraints. The dense backend in this run is the local LSA index (TF-IDF + truncated SVD). The OpenAI embedding run is pending an API key.

| Variant | Recall@5 | Hit@1 | MRR | nDCG@5 | p95 latency |
|---|---|---|---|---|---|
| BM25 | 0.643 | 0.540 | 0.598 | 0.594 | 0.1 ms |
| Dense (LSA) | 0.797 | 0.580 | 0.680 | 0.696 | ~10 ms |
| Hybrid (RRF) | 0.797 | 0.600 | 0.696 | 0.707 | ~9 ms |
| Hybrid + filters | 0.803 | **0.720** | **0.765** | **0.763** | ~9 ms |
| + Haiku rerank | not yet run | | | | |

Recall@5 by query type:

| Variant | Lexical | Semantic | Constraint |
|---|---|---|---|
| BM25 | 0.967 | 0.407 | 0.833 |
| Hybrid + filters | 0.967 | 0.654 | 1.000 |

What these numbers say:

- **BM25 is nearly perfect on lexical queries but finds only 41% of semantic ones.** A shopper who describes a need instead of naming a product mostly gets nothing back.
- **Fusion improves ranking more than recall.** Hybrid matches dense on recall but moves the right product up (higher hit@1, MRR and nDCG).
- **Deterministic filters gave the biggest single gain in top-1 accuracy:** hit@1 went from 0.60 to 0.72.
- **Semantic recall is still the weakest number at 0.654.** Real embeddings and the reranker are aimed at it. The container smoke test shows the gap concretely: "keep my beers cold at a braai" didn't return the YETI cooler in the top three without them.

## Answer quality

Product answers come from Sonnet 5.5, which is told to use only the retrieved catalog facts and to say when the facts don't answer the question. An Opus 5.5 judge grades every answer for groundedness (no invented specifications) and for whether it answers the question. The CI gate requires a grounded rate of at least 0.95. This suite needs an API key and has not been run yet.

## Observability

Each request emits one JSON trace containing:

- spans for every stage
- token counts, cost and latency for each model call
- the routed intent and which router decided it
- the tenant

The trace never includes the customer's message. Prometheus counters track the same data by tenant, model and purpose: spend, escalations, model errors and budget-driven degradations.

This is what turns "the assistant feels slow" into "p95 is dominated by the rerank span". It also turns "the bill went up" into "the escalation rate rose after the catalog changed".

## Security

- **Identity:** the service verifies JWTs itself and only ever looks up the token subject's orders. Building it surfaced an existing bug in the order service: any signed-in user could read any other user's orders by id. Ownership checks are now enforced there too; another customer's order returns 404, so ids can't be probed.
- **Tenancy:** tenants come from a strict allow-list. The gateway sets the tenant header, and a network policy only admits traffic from the gateway.
- **Prompt injection:** routing and reranking use structured outputs, so a model can only return a known intent or ids from the candidate list. Customer text is never treated as instructions.
- **Cost and abuse:** each tenant has a daily model budget, and each caller has a rate limit. When the budget runs out the service degrades to its non-LLM paths instead of failing.
- **Runtime:** the container is non-root with a read-only root filesystem and no Linux capabilities. Egress is limited to DNS, the order service and HTTPS to public addresses.

## Next measurements

1. Run the routing, retrieval and answer suites with API keys, filling in the Haiku router, cascade, OpenAI dense and rerank rows.
2. Distil from teacher labels over a larger pool of real messages and plot student accuracy against training-set size.
3. Sweep the cascade threshold, plotting accuracy against cost per 1k calls, and pick the operating point from the curve.
4. Replace the in-memory budget ledger and rate limiter with Redis so the limits hold across replicas.
