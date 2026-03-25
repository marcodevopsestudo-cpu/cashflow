# Requirements Traceability

This document maps business and non-functional requirements to the implemented solution.

---

## Functional Requirements

### FR-01: Transaction Ingestion

**Requirement:**
The system must allow merchants to register financial transactions (credit/debit).

**Implementation:**

- Transaction Service endpoint:
  - `POST /api/transactions`

- Data persisted in PostgreSQL
- Idempotency support (if applicable)
- Outbox pattern ensures reliable event publishing

---

### FR-02: Daily Balance Reporting

**Requirement:**
The system must provide a daily consolidated balance per merchant.

**Implementation:**

- Asynchronous consolidation via Consolidation Service
- Aggregation stored in `daily_balance` table
- Exposed through Transaction Service endpoint:

```
GET /api/daily-balance/{date}
```

**Notes:**

- Balance includes:
  - Total credits
  - Total debits
  - Final balance per day

- Data is pre-aggregated for fast queries

---

## Non-Functional Requirements

### NFR-01: High Availability

**Requirement:**
The transaction ingestion service must remain available even if consolidation fails.

**Solution:**

- Decoupled architecture
- Transaction Service does not depend on Consolidation Service
- Outbox pattern ensures durability

---

### NFR-02: Throughput (≥ 50 req/s)

**Requirement:**
The system must handle peak loads of at least 50 requests per second.

**Solution:**

- Stateless Transaction Service
- Fast database writes
- No synchronous processing during request
- Asynchronous processing via Service Bus

**Design Strategy:**

Instead of processing balances synchronously:

1. Transactions are persisted quickly
2. Events are stored in Outbox
3. Worker processes them asynchronously

---

### NFR-03: Data Loss Control (≤ 5%)

**Requirement:**
Maximum acceptable data loss is 5%.

**Solution:**

- Transactional Outbox Pattern
- Reliable messaging (Service Bus)
- Retry mechanisms in worker
- Dead-letter queue for failed messages

---

### NFR-04: Eventual Consistency

**Requirement:**
The system may tolerate slight delays in balance updates.

**Solution:**

- Consolidation is asynchronous
- Balance is not real-time
- Updates occur within seconds (up to ~30 seconds)

**Justification:**

This trade-off enables:

- Higher throughput
- Better availability
- Reduced system coupling

---

## Summary

| Requirement             | Status                 | Implementation         |
| ----------------------- | ---------------------- | ---------------------- |
| Transaction ingestion   | ✅ Implemented         | Transaction Service    |
| Daily balance reporting | ✅ Implemented         | Worker + Query API     |
| High availability       | ✅ Implemented         | Decoupled architecture |
| Throughput ≥ 50 req/s   | ✅ Addressed by design | Async processing       |
| Data loss ≤ 5%          | ✅ Addressed           | Outbox + retries       |
| Eventual consistency    | ✅ Implemented         | Async consolidation    |

---

## Final Considerations

The solution intentionally prioritizes:

- Availability over strict consistency
- Scalability over synchronous processing
- Reliability through decoupling and messaging

All requirements are addressed either through implementation or architectural design decisions.
