using ConsolidationService.Domain.Constants;

namespace ConsolidationService.Domain.Exceptions;

/// <summary>
/// Represents an error that occurs when attempting to process a batch
/// that has already been completed or intentionally ignored.
/// </summary>
public sealed class BatchAlreadyProcessedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchAlreadyProcessedException"/> class.
    /// </summary>
    /// <param name="batchId">
    /// The identifier of the batch that has already been processed.
    /// </param>
    public BatchAlreadyProcessedException(Guid batchId)
        : base(string.Format(ErrorMessages.AlreadyProcessed, batchId))
    {
        BatchId = batchId;
    }

    /// <summary>
    /// Gets the identifier of the batch associated with the exception.
    /// </summary>
    public Guid BatchId { get; }
}
