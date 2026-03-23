# Cashflow Challenge Solution

## Executive Summary

This repository contains a software architect challenge solution for a cashflow scenario with **two business services** and **one operational support component**:

1. **Transaction Service**
   Responsible for receiving debit and credit transactions, validating requests, persisting data in PostgreSQL, and registering integration events through the outbox pattern.

2. **Consolidation Service**
   Responsible for asynchronously processing published transaction batches and updating the **daily consolidated balance** read model.

3. **DB Migrator**
   Responsible for applying ordered SQL migrations to PostgreSQL in a deterministic and auditable way.

The solution is intentionally designed so that the **transaction write path remains available even if the consolidation flow is delayed or temporarily unavailable**.

---

## Challenge Scope Coverage

The challenge asks for:

- a service to control financial transactions;
- a service to generate the daily consolidated balance;
- clear architectural decisions;
- code and documentation in the repository;
- tests and execution guidance.

This repository addresses that scope with:

- **Transaction Service** for the synchronous write path;
- **Consolidation Service** for the asynchronous daily balance processing path;
- **DB Migrator** for controlled schema evolution;
- architecture and operational documentation at the repository root and inside each service folder.

---

## Current Implementation Status

### Fully Functional

#### Transaction Service

Implemented capabilities:

- `POST /api/transactions`
- `GET /api/transactions/{transactionId}`
- PostgreSQL persistence
- request-level idempotency
- transactional outbox persistence
- scheduled outbox publication
- Application Insights integration hooks
- layered structure with Domain / Application / Infrastructure / API

This service is the **main synchronous entry point** and is already positioned as the most complete part of the solution.

### Functional MVP

#### Consolidation Service

Implemented capabilities:

- asynchronous batch consumption
- batch lifecycle tracking
- transaction loading from PostgreSQL
- daily aggregation by date
- upsert into `daily_balance`
- processed transaction marking
- bounded retry behavior
- manual review registration for exhausted failures
- layered structure with Domain / Application / Infrastructure / Worker

This service is **implemented and operational as an MVP**, focused on the challenge’s daily consolidation requirement and on showing architectural separation, resilience, and recoverability.

### Operational Support Component

#### DB Migrator

Implemented capabilities:

- ordered SQL execution
- migration history table
- checksum validation
- fail-fast behavior for migration drift

---

## What Is Implemented End-to-End

The repository already implements the following **end-to-end business flow**:

1. a client sends a debit or credit request to **Transaction Service**;
2. Transaction Service validates and persists the transaction in PostgreSQL;
3. in the same database transaction, the service records an **outbox event**;
4. a scheduled publisher reads pending outbox events and publishes a batch message;
5. **Consolidation Service** consumes the message asynchronously;
6. the consolidation workflow loads pending transactions;
7. the workflow aggregates credit and debit amounts by date;
8. the workflow updates the `daily_balance` read model;
9. the workflow marks transactions as consolidated;
10. the daily consolidated balance becomes available in the database read model.

This means the repository is not only showing isolated services. It already demonstrates the intended **integration pattern between write path and consolidation path**.

---

## High-Level Architecture

```mermaid
flowchart LR
    Client[Client / Consumer] -->|HTTP| Tx[Transaction Service]
    Tx --> Pg[(PostgreSQL)]
    Tx --> Outbox[(Outbox Table)]
    Timer[Outbox Publisher] --> Outbox
    Timer --> Bus[(Message Broker / Service Bus)]
    Bus --> Cons[Consolidation Service]
    Cons --> Pg
    Migrator[DB Migrator] --> Pg



    ## Testing Strategy

The current implementation focuses on validating the most critical business paths required by the challenge, while keeping the architecture open for more advanced testing strategies.

### What is currently implemented

- Unit tests for domain logic
- Application-layer tests for use case orchestration
- Validation of transaction persistence and outbox registration
- Validation of consolidation logic at application level

These tests ensure that the core business rules and data transformations behave as expected.

---

### What is intentionally not fully implemented (yet)

A full end-to-end integration test covering:

1. transaction ingestion
2. outbox persistence
3. message publication
4. consolidation processing
5. final daily balance verification

---

### Why this was not fully implemented

Given the challenge time constraints, the focus was placed on:

- delivering a clear architectural separation between services;
- implementing the core business flows;
- ensuring the system is resilient and decoupled;
- documenting the integration model explicitly.

A full integration test would require:

- containerized infrastructure (PostgreSQL + message broker);
- orchestration of multiple services;
- controlled timing for async processing;
- test environment isolation.

This is considered a **next iteration step**, not a blocker for validating the architectural approach.

---

### Planned evolution: Integration Testing

The next step would be to introduce automated end-to-end tests using:

- Testcontainers for PostgreSQL and message broker
- A test harness orchestrating both services
- Deterministic batch processing triggers
- Assertions over the `daily_balance` read model

#### Example scenario to be automated

1. Create transactions:
   - Credit: 100
   - Debit: 40
   - Credit: 15

2. Trigger outbox publication

3. Wait for consolidation processing

4. Assert:
   - daily balance = 75
   - transactions marked as processed
   - no processing errors

---

### Architectural readiness for integration testing

The system was designed to support this evolution:

- clear separation between services
- database schema explicitly defined
- outbox pattern enabling deterministic event flow
- consolidation workflow isolated in application layer
- ability to run services independently

This allows integration tests to be introduced without major refactoring.
```
