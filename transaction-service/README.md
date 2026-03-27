---

# 2) `transaction-service/README.md`

```md
# Transaction Service

## Overview

The Transaction Service is the synchronous entry point of the cashflow solution.

It is responsible for accepting financial transactions, persisting the source-of-truth data, registering integration intent through the outbox pattern and exposing read endpoints for both raw transactions and consolidated daily balance.

This service was intentionally designed to be:

- fast on the write path;
- resilient to downstream failures;
- operationally simple during request execution;
- independent from asynchronous consolidation.

In other words, the transaction API should continue working even when the consolidation path is degraded.

---

## Main Responsibilities

- receive transactions via HTTP;
- validate request payload and business input;
- persist transactions in PostgreSQL;
- persist the outbox event as part of the same consistency boundary;
- expose transaction lookup endpoint;
- expose daily balance lookup endpoint backed by the consolidated read model;
- avoid synchronous dependency on balance processing.

---

## Why This Service Exists

The challenge requires a system capable of:

- registering financial transactions;
- making daily consolidated balance available;
- preserving availability even if the consolidation side fails.

To satisfy that, this service focuses on the write path and deliberately avoids performing heavy balance processing inline during request handling.

That decision improves:

- availability;
- throughput behavior;
- response-time predictability;
- resilience to downstream outages.

---

## Endpoints

### Register transaction

`POST /api/transactions`

Registers a new financial transaction.

Expected architectural behavior:

- validate request;
- persist transaction;
- register outbox event;
- return without waiting for consolidation.

### Get transaction by id

`GET /api/transactions/{id}`

Retrieves a previously registered transaction from the source data store.

### Get daily balance by date

`GET /api/daily-balance/{date}`

Retrieves the pre-aggregated daily balance read model.

Important note:

- this endpoint reads consolidated data;
- the result is eventually consistent;
- newly accepted transactions may not appear immediately until the asynchronous consolidation flow completes.

---

## Architectural Role

This service is part of a broader distributed architecture composed of:

- **Transaction Service** → synchronous ingestion and query entry point;
- **Consolidation Service** → asynchronous daily balance processor;
- **DB Migrator** → schema management;
- **Terraform infrastructure layer** → repeatable provisioning;
- **GitHub Actions pipelines** → automated validation and deployment.

The Transaction Service is the front door of the platform and therefore the primary place where write-path architectural discipline matters most.

---

## Design Principles

### 1. Availability First on the Write Path

The service is designed so that the success of transaction ingestion depends primarily on:

- API availability;
- request validation;
- PostgreSQL availability;
- successful durable persistence.

It does **not** depend on the balance worker being available at request time.

### 2. Minimal Synchronous Work

During the request lifecycle, the service should do only what is strictly necessary:

- validate;
- persist source data;
- persist integration intent.

This reduces tail latency and improves behavior under burst traffic.

### 3. Durable Integration Intent

The outbox pattern ensures that the transaction is not accepted without also recording the intention to propagate that change to the asynchronous processing pipeline.

This is essential to reduce the classic failure window where business data is committed but the event is lost.

### 4. Read/Write Separation by Intent

The service writes source transaction data, but reads daily balance from a dedicated consolidated projection.

This is not full-blown CQRS as a formal platform pattern, but it clearly applies the same reasoning:

- optimize writes for durability and throughput;
- optimize reads for query simplicity and performance.

### 5. Stateless Execution Model

The service is structured to scale horizontally more easily because request handling is centered on stateless processing plus durable persistence.

---

## Write Path Flow

At a high level, the transaction write flow is:

1. receive HTTP request;
2. validate payload and business rules;
3. persist transaction in PostgreSQL;
4. register the integration event in the outbox;
5. return success to the caller;
6. let asynchronous publication and consolidation happen afterward.

This ordering is deliberate.

It protects the caller from downstream latency and isolates the write path from asynchronous processing instability.

---

## Read Path Strategy

The service exposes two distinct read styles:

### Transaction Read

Reads the source-of-truth transaction data.

### Daily Balance Read

Reads the pre-aggregated `daily_balance` projection.

This projection-based read strategy provides:

- simpler query path;
- lower read cost;
- predictable response behavior;
- better scalability for repeated balance queries.

---

## Resilience Strategy

### If the Consolidation Service Fails

The transaction API can continue receiving and storing valid transactions.

Impact:

- source data remains safe;
- balance projection may become stale temporarily;
- recovery can happen later without losing accepted source transactions.

### If Azure Service Bus Fails Temporarily

The service still benefits from the outbox approach.

Practical effect:

- accepted transactions remain stored;
- publication may be delayed;
- consolidation is delayed;
- the system degrades with lag rather than immediate functional collapse of the write path.

### If Downstream Processing Lags

The write path remains protected, because it is not synchronously coupled to the read-model update.

This is one of the central architectural strengths of the solution.

---

## Concurrency and Throughput

The transaction service was designed to behave well under concurrent writes because:

- it does not synchronously calculate balance;
- it performs bounded work per request;
- it relies on durable persistence plus asynchronous continuation;
- it avoids downstream coupling in the hot path.

The practical validation performed with concurrent requests reinforces this architectural choice.

That validation should be interpreted as implementation evidence for the MVP, not as a formal production performance benchmark.

---

## Patterns Used

- Transactional Outbox Pattern
- Event-Driven Architecture
- Pre-aggregated Read Model
- Failure Isolation
- Stateless API design
- Asynchronous downstream processing
- Infrastructure as Code support for deployment consistency

---

## Observability Perspective

From an operational point of view, this service should provide visibility into:

- request volume;
- success/error rate;
- latency;
- outbox backlog creation rate;
- correlation between accepted writes and downstream processing;
- failures in storage or publication preparation.

The current platform already includes Application Insights as an observability foundation.

Next evolution steps would include richer dashboards, alerting and stronger end-to-end correlation.

---

## Security and Platform Concerns

This service runs within a platform that already demonstrates important security-oriented choices:

- secret management through Key Vault;
- Azure authentication from CI/CD using OIDC;
- Entra-based identity components in the platform;
- separation between deployment automation and application code.

Future hardening can evolve around:

- private networking;
- stricter RBAC review;
- environment policy enforcement;
- deeper security observability.

---

## Local Execution

Typical local execution flow:

1. start PostgreSQL;
2. apply schema migrations with DB Migrator;
3. configure local settings;
4. start the Transaction Service;
5. optionally start the Consolidation Service as well;
6. send transactions through the API;
7. verify transaction persistence and downstream behavior.

---

## What This Service Does Not Try To Do

To keep the architecture healthy, this service intentionally does **not** try to:

- calculate daily balance synchronously on every request;
- depend on the worker to respond to callers;
- make ingestion success contingent on immediate messaging success;
- centralize all business concerns into a single bloated service.

Those omissions are not limitations by accident.
They are deliberate architectural decisions.

---

## Current Strengths

- protects the write path from downstream instability;
- keeps request handling lightweight;
- uses durable outbox registration;
- exposes both source and projected reads clearly;
- aligns well with burst traffic and asynchronous processing;
- fits well with serverless deployment and horizontal scaling.

---

## Future Improvements

Potential next steps include:

- stronger end-to-end idempotency contracts;
- richer request and outbox observability;
- replay/reconciliation tooling;
- contract versioning for integration messages;
- broader automated integration tests;
- more formal performance testing;
- tighter security and networking hardening.

---

## Final Positioning

The Transaction Service is the availability-oriented boundary of the platform.

Its role is not to do everything.
Its role is to do the **right synchronous work**, persist it durably and allow the rest of the system to continue asynchronously.

That is exactly what makes the architecture more resilient, scalable and architecturally mature.
