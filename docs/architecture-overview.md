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
```
