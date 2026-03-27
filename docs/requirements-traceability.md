# Requirements Traceability

This document maps the business and non-functional requirements of the challenge to the implemented architectural solution.

It also makes explicit where the current state should be interpreted as:

- already implemented;
- partially implemented;
- future evolution.

---

## Functional Requirements

### FR-01 — Transaction Ingestion

**Requirement**
The system must allow financial transactions to be registered.

**Architectural Response**

- Transaction Service exposes a synchronous ingestion endpoint.
- Transactions are persisted as source-of-truth data in PostgreSQL.
- Integration intent is registered through the outbox pattern.
- The request does not depend on immediate consolidation.

**Current Status**
Implemented and validated.

---

### FR-02 — Transaction Retrieval

**Requirement**
The system must allow previously registered transactions to be queried.

**Architectural Response**

- Transaction Service exposes transaction lookup endpoint.
- Reads are served from persisted transaction data.

**Current Status**
Implemented.

---

### FR-03 — Daily Balance Query

**Requirement**
The system must provide a daily consolidated balance.

**Architectural Response**

- Consolidation Service processes source transactions asynchronously.
- Aggregated balance is stored in `daily_balance`.
- Transaction Service exposes read access to the projection.

**Current Status**
Implemented and validated.

---

### FR-04 — Daily Balance Composition

**Requirement**
The daily balance must represent:

- total credits;
- total debits;
- final balance per day.

**Architectural Response**

- Consolidation flow aggregates transactions by date and financial direction.
- Projection model stores the consolidated result for efficient reads.

**Current Status**
Implemented.

---

## Non-Functional Requirements

### NFR-01 — High Availability of Transaction Ingestion

**Requirement**
Transaction ingestion must remain available even if consolidation fails.

**Architectural Response**

- Write path is decoupled from consolidation.
- Transaction Service does not wait for the Consolidation Service.
- Outbox pattern preserves downstream processing intent.

**Current Status**
Architecturally implemented and aligned with the intended resilience model.

---

### NFR-02 — Throughput / Peak Load Handling

**Requirement**
The solution should support peak load without coupling ingestion to balance calculation.

**Architectural Response**

- Write path is lightweight.
- Heavy processing is asynchronous.
- Worker and API are separated.
- Projection-based read avoids on-demand recalculation.

**Current Status**
Architecturally implemented; manually validated under concurrent load in MVP scope.

---

### NFR-03 — Concurrency Handling

**Requirement**
The architecture should behave correctly under concurrent requests.

**Architectural Response**

- Source transaction write path is bounded and durable.
- Consolidation is asynchronous and decoupled.
- Derived-state update does not block caller request flow.

**Current Status**
Practically validated in MVP scope through concurrent request execution.

---

### NFR-04 — Data Loss Risk Reduction

**Requirement**
The architecture should minimize the chance of losing accepted transaction processing intent.

**Architectural Response**

- Transactional outbox pattern.
- Durable source transaction persistence.
- Retry-capable asynchronous processing path.

**Current Status**
Implemented in architectural terms; enterprise-grade guarantee hardening remains future work.

---

### NFR-05 — Eventual Consistency Support

**Requirement**
The system may tolerate a delay in balance update.

**Architectural Response**

- Consolidation is asynchronous by design.
- `daily_balance` is a projection, not an immediate inline calculation.

**Current Status**
Implemented and accepted as a deliberate trade-off.

---

### NFR-06 — Resilience to Messaging Failure

**Requirement**
Temporary messaging issues should not immediately invalidate the write path.

**Architectural Response**

- Source transaction remains stored.
- Outbox retains pending publication intent.
- Consolidation may be delayed but can recover later.

**Current Status**
Architecturally addressed.

---

### NFR-07 — Operational Observability

**Requirement**
The platform should provide diagnostics for distributed and asynchronous processing.

**Architectural Response**

- Application Insights platform integration.
- Structured processing lifecycle.
- Clear separation between write and async paths for diagnosis.

**Current Status**
Foundationally implemented; advanced dashboards and SLO-based monitoring remain future work.

---

### NFR-08 — Secure and Repeatable Delivery

**Requirement**
The system should support repeatable deployments with secure cloud authentication patterns.

**Architectural Response**

- Infrastructure provisioned with Terraform.
- CI/CD implemented with GitHub Actions.
- Azure authentication via OIDC.
- Secret retrieval via Key Vault.

**Current Status**
Implemented.

---

### NFR-09 — Business Continuity Direction

**Requirement**
The architecture should have a viable continuity direction under partial outage.

**Architectural Response**

- Accepted transactions remain source-of-truth data.
- Consolidation can recover later.
- Failure isolation protects the write path.

**Current Status**
Partially addressed architecturally; formal BC/DR operational maturity remains future work.

---

## Out of Scope / Future Evolution

The following are recognized as important but not fully complete in the current MVP:

- tested disaster recovery process;
- formal failover strategy;
- backup and restore validation;
- advanced alerting and dashboards;
- replay/reconciliation tooling;
- deeper network and security hardening;
- large-scale automated stress/performance benchmark suite.

---

## Final Traceability Summary

The solution already demonstrates strong alignment between challenge requirements and implementation strategy.

Its most important traceability point is:

> the architecture explicitly transforms the requirement for ingestion availability under downstream failure into a decoupled, outbox-based, event-driven design.

That decision connects directly to the most relevant functional and non-functional requirements of the challenge.
