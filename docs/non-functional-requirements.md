# Non-Functional Requirements

This document explains how the current architecture addresses the main non-functional concerns of the challenge, especially:

- throughput under peak load;
- resilience under downstream failure;
- controlled handling of incomplete derived data;
- operational observability.

The goal is not only to describe implementation details, but to make the architectural reasoning explicit.

---

## 1. Architectural Principle

The most important non-functional decision in the solution is the separation between:

- the **transaction write path**; and
- the **daily balance consolidation path**.

This separation exists because the challenge requires that the transaction service must remain available even if the consolidation system fails.

To satisfy that requirement, the architecture ensures that transaction ingestion depends only on:

- request validation;
- durable persistence in PostgreSQL;
- durable outbox registration.

The write path does **not** wait for balance consolidation to complete.

---

## 2. Throughput Strategy

The architecture is designed so that peak pressure is absorbed primarily by the Transaction Service and the persistence/buffering layers, not by synchronous balance calculation.

### Write-path behavior

During transaction ingestion, the system performs only the essential synchronous operations:

- request validation;
- persistence of transaction data;
- registration of the integration event in the outbox.

This keeps the request lifecycle bounded mainly by:

- HTTP handling;
- database write latency.

### Why this matters

Because balance processing is asynchronous, the system avoids coupling ingestion latency to:

- downstream worker execution time;
- queue consumer throughput;
- temporary consolidation degradation.

### Result

Under peak traffic:

- transaction ingestion remains available;
- consolidation may lag;
- the system continues accepting source transactions without forcing synchronous balance updates.

---

## 3. Buffering and Backpressure Model

The solution uses multiple buffering layers, each with a specific role in resilience and flow control.

### 3.1 PostgreSQL as source-of-truth buffer

PostgreSQL stores all source transactions durably.

This is the primary durability boundary of the system and guarantees that financial source data is recorded before any asynchronous processing happens.

### 3.2 Outbox as publication recovery buffer

The outbox table stores the integration intent in the same transaction as the original write.

If message publication cannot happen immediately, the outbox entry remains available for later retry.

This prevents publication gaps between source persistence and broker delivery.

### 3.3 Azure Service Bus as asynchronous work buffer

Azure Service Bus buffers work between publisher and consumer.

It helps absorb traffic spikes, supports asynchronous scaling and prevents the consumer runtime from becoming a direct dependency of the producer request path.

### Result

If the consolidation service becomes slow or temporarily unavailable:

- transactions continue to be accepted;
- outbox records can accumulate safely;
- queued messages can be processed later when capacity is restored.

This is the core mechanism that preserves write-path availability during downstream degradation.

---

## 4. Resilience Strategy

### 4.1 Protection of transaction ingestion

The transaction service is protected from consolidation failures because:

- consolidation is not executed inline;
- publication is decoupled from the HTTP request path;
- the outbox preserves recoverability if the broker is unavailable.

### 4.2 Publication recoverability

If Azure Service Bus is temporarily unavailable, the event is not lost because it has already been recorded in PostgreSQL.

Publication can be retried later by the outbox publisher.

### 4.3 Consolidation recoverability

When consolidation processing fails:

- retries are bounded;
- backoff is applied;
- exhausted failures are moved to a manual reconciliation path.

This avoids infinite retry loops and keeps operational handling explicit.

---

## 5. Data Loss Model

The architecture is designed to eliminate loss of **source financial data** while allowing controlled temporary incompleteness in **derived balance data**.

### Important distinction

- **Transactions (source data): not expected to be lost**
- **Daily balance (derived data): may be temporarily incomplete until asynchronous processing catches up or failures are reconciled**

This distinction is important because the challenge allows tolerance for limited loss, but the architecture intentionally protects the original financial events first.

### Possible scenarios affecting derived data completeness

1. message processing failure after retry exhaustion;
2. consolidation workflow errors for a subset of transactions;
3. extreme infrastructure disruption before all recovery mechanisms have acted.

### Operational interpretation

The design does not normalize silent loss.

Instead, it makes incomplete derived data:

- visible;
- measurable;
- recoverable through retry or manual follow-up.

---

## 6. Peak Load and the 5% Loss Constraint

The challenge states that on peak days the consolidation flow may receive high request volume with at most 5% loss.

The architecture addresses this requirement primarily through:

- decoupling;
- buffering;
- retry;
- explicit failure tracking.

### How the design supports the target

- the write path remains lightweight and durable;
- asynchronous buffers absorb load variation;
- failed items are recorded explicitly;
- pending, processed and failed states can be measured.

### What this means in practice

The repository demonstrates the architectural mechanisms required to support this target.

Formal certification of this percentage, however, would depend on:

- dedicated load testing;
- telemetry collection under stress;
- operational validation in an environment representative of production.

That distinction is important and honest: the design is aligned with the requirement, but numeric validation still depends on test execution.

---

## 7. Eventual Consistency

Because the architecture is asynchronous, the `daily_balance` read model is eventually consistent with the source transactions.

A small delay may exist between:

- transaction creation; and
- the visibility of the consolidated balance update.

This delay is influenced by:

- outbox polling interval;
- queue buffering;
- worker throughput;
- retry timing in failure scenarios.

### Why this trade-off was chosen

This trade-off improves:

- write-path availability;
- resilience under failure;
- decoupling between services;
- operational stability under burst traffic.

For the challenge scenario, this is a reasonable and deliberate architectural choice.

---

## 8. Observability and Operational Control

The architecture includes operational observability as part of the resilience model.

### Monitoring goals

The system should allow operators to understand:

- how many transactions were ingested;
- how many were published;
- how many were consolidated successfully;
- how many failed and require manual follow-up;
- whether the backlog is growing.

### Current operational support

The repository already includes Application Insights integration and structured logging in the services.

Persistence also supports operational tracking through explicit records such as:

- processed transactions;
- failed transactions in `transaction_processing_error`;
- pending and completed batch states.

### Why this matters

Without observability, asynchronous resilience patterns become hard to operate.

With observability, eventual consistency becomes manageable because backlog, failures and processing health can be measured and acted on.

---

## 9. Cost and Operational Simplicity

The solution adopts a serverless execution model to keep cost and operations proportional to usage.

This supports the challenge well because it avoids maintaining dedicated compute running continuously when not needed.

### Benefits of this model

- lower idle cost;
- automatic scaling behavior;
- reduced operational overhead;
- alignment with event-driven execution.

### Known trade-off

A serverless model may introduce cold start effects, which is a valid trade-off in exchange for cost control and lower operational complexity.

---

## 10. Final Assessment

The non-functional strategy of the solution is based on a clear priority order:

1. protect transaction ingestion;
2. preserve source data durably;
3. decouple balance processing;
4. recover from downstream failure;
5. make incomplete derived data visible and manageable.

That priority is consistent with the challenge and with the architecture implemented in the repository.

```

```
