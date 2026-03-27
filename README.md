# Cashflow Solution

## Overview

Cashflow is an event-driven solution designed to handle financial transaction ingestion and daily balance consolidation.

The architecture prioritizes availability and resilience by separating:

- a **synchronous write path** (transaction ingestion)
- an **asynchronous processing path** (daily consolidation)

This ensures that transaction ingestion remains available even if the consolidation flow is degraded or unavailable.

---

## Why this design?

This architecture prioritizes **availability of transaction ingestion over immediate consistency** of the daily balance.

This ensures that business operations are not blocked by downstream failures, at the cost of eventual consistency in the read model.

---

## Business Context

- Register financial transactions (credits/debits)
- Provide daily consolidated balance

---

## Architecture Summary

- Transaction Service → ingestion + queries
- Consolidation Service → async processing
- PostgreSQL → source of truth + read model
- Service Bus → decoupling
- Outbox Pattern → reliability
- Terraform → infrastructure
- GitHub Actions → CI/CD

---

## Key Architectural Decisions

### Event-Driven Architecture

Decouples ingestion from processing.

### Transactional Outbox

Ensures no data is accepted without integration intent.

### Eventual Consistency

Daily balance is updated asynchronously.

### Failure Isolation

Processing failures do not impact ingestion.

---

## Failure Scenario (Important)

If Azure Service Bus is unavailable:

- transactions are still accepted
- data is persisted
- consolidation is delayed
- system recovers later

👉 System degrades with delay, not failure.

---

## Scalability

- API is lightweight and stateless
- Worker processes asynchronously
- Read model is pre-aggregated

---

## Validation

Validated scenarios:

- transaction ingestion
- async consolidation
- daily balance query
- **100 concurrent requests (Postman)**

Result:

- all transactions persisted
- all processed successfully
- no data inconsistency observed

---

## Trade-offs

| Decision         | Benefit      | Cost                   |
| ---------------- | ------------ | ---------------------- |
| Async processing | scalability  | delayed consistency    |
| Outbox           | reliability  | complexity             |
| Read model       | fast queries | projection maintenance |

---

## Documentation

- [Architecture Overview](docs/architecture-overview.md)
- [Non Functional Requirements](docs/non-functional-requirements.md)
- [Requirements Traceability](docs/requirements-traceability.md)
- [Operations Guide](docs/OPERATIONS.md)
- [Troubleshooting](docs/troubleshooting.md)
- [BC/DR and Resilience](docs/BC-DR-and-Resilience-Notes.md)

---

## Final Positioning

This solution is an **MVP with strong architectural foundations**, designed to evolve into a production-grade system.

It demonstrates:

- scalability
- resilience
- decoupling
- reliability
- operational maturity
