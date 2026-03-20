namespace TransactionService.Domain.Entities;

/// <summary>
/// Represents an idempotent request registration for transaction creation.
/// </summary>
public sealed class IdempotencyEntry
{
    private IdempotencyEntry()
    {
    }

    public string IdempotencyKey { get; private set; } = string.Empty;

    public string RequestHash { get; private set; } = string.Empty;

    public Guid? TransactionId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public bool IsCompleted => TransactionId.HasValue;

    public static IdempotencyEntry Create(string idempotencyKey, string requestHash)
    {
        return new IdempotencyEntry
        {
            IdempotencyKey = idempotencyKey,
            RequestHash = requestHash,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Complete(Guid transactionId)
    {
        if (transactionId == Guid.Empty)
        {
            throw new ArgumentException("TransactionId cannot be empty.", nameof(transactionId));
        }

        TransactionId = transactionId;
    }
}
