# Retrieval eval (50 queries over 85 products, dense backend: openai:text-embedding-3-small)

| Variant | Recall@5 | Recall@10 | Hit@1 | MRR | nDCG@5 | p50 ms | p95 ms | $/1k queries |
|---|---|---|---|---|---|---|---|---|
| bm25 | 0.643 | 0.680 | 0.540 | 0.598 | 0.594 | 0.02 | 0.05 | 0.0000 |
| dense | 0.993 | 1.000 | 0.840 | 0.901 | 0.927 | 322.85 | 519.04 | 0.0001 |
| hybrid | 0.943 | 0.970 | 0.680 | 0.792 | 0.825 | 0.20 | 0.62 | 0.0000 |
| hybrid+filters | 0.950 | 0.970 | 0.800 | 0.864 | 0.881 | 0.10 | 0.20 | 0.0000 |
| hybrid+filters+rerank | 0.970 | 0.970 | 0.960 | 0.970 | 0.962 | 1038.90 | 1556.94 | 0.1122 |
| dense+filters | 1.000 | 1.000 | 0.960 | 0.974 | 0.981 | 0.18 | 0.75 | 0.0000 |
| dense+filters+rerank | 1.000 | 1.000 | 0.980 | 0.990 | 0.993 | 1032.94 | 1455.79 | 0.1119 |

## Recall@5 by query type

| Variant | constraint (n=8) | lexical (n=15) | semantic (n=27) |
|---|---|---|---|
| bm25 | 0.833 | 0.967 | 0.407 |
| dense | 0.958 | 1.000 | 1.000 |
| hybrid | 0.958 | 1.000 | 0.907 |
| hybrid+filters | 1.000 | 1.000 | 0.907 |
| hybrid+filters+rerank | 1.000 | 1.000 | 0.944 |
| dense+filters | 1.000 | 1.000 | 1.000 |
| dense+filters+rerank | 1.000 | 1.000 | 1.000 |
