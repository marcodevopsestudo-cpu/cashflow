# Operations Guide

## 1. Local Execution

This section describes the recommended flow to run the system locally and validate the **end-to-end architecture behavior**, including:

- transaction ingestion
- outbox processing
- message publication
- asynchronous consolidation

---

## Prerequisites

- .NET 8 SDK
- PostgreSQL instance
- Azure Functions Core Tools
- (optional) Azurite for local storage emulation

---

## Recommended Execution Order

1. Start PostgreSQL and ensure connectivity.
2. Run database migrations using DB Migrator.
3. Configure `local.settings.json` for both services.
4. Start Transaction Service.
5. Start Consolidation Service.
6. Execute test transactions.
7. Validate results in:
   - `transactions` table
   - `outbox` table
   - `daily_batch`
   - `daily_balance`
   - `transaction_processing_error` (if failures occur)
8. Inspect logs via console or Application Insights.

---

## 2. Database Migration

The DB Migrator is responsible for preparing the database schema before services run.

It ensures:

- consistent schema across environments
- ordered execution of scripts
- traceability of applied migrations

---

## Configuration

Connection string can be provided via:

- `ConnectionStrings__Postgres`
- `POSTGRES_CONNECTION`

Migration path can be provided via:

- `--migrations-path=<path>`
- `MIGRATIONS_PATH`

---

## Example — Transaction Service Migrations

```bash
export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=transactiondb;Username=postgres;Password=postgres"

dotnet run \
  --project ./db-migrator/db-migrator.csproj \
  -- \
  --migrations-path=./transaction-service/scripts
```
