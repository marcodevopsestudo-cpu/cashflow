namespace TransactionService.Application.Abstractions.Messaging;

/// <summary>
/// Represents a single stored integration event contained within a batch payload.
/// </summary>
/// <param name="EventId">The unique identifier of the event.</param>
/// <param name="EventName">The logical name of the event.</param>
/// <param name="EventVersion">The version of the event contract.</param>
/// <param name="AggregateId">The identifier of the aggregate that produced the event.</param>
/// <param name="CorrelationId">The correlation identifier used for tracing across services.</param>
/// <param name="OccurredOnUtc">The UTC timestamp when the event occurred.</param>
/// <param name="Payload">The serialized payload of the event.</param>
public sealed record StoredIntegrationEventBatchItem(
    Guid EventId,
    string EventName,
    int EventVersion,
    string AggregateId,
    string CorrelationId,
    DateTime OccurredOnUtc,
    string Payload);
