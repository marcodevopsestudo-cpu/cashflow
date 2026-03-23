# Requirements Traceability

This document maps the delivered solution to the challenge requirements and clarifies where the implementation is complete, partially complete, or intentionally documented as future evolution.

---

## 1. Business Requirements

### Requirement: service to control transactions

Status: **Implemented**

Evidence:

- Transaction Service receives debit and credit entries.
- Transactions are persisted in PostgreSQL.
- Query capability exists for transaction retrieval.

### Requirement: daily consolidated balance service

Status: **Implemented in architectural design and service structure**

Evidence:

- dedicated Consolidation Service project;
- batch-oriented processing workflow;
- daily balance materialization tables and scripts;
- retry and manual error handling model.

---

## 2. Mandatory Technical Requirements

### Requirement: solution design

Status: **Implemented**

Evidence:

- centralized root documentation;
- architecture rationale;
- data flow explanation;
- repository structure documentation.

### Requirement: use C#

Status: **Implemented**

Evidence:

- services and migrator implemented in .NET/C#.

### Requirement: tests

Status: **Implemented, with room to expand**

Evidence:

- unit test projects exist for transaction and consolidation layers.

Observation:

- integration and load testing can be expanded in future iterations.

### Requirement: good practices, patterns, SOLID, architecture

Status: **Implemented**

Evidence:

- service decomposition by responsibility;
- application/domain/infrastructure separation;
- outbox pattern;
- idempotency;
- retry strategy;
- infrastructure as code;
- migration automation.

### Requirement: clear README explaining how it works and how to run locally

Status: **Implemented**

Evidence:

- root-level README and operations guide.

### Requirement: public repository with all project documentation

Status: **Addressed by repository structure**

Evidence:

- documentation centralized at root for easier review.

---

## 3. Non-Functional Requirements

### Requirement: transaction service must remain available if consolidation is down

Status: **Implemented by architecture**

How it is achieved:

- asynchronous decoupling through outbox and Service Bus;
- transaction write path does not synchronously depend on consolidation.

### Requirement: consolidation should handle burst traffic and tolerate limited loss

Status: **Architecturally addressed**

How it is approached:

- Azure Functions for elastic processing;
- Service Bus for buffering and decoupling;
- independent scaling of consumer workload;
- batch processing to improve efficiency.

Observation:

- final throughput validation depends on environment sizing and load testing.

---

## 4. Security

Status: **Implemented and partially documented for future hardening**

Evidence:

- Microsoft Entra ID integration;
- GitHub OIDC for deployment authentication;
- Key Vault integration in pipeline;
- planned private network segmentation.

---

## 5. Resilience

Status: **Implemented**

Evidence:

- outbox for reliable asynchronous publication;
- retry handling in consolidation workflow;
- manual review lane for exhausted failures;
- idempotency at transaction and batch level.

---

## 6. Observability

Status: **Implemented and documented**

Evidence:

- Application Insights usage;
- root documentation for Correlation ID propagation;
- recommended telemetry dimensions and alerting ideas.

---

## 7. Future Improvements Explicitly Documented

The challenge explicitly welcomes future evolution notes. The repository therefore documents realistic next steps instead of overstating current completeness.

Examples:

- item-level fallback inside failed batches;
- deeper network isolation;
- stronger automated replay and dead-letter handling;
- wider test coverage and load testing.

---

## 8. Final Assessment Position

The solution is aligned with the challenge intent because it demonstrates:

- architectural decomposition;
- non-functional reasoning;
- resilience and availability patterns;
- security-minded deployment;
- operational readiness;
- conscious trade-off documentation.
