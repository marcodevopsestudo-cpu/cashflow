# Consolidation Service

The Consolidation Service is the asynchronous daily-balance processor of the cashflow solution.

It consumes transaction batch messages from Azure Service Bus, loads the referenced transactions from PostgreSQL, aggregates debit and credit amounts by day, updates the `daily_balance` read model, tracks the processing lifecycle in `daily_batch`, and isolates unrecoverable items in `transaction_processing_error` for manual follow-up.

This service exists to ensure that **daily balance processing does not reduce the availability of the transaction write path**.

---

## Why This Service Exists

The challenge requires:

- a transaction control service;
- a daily consolidation service; and
- resilience so the transaction control service stays available even if consolidation fails.

To satisfy that requirement, the solution separates the system into:

- a **synchronous write path** handled by Transaction Service; and
- an **asynchronous consolidation path** handled by this service.

This design keeps transaction ingestion lightweight and durable, while allowing balance calculation to scale, retry and recover independently.

---

## Responsibilities

- consume batch messages from Azure Service Bus;
- validate and deserialize the integration message;
- register or load the current batch state;
- load referenced transactions from PostgreSQL;
- aggregate debit and credit values by balance date;
- update the `daily_balance` read model using upsert semantics;
- mark transactions as consolidated;
- finalize successful batches;
- retry failed processing with bounded attempts and exponential backoff;
- record exhausted items for manual reconciliation.

---

## Key Architectural Decisions

### Dedicated asynchronous worker

Consolidation is intentionally separated from the transaction write API.

This avoids coupling transaction ingestion latency to balance processing time and protects the write path from consolidation outages.

### Batch messages instead of one message per transaction

The service consumes a message containing a list of transaction identifiers.

This reduces throughput friction between publisher and consumer and makes the consolidation path more efficient under load.

### Bounded retry with exponential backoff

Failures are retried a limited number of times before being moved to a manual handling path.

This avoids infinite reprocessing loops and keeps failures explicit and auditable.

### Manual review lane

When retries are exhausted, failed items are recorded in `transaction_processing_error`.

This preserves recoverability without blocking the entire flow permanently.

### Structured observability

The service emits structured logs with identifiers such as:

- `CorrelationId`
- `BatchId`
- `MessageId`
- transaction counts

This improves troubleshooting and operational follow-up.

---

## Processing Flow

1. Transaction Service publishes a batch message to Azure Service Bus.
2. Consolidation Service receives the message.
3. The Function trigger creates a `ProcessConsolidationBatchCommand`.
4. The command is sent through MediatR.
5. The workflow:
   - registers or resumes the batch;
   - loads pending transactions;
   - aggregates values by date;
   - updates `daily_balance`;
   - marks transactions as consolidated;
   - finalizes the batch.
6. If processing fails, bounded retry with exponential backoff is applied.
7. If retries are exhausted, the service records the failure for manual reconciliation and marks the batch accordingly.

---

## Data Managed by This Service

The consolidation flow works with these main persistence concerns:

- `daily_batch` for batch lifecycle tracking;
- `daily_balance` for the consolidated read model;
- `transaction_processing_error` for unrecoverable or manually reviewed items.

This allows the service to be operationally transparent: success, retry, and failure states are visible in persisted data.

---

## Message Contract

```json
{
  "batchId": "4e2f4ba2-9c1a-4178-a65c-7b1e9fe44cb9",
  "correlationId": "c5307e1d706040a2b066e8a0a1e1df0d",
  "publishedAtUtc": "2026-03-22T15:30:00Z",
  "transactionIds": [101, 102, 103, 104]
}
```
