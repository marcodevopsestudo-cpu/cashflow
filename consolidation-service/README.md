# Consolidation Service

## Overview

The Consolidation Service is the asynchronous daily-balance processor of the cashflow solution.

It exists to transform accepted source transactions into a queryable daily balance read model without reducing the availability of the synchronous transaction ingestion path.

In architectural terms, this service is responsible for the **derived-data side** of the system:

- consuming integration messages;
- loading source transactions;
- aggregating credits and debits by date;
- updating the `daily_balance` projection;
- tracking processing lifecycle;
- isolating unrecoverable failures for manual follow-up.

---

## Why This Service Exists

A naive implementation could calculate balance synchronously during every write request.

That approach would create undesirable coupling between:

- API response time;
- downstream processing;
- read-model update;
- resilience to transient infrastructure problems.

Instead, this solution adopts a dedicated asynchronous worker so that:

- transaction ingestion remains lightweight;
- consolidation can scale independently;
- retries can happen safely in the background;
- temporary downstream issues do not make the API unavailable.

This service is therefore a deliberate architectural boundary, not just a technical split.

---

## Main Responsibilities

- consume messages from Azure Service Bus;
- validate and deserialize integration messages;
- load referenced transactions from PostgreSQL;
- group and aggregate transactions by balance date;
- update the `daily_balance` table using safe projection logic;
- track processing batches and lifecycle state;
- mark successful processing;
- retry transient failures with bounded strategy;
- isolate exhausted or unrecoverable items for manual reconciliation.

---

## Architectural Role

The Consolidation Service owns the asynchronous projection flow.

That means it is responsible for converting source business events into a read model optimized for query.

The service should be understood as:

- **latency-tolerant**;
- **retry-capable**;
- **failure-isolating**;
- **eventually consistent by design**.

This is important because the success criteria for this service are different from the success criteria of the API.

The API is optimized for immediate availability.
The worker is optimized for eventual completion, recoverability and operational safety.

---

## Input and Processing Strategy

The service consumes integration messages that reference transactions to be consolidated.

At a conceptual level, the worker:

1. receives a message;
2. validates its structure;
3. loads current processing context/batch state;
4. loads source transactions by identifier;
5. aggregates credit and debit data;
6. updates the `daily_balance` projection;
7. finalizes successful work;
8. records or isolates failures if needed.

This flow is intentionally explicit because asynchronous systems need traceable processing stages.

---

## Why a Dedicated Projection Worker Was Chosen

### Separation of Concerns

Daily balance is derived data, not the primary source of truth.

The source of truth is the transaction record itself.

By separating those concerns:

- the write path stays simpler and more available;
- the projection logic becomes independently evolvable;
- recovery and replay become more practical.

### Scalability

The worker can evolve separately from the API.

Possible future evolutions include:

- different scaling thresholds;
- multiple subscriptions/consumers for derived workloads;
- richer reconciliation flows;
- projection rebuild or replay tooling.

### Resilience

Projection failure should not imply transaction loss.

This service supports that principle by treating consolidation as a recoverable asynchronous concern.

---

## Eventual Consistency

The `daily_balance` read model is eventually consistent.

That means:

- a newly accepted transaction may not appear immediately in balance queries;
- the lag window depends on asynchronous publication and processing progress;
- temporary processing backlog is acceptable within the architectural trade-off.

This is not a defect in the design.

It is the chosen consistency model that enables better availability and operational decoupling.

---

## What Happens If Service Bus Is Unavailable?

This is a key scenario.

If the messaging layer becomes temporarily unavailable:

- new source transactions can still be accepted and stored by the write path;
- pending integration intent remains preserved through the outbox approach;
- consolidation is delayed;
- the worker cannot process new messages until flow is restored;
- once the pipeline is healthy again, backlog can be processed.

This means the system degrades primarily by **projection delay**, not by immediate loss of accepted transaction data.

That is an intentional resilience characteristic.

---

## Failure Handling Strategy

The worker is designed to handle failure as part of normal distributed-system behavior.

