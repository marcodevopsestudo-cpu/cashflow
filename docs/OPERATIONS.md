# Operations Guide

## 1. Local Development

### Prerequisites

- .NET 8 SDK
- PostgreSQL instance
- Azure Functions Core Tools
- optional Azurite

### Recommended order

1. Provision or start PostgreSQL.
2. Run database migrations.
3. Configure local settings for both services.
4. Start Transaction Service.
5. Start Consolidation Service.
6. Execute test requests and validate tables/logs.

---

## 2. Running the DB Migrator

The DB Migrator reads the connection string from either:

- `ConnectionStrings__Postgres`
- `POSTGRES_CONNECTION`

The migrations path can be provided with:

- `--migrations-path=<path>`
- `MIGRATIONS_PATH`

### Example: transaction service migrations

```bash
export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=transactiondb;Username=postgres;Password=postgres"
dotnet run --project ./db-migrator/db-migrator.csproj -- --migrations-path=./transaction-service/scripts
```

### Example: consolidation service migrations

```bash
export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=transactiondb;Username=postgres;Password=postgres"
dotnet run --project ./db-migrator/db-migrator.csproj -- --migrations-path=./consolidation-service/scripts/sql
```

### What the migrator guarantees

- ordered execution by filename;
- one-time execution tracking in `__schema_migrations`;
- checksum validation to detect illegal edits in already-applied scripts;
- transaction rollback when a script fails.

---

## 3. CI/CD Positioning

The repository includes a GitHub Actions workflow for the Transaction Service pipeline.

Current deployment flow:

1. checkout code;
2. restore, build, and optionally test the service;
3. authenticate to Azure using GitHub OIDC;
4. read the PostgreSQL connection string from Key Vault;
5. run **DB Migrator** before deployment;
6. publish the Function App;
7. deploy to Azure Functions.

This ordering is intentional:

- database schema is prepared before the new application version starts;
- migrations are repeatable and owned by the pipeline;
- manual DBA execution is not required for normal changes.

---

## 4. Operational Monitoring

### Suggested telemetry to review regularly

- HTTP failures and latency in Transaction Service;
- outbox pending count and oldest pending age;
- Service Bus failures and backlog;
- consolidation retry counts;
- rows in `transaction_processing_error`;
- migration failures in pipeline logs.

### Suggested alert scenarios

- outbox not drained within expected time window;
- repeated publication failures;
- consolidation retry threshold frequently reached;
- manual error table growth above baseline;
- failed deployment after migration step.

---

## 5. Failure Handling Playbook

### If consolidation is down

Expected behavior:

- transaction ingestion continues;
- outbox messages accumulate until publication/consumption recovers.

Action:

- inspect Service Bus health, worker logs, and Application Insights traces.

### If a batch repeatedly fails

Expected behavior:

- retries stop after the configured limit;
- the failure is persisted for manual review.

Action:

- inspect batch payload, related transactions, and database state;
- correct the root cause;
- replay safely using the existing idempotency and batch controls.

### If a migration fails in CI/CD

Expected behavior:

- deployment should stop;
- the application should not move forward on a partially prepared schema.

Action:

- inspect the failing migration;
- create a corrective migration rather than editing an applied one.

---

## 6. Run Commands

### Build

```bash
dotnet build ./transaction-service/TransactionService.sln
dotnet build ./consolidation-service/ConsolidationService.sln
dotnet build ./db-migrator/db-migrator.sln
```

### Test

```bash
dotnet test ./transaction-service/TransactionService.sln
dotnet test ./consolidation-service/ConsolidationService.sln
```

### Run services locally

Use Azure Functions Core Tools from each service host project after configuring `local.settings.json`.

---

## 7. Enterprise Evolution Backlog

Operational improvements intentionally documented for future versions:

- automated replay tooling for manual-review items;
- dead-letter queues with controlled replay;
- dashboards and alerts as code;
- blue/green or staged rollout strategy;
- private networking and stricter segmentation.
