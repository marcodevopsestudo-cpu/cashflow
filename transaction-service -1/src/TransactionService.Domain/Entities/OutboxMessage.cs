namespace TransactionService.Domain.Entities;

/// <summary>
/// Represents an outbox message persisted for deferred publication.
/// </summary>
public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    /// <summary>
    /// Gets the outbox message identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the event name.
    /// </summary>
    public string EventName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the event version.
    /// </summary>
    public int EventVersion { get; private set; }

    /// <summary>
    /// Gets the aggregate identifier associated with the event.
    /// </summary>
    public string AggregateId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the serialized payload.
    /// </summary>
    public string Payload { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the correlation identifier.
    /// </summary>
    public string CorrelationId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the UTC timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the outbox message was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the outbox message was processed.
    /// </summary>
    public DateTime? ProcessedOnUtc { get; private set; }

    /// <summary>
    /// Gets the latest processing error.
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Gets the retry count.
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    /// Creates a new outbox message instance.
    /// </summary>
    /// <param name="id">The outbox message identifier.</param>
    /// <param name="eventName">The event name.</param>
    /// <param name="eventVersion">The event version.</param>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <param name="payload">The serialized payload.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <param name="occurredOnUtc">The UTC timestamp when the event occurred.</param>
    /// <returns>A new <see cref="OutboxMessage"/> instance.</returns>
    public static OutboxMessage Create(
        Guid id,
        string eventName,
        int eventVersion,
        string aggregateId,
        string payload,
        string correlationId,
        DateTime occurredOnUtc)
    {
        return new OutboxMessage
        {
            Id = id,
            EventName = eventName,
            EventVersion = eventVersion,
            AggregateId = aggregateId,
            Payload = payload,
            CorrelationId = correlationId,
            OccurredOnUtc = occurredOnUtc,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Marks the message as processed.
    /// </summary>
    public void MarkAsProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
        Error = null;
    }

    /// <summary>
    /// Marks the message as failed.
    /// </summary>
    /// <param name="error">The processing error.</param>
    public void MarkAsFailed(string error)
    {
        RetryCount++;
        Error = error;
    }
}
