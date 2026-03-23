# Non-Functional Requirements — Throughput, Resilience and Data Loss Control

The challenge defines two key non-functional constraints:

- the system must support peak load during transaction ingestion;
- the consolidation process may tolerate up to **5% data loss**.

This section explains how the current architecture addresses these constraints through **decoupling, buffering, and controlled failure handling**.

---

## 1. Throughput Strategy (Handling Peak Load)

The system is designed so that **peak load is absorbed by the Transaction Service**, not by the consolidation process.

### Key design decisions

- Transaction Service performs only:
  - request validation;
  - persistence;
  - outbox registration.

- No synchronous dependency on consolidation logic.

- The request lifecycle is bounded by:
  - HTTP processing;
  - PostgreSQL write latency.

### Result

- transaction ingestion remains stable under high load;
- consolidation can lag without affecting availability;
- the system supports burst traffic without blocking the write path.

---

## 2. Buffering Model (Backpressure Handling)

The architecture introduces **three buffering layers**, each with a specific role:

### 1. PostgreSQL (Source of Truth)

- stores all transactions durably;
- guarantees no loss of financial data.

### 2. Outbox Table

- stores integration events reliably;
- ensures events are not lost if publishing fails.

### 3. Azure Service Bus

- buffers messages between producer and consumer;
- absorbs traffic spikes;
- enables asynchronous scaling.

### Result

If the consolidation service becomes slow or unavailable:

- transactions continue to be accepted;
- outbox records accumulate;
- messages are processed later when capacity is restored.

This guarantees **availability under load and failure conditions**.

---

## 3. Data Loss Model

The architecture is designed to **eliminate loss of source data** and allow **controlled loss of derived data**.

### Possible loss scenarios

1. **Message processing failure after retry exhaustion**
   - messages are moved to a failure state (manual review required)

2. **Consolidation workflow errors**
   - some transactions may not be included in `daily_balance`

3. **Extreme infrastructure failures**
   - rare scenarios before recovery mechanisms act

### Important distinction

- **Transactions (source data): never lost**
- **Daily balance (derived data): may be temporarily incomplete**

This aligns with the challenge requirement of tolerating limited data loss.

---

## 4. SLA Control — Maximum 5% Loss

The system enforces SLA compliance through **measurement, retry strategies, and operational control**.

### 4.1 Explicit Tracking

The system records:

- processed transactions;
- failed transactions (`transaction_processing_error`);
- pending transactions.

This enables calculation of:

```text
loss_rate = failed_transactions / total_transactions
```
