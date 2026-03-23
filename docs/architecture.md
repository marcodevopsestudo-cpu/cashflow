# Architecture Details

## 1. Context

The challenge requires two business capabilities:

- transaction control;
- daily consolidated balance.

It also requires that the transaction service should not become unavailable if the daily consolidation system fails. This is the central non-functional driver behind the architectural decomposition.

---

## 2. Containers and Responsibilities

### Transaction Service

Hosting model:

- Azure Functions (.NET isolated)

Responsibilities:

- receive HTTP requests;
- validate input;
- persist transaction data;
- persist outbox messages in the same local transaction;
- expose transaction query endpoints;
- periodically publish pending outbox records.

Internal logical layers:

- API/Functions
- Application
- Domain
- Infrastructure

### Consolidation Service

Hosting model:

- Azure Functions (.NET isolated)

Responsibilities:

- consume Service Bus messages;
- create/load batch metadata;
- load pending transactions;
- aggregate amounts by day;
- update `daily_balance`;
- mark processed transactions;
- persist retry and manual handling information.

### PostgreSQL

Responsibilities:

- system of record for transactions;
- outbox persistence;
- idempotency tracking;
- consolidated read model;
- batch state and manual error records.

### Azure Service Bus

Responsibilities:

- decouple producer and consumer;
- buffer spikes;
- support asynchronous integration.

### DB Migrator

Responsibilities:

- execute SQL scripts in order;
- record migration history;
- ensure script immutability through checksum verification.

---

## 3. Main Data Flows

### 3.1 Transaction ingestion flow

1. Client calls Transaction Service.
2. Request is authenticated/authorized.
3. Application validates input and idempotency key.
4. Transaction is saved to PostgreSQL.
5. Outbox message is saved in the same database transaction.
6. API returns success without waiting for consolidation.

### 3.2 Outbox publication flow

1. Timer-triggered publisher runs.
2. Pending outbox entries are loaded.
3. Messages are published to Azure Service Bus.
4. Outbox entries are marked as published or retried later on failure.

### 3.3 Consolidation flow

1. Consolidation Service receives a batch message.
2. The workflow creates or loads the batch.
3. Transactions are loaded and validated.
4. Transactions are aggregated by date.
5. `daily_balance` is upserted.
6. Transactions are marked as consolidated.
7. Batch is finalized.
8. On repeated failure, a manual-review record is persisted.

---

## 4. Database Concerns

### Transaction-side tables

- transaction table
- outbox table
- idempotency table

### Consolidation-side tables

- `daily_balance`
- `daily_batch`
- `transaction_processing_error`

### Migration approach

Migration scripts are versioned in source control and applied through `db-migrator`.

Important rule:

- applied migration files should not be edited;
- any schema change should be introduced through a new migration file.

This keeps environments deterministic and auditable.

---

## 5. Reliability Patterns Used

### Transactional Outbox

Guarantees local atomicity between the write model and integration event registration.

### Idempotency

Required because HTTP retries and broker redelivery are expected realities in distributed systems.

### Bounded Retry

Avoids infinite loops and forces visibility when a problem needs manual intervention.

### Manual Error Lane

Useful for financial data where silent discard is unacceptable.

---

## 6. Why not synchronous balance calculation?

A synchronous approach would make transaction ingestion directly dependent on the balance processor and potentially on aggregation logic, contention, and downstream failures.

That would violate the core availability requirement of the challenge.

The asynchronous model was therefore selected to preserve write-path availability and isolate responsibilities.

---

## 7. Why not process each transaction as an individual message?

Per-transaction messaging is viable, but a batch contract was selected for this version because:

- it reduces broker chatter for the consolidation workload;
- it can reduce database round-trips during aggregation;
- it is a pragmatic fit for daily balance materialization.

Trade-off:

- batch failure handling is more complex;
- the current version still treats some failures at batch level.

---

## 8. Correlation and Diagnostics

The preferred model is to create a correlation identifier at the HTTP boundary and propagate it through the outbox and messaging pipeline.

This allows a reviewer or operator to answer questions such as:

- which request generated this outbox record?
- which batch updated this balance row?
- which retries belong to the same business flow?

---

## 9. Network and Infrastructure Position

Terraform is used to provision the platform resources. For the challenge, the infrastructure favors speed of delivery and clarity.

Production-oriented evolution documented for later:

- segmented VNets and subnets;
- private endpoints;
- stronger database isolation;
- more restrictive ingress and egress controls.

---

## 10. Architectural Summary

This architecture is intentionally pragmatic:

- simple enough to explain clearly;
- robust enough to satisfy the challenge’s non-functional direction;
- extensible enough to evolve into a more production-grade enterprise implementation.
