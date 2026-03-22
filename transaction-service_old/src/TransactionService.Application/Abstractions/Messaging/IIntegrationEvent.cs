namespace TransactionService.Application.Abstractions.Messaging;

/// <summary>
/// Defines the base contract for integration events.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Gets the event identifier.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the event name.
    /// </summary>
    string EventName { get; }

    /// <summary>
    /// Gets the event version.
    /// </summary>
    int EventVersion { get; }

    /// <summary>
    /// Gets the aggregate identifier associated with the event.
    /// </summary>
    string AggregateId { get; }

    /// <summary>
    /// Gets the correlation identifier.
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Gets the UTC timestamp when the event occurred.
    /// </summary>
    DateTime OccurredOnUtc { get; }
}
