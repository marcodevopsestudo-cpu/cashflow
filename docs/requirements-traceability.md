# Requirements Traceability

This document maps the challenge requirements to the implemented solution.

The goal is to clearly indicate:

- what is fully implemented;
- what is implemented as a minimum viable version (MVP);
- what is intentionally left as future evolution.

---

## 1. Business Requirements

### Requirement: Service to control transactions

**Status: Implemented**

**Evidence**

- Transaction Service receives debit and credit requests;
- transactions are persisted in PostgreSQL;
- transaction retrieval endpoints are available;
- idempotency is enforced at the application level.

---

### Requirement: Daily consolidated balance service

**Status: Implemented (MVP with full architectural separation)**

**Evidence**

- dedicated Consolidation Service exists as an independent workload;
- batch-oriented processing flow is implemented;
- aggregation logic updates the `daily_balance` read model;
- retry and manual error handling are implemented;
- supporting database tables and migrations are present.

**Note**

The current implementation focuses on a robust MVP aligned with the challenge scope.
Additional refinements (e.g., fine-grained failure handling) are documented as future improvements.

---

## 2. Mandatory Technical Requirements

### Requirement: Solution design

**Status: Implemented**

**Evidence**

- architecture documented in `docs/architecture.md`;
- data flows explicitly described;
- clear service decomposition and responsibilities;
- documented design decisions and trade-offs.

---

### Requirement: Use C#

**Status: Implemented**

**Evidence**

- all services and supporting components are implemented in .NET / C#.

---

### Requirement: Tests

**Status: Partially implemented (focused on core logic)**

**Evidence**

- unit and application-level tests exist for both services.

**Note**

- end-to-end integration testing and load testing are identified as next steps;
- current validation is supported through deterministic local execution (see operations guide).

---

### Requirement: Good practices, patterns, SOLID, architecture

**Status: Implemented**

**Evidence**

- clear separation between Domain, Application, and Infrastructure layers;
- transactional outbox pattern;
- idempotency handling;
- bounded retry strategy;
- infrastructure as code (Terraform);
- migration automation through DB Migrator.

---

### Requirement: Clear README explaining how it works and how to run locally

**Status: Implemented**

**Evidence**

- root-level README with system overview;
- explicit local execution flow;
- end-to-end validation steps;
- supporting documentation in `docs/`.

---

### Requirement: Public repository with documentation

**Status: Implemented**

**Evidence**

- documentation organized under `docs/`;
- architecture, operations, non-functional requirements, and resilience clearly described.

---

## 3. Non-Functional Requirements

### Requirement: Transaction service must remain available if consolidation is down

**Status: Implemented**

**How it is achieved**

- asynchronous decoupling using outbox + Service Bus;
- transaction ingestion does not depend on consolidation processing;
- failures in downstream services do not affect the write path.

---

### Requirement: Consolidation must handle burst traffic and tolerate limited data loss

**Status: Implemented (architectural level, validated through design)**

**How it is achieved**

- asynchronous processing with independent scaling (Azure Functions);
- buffering through outbox and Service Bus;
- batch processing to improve throughput;
- retry and manual handling for failed records.

**Note**

- final throughput and loss-rate validation depend on environment sizing and load testing;
- observability and operational controls are defined to enforce SLA (see NFR document).

---

## 4. Security

**Status: Implemented (baseline) with planned hardening**

**Evidence**

- Microsoft Entra ID integration;
- GitHub OIDC authentication for deployments;
- Azure Key Vault integration;
- planned network isolation (documented in architecture and BC/DR docs).

---

## 5. Resilience

**Status: Implemented**

**Evidence**

- transactional outbox guarantees event durability;
- retry strategy in consolidation workflow;
- manual review lane for unrecoverable failures;
- idempotency across transaction and batch processing.

---

## 6. Observability

**Status: Implemented (baseline)**

**Evidence**

- Application Insights integration;
- correlation ID propagation strategy;
- documented metrics and alert recommendations.

---

## 7. Future Improvements

The solution explicitly documents realistic next steps instead of overstating completeness.

Examples:

- automated replay tooling for failed batches;
- dead-letter queue integration;
- finer-grained failure handling inside batches;
- load testing and SLA validation;
- enhanced network isolation and security controls.

---

## 8. Final Positioning

The solution aligns with the challenge objectives by demonstrating:

- clear architectural decomposition;
- separation between write and processing paths;
- support for key non-functional requirements;
- resilience and failure isolation;
- operational readiness and reproducibility;
- explicit trade-offs and evolution roadmap.

The implementation prioritizes **correct architectural direction and clarity of reasoning**, while leaving room for iterative improvement.
