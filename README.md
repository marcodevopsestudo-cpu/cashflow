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

👉 The system degrades with delay, not with total failure.

---

## Scalability

- API is lightweight and stateless
- Worker processes asynchronously
- Read model is pre-aggregated

---

## Security

The solution enforces authentication and authorization using Microsoft Entra ID.

### Authentication

- All protected endpoints require a valid JWT issued by Microsoft Entra ID.
- Token validation includes issuer, audience, and signature verification.
- Only trusted identity providers are accepted.

### Authorization

- Role-based access control (RBAC) is enforced at the API level.
- Requests are validated against required roles and permissions.
- Unauthorized requests are rejected before reaching the application layer.

### Middleware Enforcement

Security and request integrity concerns are centralized using middleware:

- Authorization middleware enforces access policies.
- Idempotency middleware ensures that duplicate requests do not result in duplicated operations.
- Correlation ID middleware ensures traceability across secure requests.

### Secure Configuration

- Sensitive settings (e.g., client IDs, secrets, connection strings) are not hardcoded.
- Configuration is managed via secure providers (e.g., Key Vault).

### Design Consideration

Security is applied at the entry point of the system, ensuring that only authenticated and authorized requests can access business functionality, without impacting internal processing flows.

## Validation

The solution was validated through functional and concurrent execution testing.

### Functional Validation

- transaction ingestion
- asynchronous consolidation
- daily balance query

### Concurrent Validation

A Postman collection was used to simulate concurrent load:

- 100 transaction requests executed
- parallel execution using async/concurrent calls

### Observed Results

- all requests were successfully accepted
- all transactions were persisted
- all transactions were consolidated
- no data inconsistency was observed

### Interpretation

This test validates the **architectural behavior under concurrency**, demonstrating:

- a resilient write path
- asynchronous processing decoupling
- correct system behavior under concurrent execution

### Important Note

This is not a formal performance benchmark.
It is a **practical validation of system behavior**, not a throughput or latency certification.

---

## Testing

A Postman collection is available for testing:

- [Postman Collection](docs/testing/cashflow-service.postman_collection.json)

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

- security
- scalability
- resilience
- decoupling
- reliability
- operational maturity
