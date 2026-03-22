namespace TransactionService.Domain.Entities;

/// <summary>
/// Represents an idempotent request registration for transaction creation.
/// </summary>
/// <remarks>
/// This entity is used to ensure that repeated requests with the same
/// idempotency key are handled consistently, preventing duplicate processing.
/// </remarks>
public sealed class IdempotencyEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdempotencyEntry"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is intended for ORM usage only.
    /// </remarks>
    private IdempotencyEntry()
    {
    }

    /// <summary>
    /// Gets the idempotency key associated with the request.
    /// </summary>
    public string IdempotencyKey { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the hash of the original request payload.
    /// </summary>
    public string RequestHash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the associated transaction identifier when processing is completed.
    /// </summary>
    public Guid? TransactionId { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the idempotency entry was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the request processing has been completed.
    /// </summary>
    public bool IsCompleted => TransactionId.HasValue;

    /// <summary>
    /// Creates a new <see cref="IdempotencyEntry"/> instance.
    /// </summary>
    /// <param name="idempotencyKey">The unique idempotency key.</param>
    /// <param name="requestHash">The hash of the request payload.</param>
    /// <returns>A new initialized <see cref="IdempotencyEntry"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="idempotencyKey"/> or <paramref name="requestHash"/> is null or whitespace.
    /// </exception>
    public static IdempotencyEntry Create(string idempotencyKey, string requestHash)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("IdempotencyKey cannot be null or empty.", nameof(idempotencyKey));
        }

        if (string.IsNullOrWhiteSpace(requestHash))
        {
            throw new ArgumentException("RequestHash cannot be null or empty.", nameof(requestHash));
        }

        return new IdempotencyEntry
        {
            IdempotencyKey = idempotencyKey,
            RequestHash = requestHash,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Marks the idempotency entry as completed by associating a transaction identifier.
    /// </summary>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="transactionId"/> is empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the entry has already been completed.
    /// </exception>
    public void Complete(Guid transactionId)
    {
        if (transactionId == Guid.Empty)
        {
            throw new ArgumentException("TransactionId cannot be empty.", nameof(transactionId));
        }

        if (IsCompleted)
        {
            throw new InvalidOperationException("The idempotency entry has already been completed.");
        }

        TransactionId = transactionId;
    }
}
