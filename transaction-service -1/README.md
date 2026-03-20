# Transaction Service

Azure Functions isolated solution for the first service in the event-driven architecture.

## Included

- `POST /api/transactions`
- `GET /api/transactions/{transactionId}`
- MediatR command/query flow
- PostgreSQL persistence with EF Core
- Azure Service Bus publisher
- centralized messages
- custom error model
- structured logging
- XML docs on public contracts
- database scripts
- Dockerfile

## Azure Function or API?

This is an HTTP API hosted as an Azure Function.  
So, yes: it behaves like an API, but the hosting model is Azure Functions isolated worker.

## About Docker

A Dockerfile is included as an optional artifact. Azure Functions does not require it, but it is useful for CI/CD consistency and container-based hosting.
