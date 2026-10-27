# Routing eval (90 held-out messages, 8 intents)

| Router | Accuracy (95% CI) | Macro-F1 | p50 ms | p95 ms | $/1k calls | Escalation |
|---|---|---|---|---|---|---|
| rules | 0.722 (0.633-0.811) | 0.747 | 0.01 | 0.01 | 0.0000 | - |
| distilled | 0.589 (0.489-0.689) | 0.570 | 0.93 | 1.63 | 0.0000 | - |

Not run in this report:
- `cascade`: no LLM teacher configured; identical to distilled
- `llm`: ANTHROPIC_API_KEY not set

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
