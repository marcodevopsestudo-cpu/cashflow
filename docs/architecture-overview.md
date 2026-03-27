# Architecture Overview

## 1. Architectural Context

The challenge requires two core business capabilities:

- transaction control;
- daily consolidated balance.

It also defines a critical non-functional requirement:

> the transaction service must remain available even if the daily consolidation service fails.

That requirement is the main driver of the architecture.

Instead of designing a synchronous end-to-end flow, the solution separates the system into:

- a **write path**, optimized for low-latency and availability;
- a **processing path**, optimized for asynchronous consolidation and recoverability.

# Architecture Overview

## 1. Architectural Context

The challenge requires two core capabilities:

- transaction control;
- daily consolidated balance.

It also introduces an important non-functional requirement:

> the transaction service must remain available even if the daily consolidation service fails.

That requirement is the main architectural driver of the solution.

Because of that, the system is intentionally separated into:

- a **write path**, optimized for low latency, durability and availability;
- a **processing path**, optimized for asynchronous consolidation, recoverability and independent scale.

This is a deliberate architectural trade-off in favor of availability and resilience over immediate strong consistency of derived data.

---

## 2. High-Level Architecture

````mermaid
flowchart LR
    Client[Client / Postman / Consumer App]
    TxApi[Transaction Service]
    Pg[(PostgreSQL<br/>Transactions + Outbox + Read Model)]
    Publisher[Outbox Publisher]
    Sb[(Azure Service Bus)]
    Consolidation[Consolidation Service]
    Ai[Application Insights]
    Kv[Key Vault]
    GitHub[GitHub Actions + OIDC]
    Azure[Azure Control Plane]
    Tf[Terraform IaC]

    Client -->|HTTP| TxApi
    TxApi --> Pg
    Pg --> Publisher
    Publisher --> Sb
    Sb --> Consolidation
    Consolidation --> Pg
    TxApi --> Ai
    Consolidation --> Ai
    TxApi --> Kv
    Consolidation --> Kv
    GitHub --> Azure
    Tf --> Azure
This design favors **availability, decoupling and resilience** over strict immediate consistency.

---

## 2. High-Level Architecture

```mermaid
flowchart LR
    Client[Client / Postman / Consumer App] -->|Microsoft Entra access token| TxApi[Transaction Service]
    GitHub[GitHub Actions + OIDC] --> Azure[Azure Control Plane]

    TxApi --> Pg[(PostgreSQL<br/>Transactions + Outbox Table)]
    Pg --> Publisher[Timer-triggered Publisher]
    Publisher --> Sb[(Azure Service Bus Topic)]
    Sb --> Consolidation[Consolidation Service]
    Consolidation --> Pg

    TxApi --> Ai[Application Insights]
    Consolidation --> Ai
    TxApi --> Kv[Key Vault]
    Consolidation --> Kv
````

## Throughput and Availability Strategy

The system was designed to eliminate bottlenecks between transaction ingestion and balance computation.

By isolating the transaction write path from the balance read model, the system ensures that high write throughput does not impact query performance.

Daily balance queries are executed against a consolidated and indexed data store, providing fast and predictable response times.

Asynchronous processing via messaging enables horizontal scalability of the consolidation process, allowing the system to handle increased load without degrading availability.

This approach ensures that the system meets and exceeds the requirement of handling high request rates while tolerating partial processing delays without affecting user-facing operations.
