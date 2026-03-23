namespace TransactionService.Application.Abstractions.Messaging;

/// <summary>
/// Represents a batch of stored integration events to be published as a single message.
/// Used to optimize throughput when processing outbox messages.
/// </summary>
/// <param name="BatchId">The unique identifier of the batch.</param>
/// <param name="EventName">The logical name of the batch event.</param>
/// <param name="EventVersion">The version of the batch event contract.</param
/// <param name="OccurredOnUtc">The UTC timestamp when the batch was created.</param>
/// <param name="Transactions">The collection of integration events contained in the batch.</param>
public sealed record StoredIntegrationEventBatch(
    Guid BatchId,
    string EventName,
    int EventVersion,
    DateTime OccurredOnUtc,
    IReadOnlyCollection<StoredIntegrationEventBatchItem> Transactions);
