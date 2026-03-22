namespace TransactionService.Application.Abstractions.Messaging;

/// <summary>
/// Represents a stored integration event ready to be published.
/// </summary>
/// <param name="EventId">The event identifier.</param>
/// <param name="EventName">The event name.</param>
/// <param name="EventVersion">The event version.</param>
/// <param name="AggregateId">The aggregate identifier.</param>
/// <param name="CorrelationId">The correlation identifier.</param>
/// <param name="OccurredOnUtc">The UTC timestamp when the event occurred.</param>
/// <param name="Payload">The serialized event payload.</param>
public sealed record StoredIntegrationEvent(
    Guid EventId,
    string EventName,
    int EventVersion,
    string AggregateId,
    string CorrelationId,
    DateTime OccurredOnUtc,
    string Payload);
