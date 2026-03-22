namespace ConsolidationService.Domain.Entities;

/// <summary>
/// Represents a transaction or batch item that requires manual investigation.
/// </summary>
public sealed class TransactionProcessingError
{
    public long Id { get; set; }

    public Guid BatchId { get; set; }

    public long? TransactionId { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public string ErrorCode { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public string? StackTrace { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int RetryCount { get; set; }

    public string Status { get; set; } = string.Empty;
}
