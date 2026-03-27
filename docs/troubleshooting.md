---

# 8) `docs/troubleshooting.md`

```md
# Troubleshooting Guide

This document captures common issues, diagnostic reasoning and recommended resolution paths for the cashflow solution.

Because the platform is distributed and asynchronous, troubleshooting should always begin by identifying **which stage of the flow is failing**:

- ingestion;
- persistence;
- outbox registration;
- publication;
- messaging;
- worker processing;
- projection update;
- read query.

---

## 1. First Diagnostic Question

Before investigating deeply, ask:

> Is the problem in accepting source transactions, or only in reflecting them in the consolidated read model?

This distinction avoids many wrong assumptions.

A stale `daily_balance` does not necessarily mean transaction ingestion failed.

---

## 2. Transactions Are Accepted but Daily Balance Is Not Updated

### Symptoms

- `POST /api/transactions` succeeds;
- transaction exists in source data;
- `GET /api/daily-balance/{date}` does not reflect expected value.

### Likely Causes

- outbox backlog;
- publication delay or failure;
- Service Bus outage or routing issue;
- worker failure;
- projection update failure;
- eventual-consistency lag window not yet elapsed.

### Resolution Path

1. confirm transaction exists in the source table;
2. confirm integration intent/outbox record exists;
3. confirm publication happened or is pending;
4. inspect Service Bus health and backlog;
5. inspect Consolidation Service logs;
6. verify batch/failure tracking data;
7. inspect `daily_balance` update outcome.

### Architectural Interpretation

This scenario often indicates degraded projection flow, not necessarily failure of the write path.

---

## 3. Service Bus Is Unavailable

### Symptoms

- publication cannot proceed;
- worker stops receiving new work;
- balance projection stops advancing;
- ingestion may still be functioning.

### Expected Architectural Behavior

- accepted transactions remain stored;
- outbox retains pending propagation intent;
- consolidation is delayed;
- platform degrades through lag.

### Resolution Path

1. restore Service Bus health;
2. verify pending outbox backlog;
3. confirm publication resumes;
4. monitor worker catch-up behavior;
5. validate projection freshness after recovery.

### Architectural Interpretation

This is one of the intended resilience scenarios of the design.

---

## 4. Consolidation Service Fails Repeatedly

### Symptoms

- repeated worker exceptions;
- retries increase;
- projection stops moving forward;
- failed/reconciliation-oriented records appear.

### Likely Causes

- invalid message payload;
- source data inconsistency;
- database connectivity issue;
- projection update logic failure;
- incompatible contract evolution.

### Resolution Path

1. inspect worker startup and runtime logs;
2. confirm message structure matches expected contract;
3. confirm referenced transactions exist;
4. verify database connectivity and permissions;
5. inspect failed-item records;
6. isolate whether the failure is transient or persistent.

### Architectural Interpretation

The important architectural success criterion here is that transaction ingestion should still remain isolated from this failure.

---

## 5. Deployment Fails in Azure Functions

### Symptoms

- CI/CD build succeeds;
- artifact upload succeeds;
- Function App deploy or trigger sync fails;
- app starts with missing metadata/runtime structure.

### Likely Causes

- wrong publish directory;
- missing runtime files;
- invalid Azure Functions publish output;
- malformed package structure.

### Resolution Path

1. ensure `dotnet publish` is used rather than only `dotnet build`;
2. verify generated output includes required Azure Functions files;
3. validate publish artifact structure before deploy;
4. inspect deployment logs carefully;
5. verify function metadata presence.

---

## 6. Application Starts but Fails to Read Secrets

### Symptoms

- startup failure;
- dependency configuration missing;
- Key Vault access-related errors;
- connection strings not resolved.

### Likely Causes

- identity misconfiguration;
- missing Key Vault permissions;
- wrong secret name;
- wrong environment binding.

### Resolution Path

1. verify managed identity or federated identity setup;
2. confirm Key Vault access policy / RBAC;
3. confirm secret name and expected configuration binding;
4. inspect startup logs for exact failure point.

---

## 7. Database Connectivity Issues

### Symptoms

- API or worker startup failure;
- query or migration errors;
- timeouts or authentication failures.

### Likely Causes

- wrong connection string;
- firewall/network issue;
- invalid credentials;
- migration drift;
- unavailable PostgreSQL instance.

### Resolution Path

1. validate connection string source;
2. confirm database reachability;
3. verify credentials;
4. check migration history and schema alignment;
5. review cloud networking rules.

---

## 8. Daily Balance Looks Incorrect

### Symptoms

- query succeeds;
- result exists;
- totals or balance do not match expectation.

### Likely Causes

- projection logic defect;
- duplicate processing scenario;
- missing transaction in projection batch;
- misunderstanding of eventual-consistency timing;
- source-data edge case.

### Resolution Path

1. inspect source transactions for the target date;
2. verify aggregation inputs;
3. inspect batch processing history;
4. confirm whether the transaction had already been consolidated;
5. validate projection update logic and idempotency assumptions.

### Architectural Interpretation

This is a correctness/reconciliation problem, not necessarily an availability problem.

---

## 9. CI/CD Succeeds but Runtime Behavior Is Still Broken

### Symptoms

- pipeline is green;
- deployment completed;
- business flow still fails.

### Likely Causes

- configuration mismatch;
- missing secret/runtime binding;
- environment-specific dependency issue;
- background flow misconfiguration;
- migration not applied as expected.

### Resolution Path

1. verify application startup logs;
2. verify environment configuration;
3. verify secret resolution;
4. confirm database schema state;
5. send a controlled test transaction;
6. trace full path from ingestion to projection.

---

## 10. How to Triage Faster

Use this simplified decision tree:

### A. Did the transaction get accepted?

- if no → investigate API, validation, database, configuration.
- if yes → go to B.

### B. Was the transaction stored?

- if no → investigate persistence and transaction boundary.
- if yes → go to C.

### C. Was integration intent recorded?

- if no → investigate outbox persistence.
- if yes → go to D.

### D. Was the message published/consumed?

- if no → investigate publisher, Service Bus and routing.
- if yes → go to E.

### E. Did the worker update `daily_balance`?

- if no → investigate worker processing and projection update.
- if yes → investigate query expectations and correctness.

---

## 11. Current Troubleshooting Maturity

The solution already supports meaningful diagnosis because it has:

- explicit service boundaries;
- explicit asynchronous flow;
- persistence-based source of truth;
- projection-based read model;
- operationally distinct processing stages.

That said, troubleshooting maturity can still improve significantly with:

- richer dashboards;
- backlog alerts;
- correlation IDs across the full pipeline;
- replay tooling;
- better runbooks for common incidents.

---

## 12. Final Guidance

Never troubleshoot the platform as if it were a single synchronous transaction.

It is a distributed flow.

The fastest and most accurate diagnosis usually comes from identifying **which stage stopped progressing** rather than assuming the whole platform failed at once.
