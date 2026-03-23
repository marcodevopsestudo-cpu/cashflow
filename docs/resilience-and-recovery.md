# BC/DR and Resilience Notes

## 1. Objectives

The system is designed with the following resilience objectives:

- keep transaction ingestion available even if downstream components fail;
- allow recovery from service or infrastructure outages;
- minimize data loss while preserving idempotency and traceability;
- ensure that financial transactions (source data) are never lost.

---

## 2. Current Resilience Posture

The current implementation already incorporates key resilience patterns:

### Decoupled Write Path

- Transaction ingestion is fully independent from consolidation.
- Requests are completed without waiting for downstream processing.

### Transactional Outbox

- Outbox records are persisted in the same database transaction as the business data.
- Guarantees that integration intent is never lost.

### Asynchronous Processing

- Azure Service Bus decouples producer and consumer.
- Consolidation runs independently and can lag without impacting availability.

### Failure Isolation

- Failures in consolidation do not propagate to the transaction service.
- Batch processing supports retries and manual intervention.

### Observability (Initial)

- Application Insights integration is available for:
  - request tracking;
  - error visibility;
  - basic operational diagnostics.

---

## 3. Known Gaps (Challenge Scope)

The current implementation intentionally prioritizes core functionality and architectural clarity.

The following areas are identified as **future improvements**, not blockers:

- no formalized regional failover process;
- limited infrastructure hardening (e.g., no private endpoints or network isolation);
- no automated runbooks for PostgreSQL recovery;
- observability not yet extended to full SLO/SLA monitoring.

---

## 4. Recovery Model

The system is designed around **recoverability rather than strict prevention**.

### Data Safety

- Transactions are stored in PostgreSQL (source of truth).
- Even if message publication or processing fails, data can be reprocessed.

### Event Recovery

- Outbox enables replay of unpublished events.
- Failed batches are recorded for manual or automated reprocessing.

### Processing Recovery

- Consolidation is idempotent and can be safely retried.
- Partial failures do not corrupt the system state.

---

## 5. Suggested Failover Strategy

The following approach is recommended for evolving the system toward production-grade resilience.

### Compute (Azure Functions)

- redeploy services in a secondary region using the same artifacts;
- infrastructure is reproducible via Terraform.

### PostgreSQL

- leverage built-in high availability (Flexible Server HA);
- complement with backup and restore strategy;
- validate recovery procedures periodically.

### Azure Service Bus

- rely on built-in durability and retry mechanisms;
- consider premium tier or geo-disaster recovery for higher criticality;
- support replay through outbox or batch reprocessing.

### Storage / Key Vault

- adopt paired-region deployment;
- validate recovery of secrets and stateful components.

---

## 6. Operational Readiness (Recommended Evolution)

### Phase 1 — Baseline Operational Safety

- define RTO and RPO targets;
- document recovery runbooks;
- validate PostgreSQL backup and restore;
- create basic dashboards and alerts.

---

### Phase 2 — Infrastructure Hardening

- introduce VNet integration and subnet isolation;
- enable private endpoints for database and messaging;
- restrict ingress and egress;
- improve security posture.

---

### Phase 3 — Mature Resilience Model

- formalize failover procedures (decision tree + ownership);
- introduce periodic disaster recovery drills;
- define SLOs and error budgets;
- automate replay and recovery flows.

---

## 7. Metrics and Signals

The system should be monitored using the following indicators:

### Transaction Service

- request latency and failure rate;
- throughput (transactions/sec);

### Outbox

- backlog size;
- age of oldest message;
- retry attempts;

### Consolidation Service

- batch processing time;
- success vs failure rate;
- retry counts;

### PostgreSQL

- availability;
- CPU and memory usage;
- connection saturation;
- storage growth;

### Service Bus

- active messages;
- dead-letter messages;
- throttling events;

---

## 8. Architectural Positioning

This solution prioritizes:

- availability of transaction ingestion;
- durability of financial data;
- recoverability through asynchronous processing;
- controlled and observable failure handling.

The system is intentionally designed to:

- tolerate temporary failures in downstream components;
- recover without data loss at the source level;
- allow operational intervention when needed.

---

## 9. Summary

- The system already implements key resilience patterns (outbox, async processing, idempotency).
- Financial data is preserved even under failure scenarios.
- Failures are isolated and recoverable.
- Additional BC/DR capabilities are identified and structured as an evolution roadmap.

This approach balances **delivery scope and architectural soundness**, aligning with the challenge constraints.