### Transient Failures

Examples:

- temporary network issues;
- short-lived infrastructure instability;
- database contention;
- transient messaging issues.

Desired behavior:

- retry with bounded strategy;
- avoid immediate manual intervention when recovery is likely.

### Persistent or Unrecoverable Failures

Examples:

- invalid payload shape;
- irreconcilable data issue;
- exhausted retry attempts;
- semantic processing failure that cannot self-heal.

Desired behavior:

- isolate the failed item or batch;
- preserve traceability;
- allow manual follow-up and reconciliation.

This is more mature than either:

- pretending failure will not happen; or
- retrying forever without control.

---

## Batch-Oriented Processing

The worker processes referenced transactions through a batch-oriented lifecycle.

That provides benefits such as:

- lower orchestration overhead than fully isolated one-message-per-transaction orchestration;
- better processing traceability;
- clearer operational units for retries and diagnosis;
- more natural support for grouped daily aggregation.

At the same time, the architecture still aims to avoid losing visibility into individual failures.

This balance is important in financial-style processing systems.

---

## Read Model Update Strategy

The `daily_balance` table is treated as a read model projection.

It exists to support efficient queries, not to replace the transaction source data.

The projection is updated from already accepted source transactions and therefore can be:

- rebuilt in principle from source data;
- reconciled if needed;
- monitored independently from ingestion.

This distinction is architecturally important because it avoids confusing derived state with authoritative state.

---

## Domain and Design Considerations

From a design perspective, the worker reflects a layered and domain-oriented processing style:

- application layer orchestrates steps;
- domain-oriented concepts drive aggregation semantics;
- infrastructure handles persistence, messaging and execution hosting.

That separation makes the service easier to evolve in areas such as:

- aggregation rules;
- error policies;
- replay mechanisms;
- observability;
- resilience tuning.

A future improvement could be the careful extraction of truly shared abstractions into a core package, but only where it strengthens cohesion without over-centralizing unrelated concerns.

---

## Observability Perspective

For asynchronous processing, observability is critical.

This service should support visibility into:

- message consumption rate;
- processing success/failure rate;
- backlog growth;
- retry volume;
- lag between ingestion and consolidation;
- exhausted items requiring manual reconciliation.

The current platform already includes foundational observability support through Application Insights.

Future maturity steps would include:

- worker-centric dashboards;
- backlog and lag alerts;
- business-health indicators for daily balance freshness;
- stronger correlation across API, outbox, messaging and projection layers.

---

## Operational Maturity

This service already demonstrates important operational characteristics for an MVP:

- asynchronous isolation from the API;
- explicit processing lifecycle;
- retry-oriented thinking;
- failure tracking and reconciliation direction;
- suitability for scale-out and recoverability evolution.

However, this is still an MVP and not yet a fully production-hardened financial processing platform.

The next maturity steps would include:

- replay tooling;
- stronger poison-message operational flow;
- formal reprocessing procedures;
- projection rebuild strategy;
- SLO-based monitoring;
- deeper DR/BC playbooks;
- larger-scale automated load validation.

---

## Strengths of the Current Design

- protects the write path from projection failures;
- embraces eventual consistency explicitly;
- supports batch-oriented asynchronous processing;
- isolates persistent failures for follow-up;
- aligns well with event-driven architecture;
- creates a strong foundation for future replay and reconciliation.

---

## Future Improvements

- stronger contract versioning for integration events;
- more formal idempotent replay guarantees;
- dedicated reconciliation/rebuild tooling for projections;
- richer end-to-end telemetry and correlation;
- operational dashboards and alert thresholds;
- more explicit runbooks for poison messages and backlog recovery;
- advanced resilience and DR strategies across environments.

---

## Final Positioning

The Consolidation Service is where the architecture turns accepted source transactions into business-facing daily balance data.

Its purpose is not to be instantaneous.
Its purpose is to be **recoverable, scalable and operationally safe**.

That is why it is asynchronous, why it is isolated from the write path and why it is one of the central architectural pieces of the solution.
