# Operations Guide

## Overview

This document describes how to operate, validate and diagnose the system.

The solution combines:

- synchronous API ingestion
- asynchronous message processing
- projection-based read model

Understanding these flows is essential for correct operation and troubleshooting.

---

## Local Execution Flow

Recommended order:

1. start PostgreSQL
2. apply migrations using DB Migrator
3. start Transaction Service
4. start Consolidation Service
5. send test transactions
6. validate persistence and consolidation

---

## Validation Checklist

### Transaction Ingestion

- API is reachable
- transaction request succeeds
- transaction is persisted

### Outbox

- outbox record is created
- integration intent is registered

### Messaging

- message is published
- message is consumed

### Consolidation

- transactions are processed
- daily balance is updated

### Query

- daily balance endpoint returns correct data

---

## Stress Test (Postman)

A manual stress test was executed using a Postman collection.

### Setup

- 100 transaction requests
- executed concurrently using async/parallel execution
- endpoint tested: `POST /api/transactions`

### Observed Behavior

- all requests were successfully accepted
- all transactions were persisted
- all transactions were consolidated
- no data inconsistency was observed

### Architectural Interpretation

This validates that:

- the write path is lightweight and resilient
- asynchronous processing absorbs concurrent load
- the system behaves correctly under parallel execution
- ingestion is not blocked by consolidation

### Limitations

- not a formal performance benchmark
- no latency metrics collected
- no throughput saturation measurement

This test validates **correctness and resilience under concurrency**, not maximum system capacity.

---

## Runtime Flow

1. client sends transaction
2. transaction is persisted
3. outbox registers integration intent
4. message is published
5. consolidation service processes data
6. daily balance is updated
7. query returns consolidated result

---

## Degraded Mode

### If Consolidation Fails

- transactions are still accepted
- balance becomes stale
- system recovers later

### If Service Bus Fails

- transactions may still be accepted
- consolidation is delayed
- system degrades gracefully

---

## Monitoring Focus

Monitor:

- API success rate and latency
- database connectivity
- outbox backlog
- messaging flow
- worker processing success/failure
- daily balance freshness

---

## Deployment

Deployment is automated via GitHub Actions:

1. build
2. test
3. run migrations
4. deploy services

---

## Troubleshooting Summary

If something fails, check in order:

1. transaction persisted?
2. outbox created?
3. message published?
4. message consumed?
5. consolidation executed?
6. read model updated?

---

## Operational Maturity

Current solution provides:

- reproducible infrastructure
- automated deployment
- clear separation of concerns
- resilient architecture

Future improvements:

- monitoring dashboards
- alerting
- replay tooling
- DR and failover strategies

---

## Final Note

Always analyze the system in two parts:

- write path (ingestion)
- read model (consolidation)

They are intentionally decoupled and behave differently under load or failure.
