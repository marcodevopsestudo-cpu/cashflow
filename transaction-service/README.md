# transaction-service

Azure Functions isolated worker application responsible for recording debit and credit entries.

## Implemented capabilities

- `POST /api/transactions`
- `GET /api/transactions/{transactionId}`
- PostgreSQL persistence
- Idempotency enforcement for transaction creation
- Transactional outbox persistence
- Timer-triggered outbox publishing to Azure Service Bus
- Application Insights integration

## Local development

1. Start PostgreSQL locally or point the app to an Azure PostgreSQL instance.
2. Apply the SQL scripts through `../db-migrator`.
3. Create `src/TransactionService.Api/local.settings.json` from the template below.
4. Run the Function App.

### local.settings.json template

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

## Testing strategy

- Unit tests cover command/query handlers, authorization evaluation, request parsing and selected domain behavior.
- `db-migrator` is intended to be executed in CI/CD before the deployment step.

## Notes

- The current repository does not yet include the future daily balance consolidation service.
- The outbox timer is intentionally hosted in the same workload as the write service to keep the first delivery slice small and operationally simple.
