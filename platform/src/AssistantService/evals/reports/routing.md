# Routing eval (90 held-out messages, 8 intents)

| Router | Accuracy (95% CI) | Macro-F1 | p50 ms | p95 ms | $/1k calls | Escalation |
|---|---|---|---|---|---|---|
| rules | 0.722 (0.633-0.811) | 0.747 | 0.02 | 0.03 | 0.0000 | - |
| distilled | 0.589 (0.489-0.689) | 0.570 | 2.05 | 4.54 | 0.0000 | - |
| cascade | 0.967 (0.922-1.000) | 0.967 | 957.24 | 2254.68 | 0.0598 | 0.7111 |
| llm | 0.989 (0.967-1.000) | 0.989 | 1089.83 | 1661.96 | 0.0840 | - |

## Per-intent F1: rules

| Intent | P | R | F1 | n |
|---|---|---|---|---|
| account_help | 1.00 | 0.82 | 0.90 | 11 |
| human_handoff | 1.00 | 0.60 | 0.75 | 10 |
| order_status | 0.69 | 0.82 | 0.75 | 11 |
| out_of_scope | 0.33 | 0.82 | 0.47 | 11 |
| payment_issue | 1.00 | 0.73 | 0.84 | 11 |
| product_question | 0.85 | 0.92 | 0.88 | 12 |
| product_search | 0.86 | 0.46 | 0.60 | 13 |
| returns_refunds | 1.00 | 0.64 | 0.78 | 11 |

## Per-intent F1: distilled

| Intent | P | R | F1 | n |
|---|---|---|---|---|
| account_help | 0.47 | 0.64 | 0.54 | 11 |
| human_handoff | 0.64 | 0.90 | 0.75 | 10 |
| order_status | 0.67 | 0.55 | 0.60 | 11 |
| out_of_scope | 0.50 | 0.27 | 0.35 | 11 |
| payment_issue | 0.70 | 0.64 | 0.67 | 11 |
| product_question | 0.75 | 1.00 | 0.86 | 12 |
| product_search | 0.50 | 0.46 | 0.48 | 13 |
| returns_refunds | 0.38 | 0.27 | 0.32 | 11 |

## Per-intent F1: cascade

| Intent | P | R | F1 | n |
|---|---|---|---|---|
| account_help | 0.92 | 1.00 | 0.96 | 11 |
| human_handoff | 1.00 | 1.00 | 1.00 | 10 |
| order_status | 1.00 | 1.00 | 1.00 | 11 |
| out_of_scope | 1.00 | 0.91 | 0.95 | 11 |
| payment_issue | 1.00 | 1.00 | 1.00 | 11 |
| product_question | 0.92 | 1.00 | 0.96 | 12 |
| product_search | 1.00 | 0.92 | 0.96 | 13 |
| returns_refunds | 0.91 | 0.91 | 0.91 | 11 |

## Per-intent F1: llm

| Intent | P | R | F1 | n |
|---|---|---|---|---|
| account_help | 1.00 | 1.00 | 1.00 | 11 |
| human_handoff | 1.00 | 1.00 | 1.00 | 10 |
| order_status | 1.00 | 1.00 | 1.00 | 11 |
| out_of_scope | 1.00 | 1.00 | 1.00 | 11 |
| payment_issue | 1.00 | 1.00 | 1.00 | 11 |
| product_question | 0.92 | 1.00 | 0.96 | 12 |
| product_search | 1.00 | 1.00 | 1.00 | 13 |
| returns_refunds | 1.00 | 0.91 | 0.95 | 11 |
