using TransactionService.Application.Common.Errors;

namespace TransactionService.Application.Common.Exceptions;

/// <summary>
/// Represents the base exception for normalized application failures.
/// </summary>
public abstract class ApplicationExceptionBase : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationExceptionBase"/> class.
    /// </summary>
    /// <param name="error">The normalized error.</param>
    protected ApplicationExceptionBase(ApplicationError error) : base(error.Message)
    {
        Error = error;
    }

    /// <summary>
    /// Gets the normalized error data.
    /// </summary>
    public ApplicationError Error { get; }
}
