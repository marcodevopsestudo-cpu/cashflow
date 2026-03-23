# Architecture Details

## 1. Context

The challenge requires two core business capabilities:

- transaction control;
- daily consolidated balance.

A key non-functional requirement states that:

> the transaction service must remain available even if the consolidation process fails.

This requirement is the **primary driver for the architectural decomposition**, leading to a separation between:

- the **write path (transaction ingestion)**; and
- the **processing path (daily balance consolidation)**.

The system is therefore designed to prioritize **availability, decoupling, and recoverability** over strict consistency.

---

## 2. Containers and Responsibilities

### Transaction Service

**Hosting model**

- Azure Functions (.NET isolated)

**Responsibilities**

- receive HTTP requests;
- validate input and enforce idempotency;
- persist transaction data;
- persist outbox messages within the same database transaction;
- expose transaction query endpoints;
- periodically publish pending outbox records.

**Internal logical layers**

- API / Functions
- Application
- Domain
- Infrastructure

**Architectural role**

This service represents the **system entry point** and is optimized for:

- low-latency writes;
- high availability;
- independence from downstream processing.

---

### Consolidation Service

**Hosting model**

- Azure Functions (.NET isolated)

**Responsibilities**

- consume messages from Azure Service Bus;
- create or load batch metadata;
- load pending transactions;
- aggregate values by transaction date;
- update the `daily_balance` read model;
- mark transactions as processed;
- persist retry attempts and manual-review records.

**Architectural role**

This service is responsible for **eventual consistency**, processing transactions asynchronously and independently from the write path.

---

### PostgreSQL

**Responsibilities**

- system of record for transactions;
- outbox persistence;
- idempotency tracking;
- consolidated read model (`daily_balance`);
- batch lifecycle and error tracking.

**Architectural role**

Acts as the **durable backbone of the system**, ensuring that:

- no financial transaction is lost;
- all processing steps are traceable and auditable.

---

### Azure Service Bus

**Responsibilities**

- decouple producer and consumer;
- buffer load spikes;
- enable asynchronous communication between services.

**Architectural role**

Provides **temporal decoupling**, allowing the system to absorb load variations and process data at different rates.

---

### DB Migrator

**Responsibilities**

- execute SQL scripts in order;
- record migration history;
- validate script integrity via checksum.

**Architectural role**

Ensures **deterministic and auditable schema evolution**, avoiding drift between environments.

---

## 3. Main Data Flows

### 3.1 Transaction Ingestion Flow

1. Client calls Transaction Service.
2. Request is authenticated and authorized.
3. Input and idempotency key are validated.
4. Transaction is persisted in PostgreSQL.
5. Outbox message is persisted in the same database transaction.
6. API returns success immediately, without waiting for consolidation.

**Key property**

- write path is **fully decoupled from consolidation**;
- guarantees **availability under load or downstream failure**.

---

### 3.2 Outbox Publication Flow

1. Timer-triggered publisher executes.
2. Pending outbox entries are retrieved.
3. Messages are published to Azure Service Bus.
4. Outbox entries are marked as published or retried on failure.

**Key property**

- ensures **reliable message delivery** without distributed transactions.

---

### 3.3 Consolidation Flow

1. Consolidation Service receives a batch message.
2. Batch metadata is created or retrieved.
3. Related transactions are loaded and validated.
4. Transactions are aggregated by date.
5. `daily_balance` is upserted.
6. Transactions are marked as consolidated.
7. Batch is finalized.
8. On repeated failure, a manual-review record is persisted.

**Key property**

- consolidation is **asynchronous, retryable, and observable**.

---

## 4. Database Concerns

### Transaction-side tables

- `transaction`
- `outbox`
- `idempotency`

### Consolidation-side tables

- `daily_balance`
- `daily_batch`
- `transaction_processing_error`

### Migration Strategy

- migration scripts are versioned in source control;
- applied via `db-migrator`;
- never modified after execution;
- schema changes are introduced through new scripts only.

**Outcome**

- deterministic environments;
- auditability of schema evolution;
- reduced risk of drift between environments.

---

## 5. Reliability Patterns

### Transactional Outbox

Ensures atomicity between:

- transaction persistence;
- event registration.

Eliminates the need for distributed transactions.

---

### Idempotency

Required due to:

- HTTP retries;
- message broker redelivery.

Prevents duplicate transaction processing.

---

### Bounded Retry

- retries are limited;
- prevents infinite processing loops;
- forces visibility of persistent failures.

---

### Manual Error Lane

- failed records are persisted;
- allows manual intervention and reprocessing;
- avoids silent data loss.

---

## 6. Architectural Decision — Asynchronous Consolidation

A synchronous balance calculation approach was intentionally avoided.

### Reason

A synchronous model would:

- couple transaction ingestion to aggregation logic;
- introduce contention under load;
- propagate failures from consolidation into the write path.

### Decision

Use **asynchronous processing** to:

- preserve availability of transaction ingestion;
- isolate responsibilities;
- allow independent scaling.

---

## 7. Messaging Strategy — Batch vs Single Event

Two approaches were considered:

- per-transaction messaging;
- batch-based messaging.

### Selected approach: Batch

**Advantages**

- reduces message volume;
- improves aggregation efficiency;
- minimizes database round-trips.

**Trade-offs**

- more complex failure handling;
- partial failures are harder to isolate.

The current implementation favors **operational simplicity and throughput efficiency**.

---

## 8. Correlation and Observability

A correlation identifier is generated at the HTTP boundary and propagated through:

- outbox records;
- Service Bus messages;
- consolidation batches.

This enables:

- end-to-end traceability;
- root-cause analysis;
- auditability of business flows.

---

## 9. Infrastructure Position

Infrastructure is provisioned using Terraform.

For the challenge scope, the design prioritizes:

- clarity;
- simplicity;
- fast setup.

### Planned evolution (production-grade)

- network segmentation (VNets and subnets);
- private endpoints;
- stricter database isolation;
- controlled ingress and egress;
- enhanced security posture.

---

## 10. Architectural Summary

This architecture is intentionally pragmatic:

- **simple enough** to be clearly understood and evaluated;
- **robust enough** to meet the challenge’s non-functional requirements;
- **flexible enough** to evolve into a production-grade system.

Core principles:

- decoupling between write and processing paths;
- resilience through asynchronous workflows;
- durability of financial data;
- observability and recoverability of failures.

The system favors **availability and controlled eventual consistency**, which aligns with the challenge constraints.
