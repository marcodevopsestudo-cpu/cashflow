using TransactionService.Application.Abstractions.Messaging;
using TransactionService.Domain.Enums;

namespace TransactionService.Application.Transactions.IntegrationEvents;

/// <summary>
/// Represents the integration event emitted when a transaction is created.
/// </summary>
public sealed record TransactionCreatedIntegrationEvent(
    Guid EventId,
    Guid TransactionId,
    string AccountId,
    TransactionKind Kind,
    decimal Amount,
    string Currency,
    DateTime TransactionDateUtc,
    string? Description,
    string CorrelationId,
    DateTime OccurredOnUtc,
    int EventVersion = 1) : IIntegrationEvent
{
    /// <summary>
    /// Gets the event name.
    /// </summary>
    public string EventName => "transaction.created";

    /// <summary>
    /// Gets the aggregate identifier.
    /// </summary>
    public string AggregateId => TransactionId.ToString();
}
