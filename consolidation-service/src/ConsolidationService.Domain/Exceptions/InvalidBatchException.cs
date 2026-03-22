namespace ConsolidationService.Domain.Exceptions;

/// <summary>
/// Represents an error that occurs when a batch payload is invalid
/// or when the batch is in an unsupported state for processing.
/// </summary>
public sealed class InvalidBatchException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidBatchException"/> class.
    /// </summary>
    /// <param name="message">
    /// A human-readable message describing why the batch is considered invalid.
    /// </param>
    public InvalidBatchException(string message)
        : base(message)
    {
    }
}
