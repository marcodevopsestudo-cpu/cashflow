# Non-Functional Requirements

## Performance

The system must support high throughput and low latency for both transaction processing and balance queries.

- The Transaction Service must handle write operations efficiently without blocking.
- The Consolidation Service must process transactions asynchronously to avoid impacting write performance.
- The Daily Balance endpoint (`/api/day-balance/{date}`) must return results quickly using indexed queries.
- The architecture removes the balance query from the transaction processing path.

The requirement of handling at least 50 requests per second with up to 5% data loss is addressed architecturally by decoupling write and read operations.

Balance queries are served from a pre-aggregated, indexed data store, ensuring constant-time reads independent of transaction ingestion rate.

Transaction processing occurs asynchronously and can scale horizontally, eliminating contention between write throughput and read performance.

As a result, the system is not constrained by synchronous processing limits and can sustain higher throughput without impacting availability.

## Scalability

The system must scale horizontally to handle increased load.

- Transaction processing is decoupled via messaging, allowing independent scaling.
- The Consolidation Service can scale out to process multiple transactions in parallel.
- Database indexing ensures efficient read performance as data grows.

## Availability

The system must remain available even if parts of it fail.

- The Transaction Service is independent of the Consolidation Service.
- If consolidation fails, transactions are still accepted and stored.
- Message-based communication ensures retry capability and eventual processing.

## Reliability

The system must guarantee data integrity and prevent data loss.

- The Outbox Pattern ensures that events are reliably published.
- Idempotency mechanisms prevent duplicate transaction processing.
- Failed transactions are captured and can be reviewed manually.

## Consistency

The system adopts eventual consistency.

- Transaction writes are immediately persisted.
- Balance calculations are updated asynchronously.
- Temporary discrepancies between transactions and balances are acceptable.

## Observability

The system must provide sufficient logging and tracing.

- Correlation IDs are used to trace requests across services.
- Structured logging is implemented for better monitoring.
- Application Insights integration provides telemetry data.

## Security

The system must enforce authentication and authorization.

- Integration with Microsoft Entra ID ensures secure access.
- API endpoints validate tokens and roles.
- Sensitive configuration is managed via secure storage.

## Maintainability

The system must be easy to maintain and evolve.

- Clear separation of concerns (API, Application, Domain, Infrastructure).
- Use of dependency injection and abstractions.
- Modular design allows extension without impacting existing functionality.

## Deployability

The system must support automated deployments.

- Infrastructure is provisioned using Terraform.
- CI/CD pipelines are implemented using GitHub Actions.
- Services can be deployed independently.

## Disaster Recovery

The system must support recovery from failures.

- Persistent storage ensures data durability.
- Message retries allow recovery from transient failures.
- Services can be restarted without data loss.
