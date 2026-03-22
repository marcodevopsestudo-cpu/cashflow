namespace ConsolidationService.Domain.Exceptions;

/// <summary>
/// Represents an attempt to process a batch that has already been completed or intentionally ignored.
/// </summary>
public sealed class BatchAlreadyProcessedException : Exception
{
    public BatchAlreadyProcessedException(Guid batchId)
        : base($"Batch '{batchId}' has already been processed.")
    {
        BatchId = batchId;
    }

    public Guid BatchId { get; }
}
