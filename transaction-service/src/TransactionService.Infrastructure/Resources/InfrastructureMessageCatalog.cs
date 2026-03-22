namespace TransactionService.Infrastructure.Resources;

/// <summary>
/// Centralizes infrastructure-layer messages.
/// </summary>
public static class InfrastructureMessageCatalog
{
    public const string PostgresConnectionStringNotFound =
        "Postgres connection string not found. Configure ConnectionStrings__Postgres.";

    public const string ServiceBusNamespaceNotFound =
        "ServiceBus namespace not found. Configure ServiceBus__FullyQualifiedNamespace when UseManagedIdentity is true.";

    public const string ServiceBusConnectionStringNotFound =
        "Service Bus connection string not found. Configure ServiceBus__ConnectionString when UseManagedIdentity is false.";

    public const string IdempotencyKeyCannotBeNullOrWhitespace =
        "Idempotency key cannot be null or whitespace.";

    public const string PublishingIntegrationEvent =
        "Publishing integration event {EventName} with event id {EventId}";
}
