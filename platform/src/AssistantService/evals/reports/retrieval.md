# Retrieval eval (50 queries over 85 products, dense backend: lsa-local)

| Variant | Recall@5 | Recall@10 | Hit@1 | MRR | nDCG@5 | p50 ms | p95 ms | $/1k queries |
|---|---|---|---|---|---|---|---|---|
| bm25 | 0.643 | 0.680 | 0.540 | 0.598 | 0.594 | 0.03 | 0.04 | 0.0000 |
| dense | 0.797 | 0.840 | 0.580 | 0.680 | 0.696 | 2.23 | 3.46 | 0.0000 |
| hybrid | 0.797 | 0.840 | 0.600 | 0.696 | 0.707 | 2.30 | 2.90 | 0.0000 |
| hybrid+filters | 0.803 | 0.840 | 0.720 | 0.765 | 0.763 | 2.45 | 3.22 | 0.0000 |

Not run in this report:
- `hybrid+filters+rerank`: ANTHROPIC_API_KEY not set

## Recall@5 by query type

| Variant | constraint (n=8) | lexical (n=15) | semantic (n=27) |
|---|---|---|---|
| bm25 | 0.833 | 0.967 | 0.407 |
| dense | 0.958 | 0.967 | 0.654 |
| hybrid | 0.958 | 0.967 | 0.654 |
| hybrid+filters | 1.000 | 0.967 | 0.654 |
