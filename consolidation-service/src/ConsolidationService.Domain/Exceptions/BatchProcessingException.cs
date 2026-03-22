namespace ConsolidationService.Domain.Exceptions;

/// <summary>
/// Represents an unrecoverable batch-processing failure.
/// </summary>
public sealed class BatchProcessingException : Exception
{
    public BatchProcessingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
