# Transaction Service

Azure Functions isolated worker application responsible for recording debit and credit transactions.

This service is the **main synchronous entry point** of the solution and is intentionally designed to remain available even if the downstream consolidation flow is delayed or unavailable.

---

## Responsibilities

- receive debit and credit requests;
- validate input;
- enforce request-level idempotency;
- persist transactions in PostgreSQL;
- persist outbox messages in the same database transaction;
- expose transaction query endpoints;
- publish pending outbox records through a timer-triggered process.

---

## Implemented Capabilities

- `POST /api/transactions`
- `GET /api/transactions/{transactionId}`
- PostgreSQL persistence
- request-level idempotency
- transactional outbox persistence
- timer-triggered outbox publication to Azure Service Bus
- Application Insights integration hooks
- layered structure with Domain / Application / Infrastructure / API

---

## Architectural Notes

### Why the outbox lives here

The outbox publisher is intentionally hosted in the same workload as the write service to keep the ingestion slice cohesive and to ensure publication can recover from temporary downstream issues without introducing a hard dependency on the consolidation runtime.

### Why this service does not wait for consolidation

The challenge requires the transaction service to remain available even if consolidation fails.
Because of that, this service acknowledges successful transaction creation after:

- durable persistence of source data;
- durable persistence of the corresponding outbox record.

Daily balance update happens asynchronously in the Consolidation Service.

---

## Security and Access

In deployed environments, this service is configured to use:

- Microsoft Entra ID authentication/authorization;
- audience, issuer, scope and allowed-client validation;
- Key Vault references for secrets;
- Managed Identity for Service Bus access.

For local development, authorization can be disabled when needed.

---

## Local Development

### Prerequisites

- .NET 8 SDK
- PostgreSQL
- Azure Functions Core Tools
- optional Azurite for local storage emulation

### Recommended Flow

1. Start PostgreSQL locally or point the app to an Azure PostgreSQL instance.
2. Apply database migrations through `../db-migrator`.
3. Create `src/TransactionService.Api/local.settings.json`.
4. Run the Function App.

### `local.settings.json` template

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ConnectionStrings__Postgres": "Host=localhost;Port=5432;Database=transactiondb;Username=postgres;Password=postgres",
    "ServiceBus__TopicName": "transaction-events",
    "ServiceBus__FullyQualifiedNamespace": "sb-example.servicebus.windows.net",
    "ServiceBus__UseManagedIdentity": "false",
    "ServiceBus__ConnectionString": "Endpoint=sb://sb-example.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=replace-me",
    "Authorization__Enabled": "false"
  }
}
```
