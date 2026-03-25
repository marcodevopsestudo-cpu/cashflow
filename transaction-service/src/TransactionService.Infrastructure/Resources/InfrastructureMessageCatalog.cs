namespace TransactionService.Infrastructure.Resources;

/// <summary>
/// Centralizes infrastructure-layer messages.
/// </summary>
public static class InfrastructureMessageCatalog
{
    /// <summary>
    /// Message indicating that the PostgreSQL connection string was not found.
    /// </summary>
    public const string PostgresConnectionStringNotFound =
        "Postgres connection string not found. Configure ConnectionStrings__Postgres.";

    /// <summary>
    /// Message indicating that the Service Bus namespace was not configured when using managed identity.
    /// </summary>
    public const string ServiceBusNamespaceNotFound =
        "ServiceBus namespace not found. Configure ServiceBus__FullyQualifiedNamespace when UseManagedIdentity is true.";

    /// <summary>
    /// Message indicating that the Service Bus connection string was not configured when not using managed identity.
    /// </summary>
    public const string ServiceBusConnectionStringNotFound =
        "Service Bus connection string not found. Configure ServiceBus__ConnectionString when UseManagedIdentity is false.";

    /// <summary>
    /// Message indicating that the idempotency key cannot be null or whitespace.
    /// </summary>
    public const string IdempotencyKeyCannotBeNullOrWhitespace =
        "Idempotency key cannot be null or whitespace.";

    /// <summary>
    /// Message indicating that an integration event is being published.
    /// </summary>
    public const string PublishingIntegrationEvent =
        "Publishing integration event {EventName} with event id {EventId}";

    /// <summary>
    /// Contains log message templates used across the infrastructure layer.
    /// </summary>
    public static class Logs
    {
        /// <summary>
        /// Log message indicating that a transaction batch is being sent to Service Bus.
        /// </summary>
        public const string SendingTransactionBatch =
            "Sending transaction batch to Service Bus. Namespace={Namespace}, Topic={Topic}, BatchId={BatchId}, Count={Count}";

        /// <summary>
        /// Log message indicating that a transaction batch was successfully published.
        /// </summary>
        public const string TransactionBatchPublished =
            "Transaction batch published successfully. BatchId={BatchId}, Count={Count}";
    }
}
