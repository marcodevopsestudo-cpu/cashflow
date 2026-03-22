namespace ConsolidationService.Domain.Exceptions;

/// <summary>
/// Represents an invalid batch payload or unsupported batch state.
/// </summary>
public sealed class InvalidBatchException : Exception
{
    public InvalidBatchException(string message)
        : base(message)
    {
    }
}
