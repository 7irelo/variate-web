# Variate

Variate is a South African online store, built as three parts that run together:

| Part | What it is | Stack |
| --- | --- | --- |
| **Storefront** | The shop customers use: browse, cart, checkout with Paystack, orders, accounts | ASP.NET Core 8 MVC, EF Core, SQL Server, ASP.NET Core Identity |
| **Platform** | The same domain split into services for Azure Kubernetes Service | .NET 8 minimal APIs, RabbitMQ, Docker, Kubernetes, Terraform, Ansible |
| **Shopping assistant** | A chat service that finds products, answers questions about them and looks up orders | Python, FastAPI, Pydantic, Claude, OpenAI embeddings, Prometheus |

![variate_home](https://github.com/user-attachments/assets/baf442f2-db0e-40a8-a34b-94c522966dd7)
![variate_electronics](https://github.com/user-attachments/assets/81f1ae15-f118-4612-aa9d-a7bcaeacc8f8)
![variate_product](https://github.com/user-attachments/assets/bec81fcc-4979-4911-89c2-26737f847026)
![variate_reviews](https://github.com/user-attachments/assets/bfa0190b-5c48-4b96-a47c-d29eac3bd9a7)
![variate_cart](https://github.com/user-attachments/assets/b8338bb9-543c-4147-a317-a6b6a4956a2c)
![variate_register](https://github.com/user-attachments/assets/935dcea1-7312-4d5f-8fe6-faf470020e0b)

## Quick start

You need [Docker](https://www.docker.com/products/docker-desktop/).

```bash
git clone https://github.com/7irelo/variate-web.git
cd variate-web/platform
cp .env.example .env          # then fill in the values
docker compose up -d --build
```

| URL | Service |
| --- | --- |
| http://localhost:7010 | Storefront |
| http://localhost:7000 | API gateway for the platform services |
| http://localhost:15672 | RabbitMQ management |

The storefront creates its database on first start. To load the full catalogue of 65 products:

```bash
docker exec -i variate-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "<MSSQL_SA_PASSWORD>" \
  -d variate_legacy < infra/sql/seed-storefront-products.sql
```

The script is idempotent, so running it twice changes nothing.

## Storefront

Customers can:

- browse eight categories (Electronics, Home and Kitchen, Fashion and Beauty, Musical Instruments, Art and Crafts, Baby and Toddler, Bed and Bath, Decor and Furniture), with sale prices shown in rand
- keep a cart that survives sign-out, with a live item count in the header
- pay on Paystack's hosted page, so card details never touch this server
- see their own orders and each order's status
- manage their account: profile, password, two-factor authentication and personal data

Admins can add and edit products and categories and see every order.

### How payment works

1. Checkout snapshots the cart into an order marked `PendingPayment`, at the prices shown in the cart, and asks Paystack for a payment page with a reference unique to that order.
2. The shopper pays on Paystack and is sent back to `/checkout/callback`.
3. The server asks Paystack's API whether the payment succeeded and checks that the reference, amount, currency and customer all match the order. Nothing in the redirect URL is trusted.
4. In one database transaction the order becomes `Paid`, a payment record is written and the cart is closed. The update only applies to an order that is still pending, so a repeated callback, a webhook and a refresh can't record the same payment twice.
5. Paystack also calls `/checkout/paystack/webhook`. The body must carry a valid HMAC-SHA512 signature, compared in constant time, or it is rejected. This confirms payments even when the shopper closes the tab before the redirect.

### Security

- Every form that changes data carries an anti-forgery token.
- Customers can only open their own orders. Anyone else's order returns 404, not 403, so order numbers can't be probed.
- Product and category changes require the `admin` role.
- New accounts must confirm their email before signing in. Outside Development the app refuses to start without SMTP settings, so it can't fall back to showing confirmation links on the page.
- Secrets are read from environment variables and never committed.

## Platform

[`platform/`](platform/README.md) splits the store into independently deployable services:

- **auth-service**: registration, login and JWT issuing
- **catalog-service**: products, categories and stock
- **order-service**: checkout and order lifecycle; publishes `order.created` and `payment.requested`
- **payment-service**: payment processing; publishes `payment.completed`
- **notification-service**: reacts to order and payment events
- **assistant-service**: the shopping assistant
- **gateway-service**: the single entry point at `/api/v1/*`

Services talk over HTTP through the gateway and over a RabbitMQ topic exchange for events. `platform/k8s` has the Kubernetes manifests for AKS, `platform/infra/terraform` provisions AKS, ACR and networking on Azure, and `platform/infra/ansible` builds, pushes and deploys the images.

## Shopping assistant

[`platform/src/AssistantService`](platform/src/AssistantService/README.md) handles four kinds of request: product search, product questions, order status and store policy. Each request is routed, answered by the cheapest path that is reliable for it, and measured.

| Stage | Approach | Measured result |
| --- | --- | --- |
| Routing | Claude Haiku 5.5 with structured output | 0.989 accuracy on held-out messages, $0.084 per 1,000 |
| Search | OpenAI embeddings plus exact price and sale filters | recall@5 of 1.000; first result correct 96% of the time |
| Answers | Claude Sonnet 5.5, restricted to catalogue facts | all 30 eval answers rated grounded by an Opus 5.5 judge |
| Orders and policies | Direct API lookup and reviewed text, no model | no model cost or hallucination risk |

Some things were built, measured and deliberately left off:

- A distilled classifier that escalates only uncertain messages costs less than routing every message through Haiku, but needs more training data before it matches Haiku's accuracy.
- Hybrid BM25 fusion and LLM reranking added cost without improving accuracy, so they only run when embeddings are unavailable.

`python -m evals.run all` reports routing accuracy with confidence intervals, retrieval recall, MRR and nDCG, groundedness, p50/p95 latency and cost per 1,000 calls. CI fails on regressions. Every request logs a JSON trace of its spans, tokens and cost, and Prometheus metrics are broken down by tenant and model.

Its security controls:

- it verifies JWTs itself, and order lookups are scoped to the token's subject
- tenants come from an allow-list, and only the gateway sets the tenant header
- structured outputs limit what a model can return
- per-tenant rate limits and daily model budgets
- the container runs as non-root on a read-only filesystem, and a NetworkPolicy restricts egress

The full results and design decisions are in [docs/assistant-writeup.md](docs/assistant-writeup.md).

## Configuration

Docker Compose reads these from `platform/.env`. Never commit that file.

| Variable | Used by | Purpose |
| --- | --- | --- |
| `MSSQL_SA_PASSWORD` | SQL Server, all .NET services | Database password |
| `RABBITMQ_USER`, `RABBITMQ_PASS` | Platform services | Message broker credentials |
| `JWT_SIGNING_KEY` | Auth, gateway, assistant | Signs and verifies access tokens |
| `PAYSTACK_SECRET_KEY` | Storefront | Paystack secret key; use `sk_test_...` for testing |
| `ANTHROPIC_API_KEY`, `OPENAI_API_KEY` | Assistant | Model access; without them the assistant runs offline |

Running the storefront outside Docker, set the same values in .NET configuration form: `ConnectionStrings__DefaultConnection`, `Paystack__SecretKey`, and outside Development `Smtp__Host`, `Smtp__Port`, `Smtp__Username`, `Smtp__Password` and `Smtp__From`. The contact details on `/contact` come from the `Contact` section of `appsettings.json`.

To test payments, use Paystack's test card `4084 0840 8408 4081` with any future expiry, CVV `408`, PIN `0000` and OTP `123456`. To receive webhooks, point Paystack at `https://<your-host>/checkout/paystack/webhook`. Paystack can't reach `localhost`, so locally only the redirect confirms payments.

## Running without Docker

Storefront, which needs the [.NET 8 SDK](https://dotnet.microsoft.com/download) and a SQL Server:

```bash
dotnet run          # http://localhost:5263; applies migrations on start
```

Assistant, which needs [Python 3.12+](https://www.python.org/downloads/):

```bash
cd platform/src/AssistantService
pip install -r requirements-dev.txt
python -m distill.train --labels gold
uvicorn assistant.main:app --port 8080
pytest                 # unit tests
python -m evals.run all
```

## Repository layout

```
Areas/ Controllers/ Models/ Services/ Views/   storefront (ASP.NET Core MVC)
Data/ Migrations/                              storefront database and EF Core migrations
platform/src/                                  platform services, including the assistant
platform/k8s/                                  Kubernetes manifests
platform/infra/                                Terraform, Ansible and SQL seed scripts
docs/                                          assistant write-up
```

## License

MIT. See [LICENSE](LICENSE).

## Contact

[tirelo.eric@gmail.com](mailto:tirelo.eric@gmail.com)
