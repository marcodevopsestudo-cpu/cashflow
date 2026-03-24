using TransactionService.Domain.Enums;

namespace TransactionService.Domain.Entities;

/// <summary>
/// Represents a transaction aggregate.
/// </summary>
public sealed class Transaction
{
   

    /// <summary>
    /// Gets the transaction identifier.
    /// </summary>
    public Guid TransactionId { get; private set; }

    /// <summary>
    /// Gets the account identifier.
    /// </summary>
    public string AccountId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the transaction kind.
    /// </summary>
    public TransactionKind Kind { get; private set; }

    /// <summary>
    /// Gets the transaction amount.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Gets the currency.
    /// </summary>
    public string Currency { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the transaction date in UTC.
    /// </summary>
    public DateTime TransactionDateUtc { get; private set; }

    /// <summary>
    /// Gets the optional description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the correlation id.
    /// </summary>
    public string CorrelationId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the transaction status.
    /// </summary>
    public TransactionStatus Status { get; private set; }

    /// <summary>
    /// Gets the creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the last update timestamp in UTC.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Creates a new transaction instance.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="kind">The transaction kind.</param>
    /// <param name="amount">The amount.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="transactionDateUtc">The transaction date.</param>
    /// <param name="description">The description.</param>
    /// <param name="correlationId">The correlation id.</param>
    /// <returns>A new transaction instance.</returns>
    public static Transaction Create(
        string accountId,
        TransactionKind kind,
        decimal amount,
        string currency,
        DateTime transactionDateUtc,
        string? description,
        string correlationId)
    {
        return new Transaction
        {
            TransactionId = Guid.NewGuid(),
            AccountId = accountId,
            Kind = kind,
            Amount = amount,
            Currency = currency,
            TransactionDateUtc = DateTime.SpecifyKind(transactionDateUtc, DateTimeKind.Utc),
            Description = description,
            CorrelationId = correlationId,
            Status = TransactionStatus.Received,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Marks the transaction as published.
    /// </summary>
    public void MarkAsPublished()
    {
        Status = TransactionStatus.Published;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the transaction as failed during publishing.
    /// </summary>
    public void MarkAsFailedToPublish()
    {
        Status = TransactionStatus.FailedToPublish;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
