namespace ConsolidationService.Domain.Entities;

/// <summary>
/// Represents a transaction-level processing error that requires manual review or later inspection.
/// </summary>
public sealed class TransactionProcessingError
{
    /// <summary>
    /// Gets the identifier of the transaction that failed processing.
    /// </summary>
    public Guid TransactionId { get; }

    /// <summary>
    /// Gets the identifier of the batch associated with the transaction.
    /// </summary>
    public Guid BatchId { get; }

    /// <summary>
    /// Gets the error message describing the reason for the failure.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Gets the UTC timestamp indicating when the error occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionProcessingError"/> class.
    /// </summary>
    /// <param name="transactionId">The identifier of the transaction that failed.</param>
    /// <param name="batchId">The identifier of the batch.</param>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <param name="occurredOnUtc">The UTC timestamp when the error occurred.</param>
    public TransactionProcessingError(
        Guid transactionId,
        Guid batchId,
        string errorMessage,
        DateTime occurredOnUtc)
    {
        TransactionId = transactionId;
        BatchId = batchId;
        ErrorMessage = errorMessage;
        OccurredOnUtc = occurredOnUtc;
    }
}
