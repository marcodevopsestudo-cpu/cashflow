---

## 3) File: `requirements-traceability.md`
### Directory
`/docs/requirements-traceability.md`

```md
# Requirements Traceability

This document maps the challenge requirements to the implemented solution and to the supporting repository documentation.

Its purpose is to make clear:

- what is implemented;
- where the evidence is in the repository;
- which parts are intentionally presented as MVP or future evolution.

---

## 1. Business Requirements

### Requirement

Service to control financial transactions.

**Status**
Implemented.

**Evidence**

- Transaction Service receives debit and credit requests.
- Transactions are persisted in PostgreSQL.
- Transaction retrieval endpoint is available.
- Idempotency is enforced on the write path.

**Repository evidence**

- `transaction-service/`
- `transaction-service/README.md`
- root `README.md`

---

### Requirement

Service for daily consolidated balance.

**Status**
Implemented as a functional MVP with full architectural separation.

**Evidence**

- Consolidation Service exists as an independent workload.
- Batch-oriented processing flow is implemented.
- Aggregation updates the `daily_balance` read model.
- Bounded retry behavior is implemented.
- Manual review registration exists for exhausted failures.

**Repository evidence**

- `consolidation-service/`
- root `README.md`
- `docs/architecture-overview.md`

**Note**
The current implementation already demonstrates the main architectural decision required by the challenge: consolidation is independent from transaction ingestion. Additional operational refinement can still evolve.

---

## 2. Mandatory Technical Requirements

### Requirement

Solution design.

**Status**
Implemented.

**Evidence**

- architecture documented in `docs/architecture-overview.md`;
- responsibilities and data flows explicitly described;
- service decomposition and trade-offs documented.

---

### Requirement

Use C#.

**Status**
Implemented.

**Evidence**

- services and supporting components are implemented in .NET / C#.

---

### Requirement

Tests.

**Status**
Partially implemented, focused on core logic.

**Evidence**

- unit and application-level tests exist for the services;
- local deterministic execution flow is documented.

**Note**
Broader integration and load testing are valid next steps, but the repository already contains enough testing evidence to satisfy the challenge expectation at a minimum viable level.

---

### Requirement

Good practices, patterns, SOLID, architecture.

**Status**
Implemented.

**Evidence**

- layered separation between Domain, Application, Infrastructure, and API/Worker;
- transactional outbox pattern;
- request-level idempotency;
- bounded retry strategy;
- infrastructure as code with Terraform;
- deterministic database migrations via DB Migrator.

---

### Requirement

README with clear instructions on how the application works and how to run locally.

**Status**
Implemented.

**Evidence**

- root `README.md` explains the solution and repository structure;
- `transaction-service/README.md` contains service-specific execution guidance;
- `docs/OPERATIONS.md` describes the recommended execution sequence.

---

### Requirement

Public repository with project documentation.

**Status**
Implemented.

**Evidence**

- documentation is organized under `docs/`;
- architecture, operations, non-functional reasoning, and requirement coverage are documented in the repository.

---

## 3. Non-Functional Requirements

### Requirement

The transaction service must not become unavailable if the daily consolidation system fails.

**Status**
Implemented.

**How it is addressed**

- transaction ingestion is separated from consolidation;
- outbox records are persisted with the source transaction;
- Azure Service Bus decouples producer and consumer;
- consolidation is asynchronous and independent from the write path.

**Repository evidence**

- root `README.md`
- `docs/architecture-overview.md`
- `docs/non-functional-requirements.md`

---

### Requirement

In peak days, the consolidation service receives 50 requests per second with at most 5% request loss.

**Status**
Architecturally addressed.

**How it is addressed**

- the ingestion path is lightweight and synchronous only up to durable persistence;
- buffering happens across PostgreSQL, outbox, and Azure Service Bus;
- failures are tracked explicitly;
- derived data incompleteness is measurable and recoverable.

**Repository evidence**

- `docs/non-functional-requirements.md`
- `docs/architecture-overview.md`

**Note**
The repository documents the architectural mechanisms that address this requirement. Full empirical certification would require dedicated load testing and operational validation.

---

## 4. Security and Access Controls

### Requirement

Security is part of the architectural evaluation.

**Status**
Implemented at infrastructure and service-configuration level.

**Evidence**

- Microsoft Entra ID configuration for the Transaction Service;
- issuer, audience, scope and allowed application validation;
- Managed Identity configuration for Service Bus access;
- Key Vault references for secrets;
- RBAC assignments for infrastructure access;
- GitHub Actions deployment through OIDC.

**Repository evidence**

- `infra/`
- root `README.md`
- `docs/architecture-overview.md`

---

## 5. What Is MVP vs What Is Future Evolution

### Implemented as part of the delivered solution

- Transaction Service
- Consolidation Service as independent workload
- DB Migrator
- Terraform-managed infrastructure
- core resilience patterns
- core security controls
- CI/CD automation

### Natural future evolution

- richer integration and load testing;
- deeper dashboards and operational alerts;
- improved manual reconciliation/backoffice flow for exhausted failures;
- stronger BC/DR operational maturity.

---

## 6. Final Assessment

The repository satisfies the challenge mainly through its architectural decisions:

- the write path is protected;
- the consolidation flow is decoupled;
- source data is durable;
- downstream failures are recoverable;
- the solution is documented in a way that exposes architectural reasoning, not only code structure.
