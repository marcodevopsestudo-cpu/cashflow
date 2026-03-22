# Consolidation Service

Consolidation Service is the asynchronous daily-balance processor for the software architect challenge. It receives batches of transaction identifiers from Azure Service Bus, loads the transactions from PostgreSQL, aggregates credit and debit amounts by day, updates the `daily_balance` read model, records the batch lifecycle in `daily_batch`, and isolates unrecoverable items in `transaction_processing_error` for manual follow-up.

## Why this service exists

The challenge requires:

- a transaction control service;
- a daily consolidation service; and
- resilience so the transaction control service stays available even if consolidation fails.

This implementation keeps the write path in **Transaction Service** and moves the consolidation responsibility to a dedicated background service. The public balance endpoint can stay in Transaction Service and read only the materialized `daily_balance` table.

## Key decisions

- **Dedicated asynchronous worker** instead of synchronous consolidation.
- **Batch messages** containing transaction ids instead of one message per transaction.
- **Manual pipeline orchestration** instead of System.Reactive to keep the code easier to review.
- **MediatR + FluentValidation** for the same application flow style used in Transaction Service.
- **Application Insights + structured logs** with `CorrelationId`, `BatchId`, `MessageId`, and transaction counts.
- **Bounded retry with exponential backoff** and no infinite reprocessing.
- **Manual review lane** through `transaction_processing_error` when retries are exhausted.

## Solution structure

```text
src/
  ConsolidationService.Domain/
  ConsolidationService.Application/
  ConsolidationService.Infrastructure/
  ConsolidationService.Worker/
tests/
  ConsolidationService.Application.Tests/
docs/
scripts/sql/
```

## Processing flow

1. Transaction Service publishes a batch message to Service Bus.
2. Consolidation Service receives the message.
3. The Function trigger creates a `ProcessConsolidationBatchCommand` and sends it through MediatR.
4. The handler delegates execution to `IConsolidationWorkflow`.
5. The workflow executes these steps:
   - register or load the batch;
   - load pending transactions;
   - aggregate transactions by date;
   - update `daily_balance` with upsert semantics;
   - mark transactions as consolidated;
   - finalize the batch.
6. If processing fails, the retry policy applies exponential backoff.
7. If retries are exhausted, the workflow records a manual-review item in `transaction_processing_error` and marks the batch as failed.

## Message contract

```json
{
  "batchId": "4e2f4ba2-9c1a-4178-a65c-7b1e9fe44cb9",
  "correlationId": "c5307e1d706040a2b066e8a0a1e1df0d",
  "publishedAtUtc": "2026-03-22T15:30:00Z",
  "transactionIds": [101, 102, 103, 104]
}
```

## Local configuration

Create `src/ConsolidationService.Worker/local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBusConnection": "<connection-string>",
    "ServiceBusTopicName": "daily-consolidation",
    "ServiceBusSubscriptionName": "consolidation-service",
    "Postgres:ConnectionString": "Host=localhost;Port=5432;Database=challenge;Username=postgres;Password=postgres",
    "ApplicationInsights:ConnectionString": "<connection-string>"
  }
}
```

## Run locally

1. Create the PostgreSQL objects using the scripts inside `scripts/sql`.
2. Start Azurite or provide an Azure Storage connection.
3. Start the Function App:
   - `func start --csharp`
4. Publish one valid batch message to the configured topic.
5. Inspect the database tables:
   - `daily_batch`
   - `daily_balance`
   - `transaction_processing_error`

## Tests

The test project focuses on the application layer and the orchestration flow.

```bash
dotnet test
```

## Documentation

- [Architecture](docs/architecture.md)
- [Workflow](docs/workflow.md)
- [Operational behavior](docs/operational-behavior.md)
- [Database design](docs/database.md)

## Suggested deployment

For this challenge, Azure Functions (.NET isolated, Flex Consumption) is the most balanced hosting model for the consolidation worker because it provides auto-scale, low operational overhead, and event-driven processing without running dedicated compute all the time.
