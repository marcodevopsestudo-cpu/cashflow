# Transaction Service

## Overview

The Transaction Service is responsible for receiving, storing, and exposing financial transactions.

It is designed to be **highly available, fast, and independent** from downstream processing, ensuring that transaction ingestion is never blocked by consolidation or reporting logic.

---

## Responsibilities

- Receive financial transactions via HTTP API
- Persist transactions in PostgreSQL
- Guarantee reliable event publishing using the **Outbox Pattern**
- Expose transaction query endpoints
- Expose consolidated daily balance for read operations

---

## Endpoints

- `POST /api/transactions` → Register transaction
- `GET /api/transactions/{id}` → Retrieve transaction
- `GET /api/daily-balance/{date}` → Retrieve consolidated daily balance per merchant

The daily balance endpoint reads from a **pre-aggregated table (`daily_balance`)**, ensuring fast and scalable queries.

---

## Architecture Role

This service is part of a distributed architecture composed of:

- **Transaction Service (this service)** → Handles ingestion and query
- **Consolidation Service (worker)** → Processes transactions asynchronously
- **DB Migrator** → Manages database schema

The Consolidation Service is responsible for updating the `daily_balance` table asynchronously.

---

## Design Principles

### ✔ Decoupling

The service does NOT perform any heavy computation during request handling.

Instead:

1. Transaction is persisted
2. Event is stored in Outbox
3. Event is published asynchronously

---

### ✔ High Availability

Even if the Consolidation Service is unavailable:

- Transactions are still accepted
- Events are safely stored
- Processing resumes later

---

### ✔ Performance

- Optimized for fast writes
- Stateless design
- Minimal processing during requests

---

### ✔ Eventual Consistency

The daily balance is:

- NOT updated in real-time
- Updated asynchronously (typically within seconds, up to ~30 seconds)

This trade-off ensures scalability and reliability.

---

## Data Flow

1. Client sends transaction request
2. Transaction is persisted in database
3. Outbox message is stored in the same transaction
4. Background process publishes message to Service Bus
5. Consolidation Service processes the transaction
6. `daily_balance` table is updated
7. Query endpoint retrieves pre-aggregated data

---

## How to Run

### Prerequisites

- .NET 8
- PostgreSQL
- Azure Service Bus (or emulator if configured)

---

### Run

```bash
dotnet run
```

---

## Configuration

Example (local):

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=cashflow;Username=postgres;Password=postgres"
  },
  "ServiceBus": {
    "ConnectionString": "your-service-bus-connection"
  },
  "Authorization": {
    "Enabled": false
  }
}
```

---

## Testing

Includes:

- Unit tests
- Application layer tests

---

## Notes

This service prioritizes:

- Fast ingestion
- High availability
- Clear separation from processing logic

All heavy processing is delegated to the Consolidation Service to ensure scalability.
