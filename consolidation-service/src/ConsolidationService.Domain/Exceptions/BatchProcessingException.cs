namespace ConsolidationService.Domain.Exceptions;

/// <summary>
/// Represents an unrecoverable error that occurs during batch processing.
/// </summary>
/// <remarks>
/// This exception is typically used to wrap unexpected failures that prevent
/// the batch from completing successfully, preserving the original exception
/// as the inner cause.
/// </remarks>
public sealed class BatchProcessingException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchProcessingException"/> class.
    /// </summary>
    /// <param name="message">
    /// A human-readable message describing the failure.
    /// </param>
    /// <param name="innerException">
    /// The underlying exception that caused the processing failure.
    /// </param>
    public BatchProcessingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
