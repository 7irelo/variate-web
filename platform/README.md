# Variate Platform

This folder contains a decoupled e-commerce platform for AKS deployment with:

- `auth-service` for JWT authentication
- `catalog-service` for products/categories/inventory
- `order-service` for checkout and order lifecycle
- `payment-service` for payment processing and webhooks
- `notification-service` for event-driven notifications
- `gateway-service` as an API edge for all services
- `rabbitmq` for asynchronous communication
- containerized SQL Server for persistent data

## Service architecture

- Synchronous paths: client -> `gateway-service` -> service HTTP APIs
- Asynchronous paths: `order-service` publishes `order.created` and `payment.requested`, `payment-service` publishes `payment.completed`
- Event broker: RabbitMQ topic exchange `variate.events`

## Key endpoints

### Auth service

- `POST /api/v1/auth/register`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `GET /api/v1/auth/me`

### Catalog service

- `GET /api/v1/catalog/products`
- `GET /api/v1/catalog/products/{id}`
- `POST /api/v1/catalog/products`
- `PATCH /api/v1/catalog/products/{id}/stock`
- `GET /api/v1/catalog/categories`

### Order service

- `POST /api/v1/orders/checkout`
- `GET /api/v1/orders/{id}`
- `GET /api/v1/orders/user/{userId}`
- `PATCH /api/v1/orders/{id}/status`
- `POST /api/v1/orders/{id}/cancel`

### Payment service

- `POST /api/v1/payments/checkout`
- `GET /api/v1/payments/{id}`
- `GET /api/v1/payments/order/{orderId}`
- `POST /api/v1/payments/webhook/stripe`

## Local development (containerized)

1. Copy `.env.example` to `.env` and set secrets.
2. Start the stack:

```bash
cd platform
docker compose up --build
```

3. Access services:

- Gateway: `http://localhost:7000`
- RabbitMQ UI: `http://localhost:15672`
- Legacy monolith container: `http://localhost:7010`

## Kubernetes deployment

- Base manifests are in `platform/k8s/base`
- Apply with:

```bash
kubectl apply -k platform/k8s/base
```

## Terraform infrastructure

Use `platform/infra/terraform` to provision:

- Azure Resource Group
- VNet and AKS subnet
- AKS cluster
- Azure Container Registry
- Log Analytics workspace

## Ansible automation

Use `platform/infra/ansible` playbooks to:

- Build and push all service images to ACR
- Apply Kubernetes manifests to AKS
