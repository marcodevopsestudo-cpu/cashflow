# Operations Guide

This guide describes how to operate, validate and reason about the solution in local or controlled environments.

It is intentionally focused on the current MVP posture while also highlighting the path toward stronger operational maturity.

---

## 1. Operational Scope

The solution contains:

- Transaction Service;
- Consolidation Service;
- DB Migrator;
- Terraform-managed Azure infrastructure;
- CI/CD pipelines with GitHub Actions.

Operationally, the platform combines:

- synchronous API behavior;
- asynchronous background publication;
- asynchronous projection processing;
- cloud-hosted observability and secret-management foundations.

---

## 2. Recommended Local Execution Order

For local validation, the recommended order is:

1. start PostgreSQL and confirm connectivity;
2. apply database migrations with DB Migrator;
3. configure local settings for services;
4. start Transaction Service;
5. start Consolidation Service;
6. send test transactions;
7. validate database state and read-model behavior;
8. inspect logs and telemetry.

This order reduces troubleshooting ambiguity.

---

## 3. Local Validation Checklist

After executing the local flow, validate the following:

### Transaction Ingestion

- API is reachable;
- transaction request succeeds;
- source transaction is stored.

### Outbox

- outbox record is created for accepted transaction;
- pending publication items can be identified if applicable.

### Messaging

- asynchronous publication flow occurs correctly;
- consolidation receives the expected message shape.

### Consolidation

- worker processes pending message;
- batch lifecycle is traceable;
- daily balance projection is updated.

### Read Path

- `GET /api/daily-balance/{date}` returns the consolidated result after processing.

### Failure Visibility

- failures are observable in logs/telemetry;
- failed items can be traced for diagnosis.

---

## 4. Main Operational Stores and Tables

Important runtime data areas include:

- source transaction data;
- outbox/pending integration intent;
- daily batch or processing lifecycle records;
- `daily_balance` read model;
- failed/reconciliation-oriented processing records.

Operationally, this means diagnosis often requires understanding whether the issue is in:

- ingestion;
- publication;
- messaging;
- worker processing;
- projection update.

---

## 5. Typical Runtime Flow

### Step 1 — Ingestion

A client submits a transaction to the Transaction Service.

### Step 2 — Durable Write

The service persists the source transaction and the integration intent.

### Step 3 — Asynchronous Continuation

The platform publishes the pending event/message.

### Step 4 — Worker Processing

The Consolidation Service consumes the message and processes the referenced data.

### Step 5 — Projection Update

The `daily_balance` read model is updated.

### Step 6 — Query

The daily balance becomes available through the Transaction Service read endpoint.

This sequence is important for operations because a query issue may be caused by lag in any intermediate step, not necessarily by a problem in the API itself.

---

## 6. Degraded-Mode Understanding

### If the API Is Healthy but Balance Is Stale

Possible causes:

- outbox backlog;
- messaging delay;
- worker failure;
- projection update issue.

Interpretation:

- source data may already be safe;
- derived data may still be catching up.

### If Service Bus Is Temporarily Unavailable

Expected behavior:

- transactions may continue being accepted if the write path remains healthy;
- pending propagation waits for recovery;
- consolidation is delayed.

Interpretation:

- the system is degraded, but not necessarily down.

### If the Worker Is Failing

Expected behavior:

- transaction ingestion may still continue;
- read model freshness degrades;
- failed items should become visible for diagnosis.

This separation is fundamental to understanding platform health correctly.

---

## 7. Operational Monitoring Focus

At minimum, operations should monitor:

### API Health

- request success rate;
- latency;
- dependency failures.

### Database Health

- connection stability;
- query failures;
- migration status.

### Outbox Health

- pending count/backlog;
- age of oldest pending item.

### Messaging Health

- publication success/failure;
- queue/topic/subscription backlog;
- dead-letter or poison-message indicators.

### Worker Health

- processing success rate;
- retry volume;
- failure rate;
- lag between ingestion and consolidation.

### Business Readiness

- freshness of `daily_balance`;
- ability to query expected dates successfully.

---

## 8. Deployment Operations

The repository already supports CI/CD-based delivery.

Operationally, deployment should follow this mindset:

1. validate build and tests;
2. apply database migrations safely;
3. publish service artifacts;
4. deploy services;
5. verify runtime health;
6. confirm post-deploy functionality.

This sequence matters because asynchronous systems can appear “deployed” while still having broken background flow.

---

## 9. Local Execution Example

### DB Migrator

```bash
export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=transactiondb;Username=postgres;Password=postgres"

dotnet run \
  --project ./db-migrator/db-migrator.csproj \
  -- \
  --migrations-path=./transaction-service/scripts
```
