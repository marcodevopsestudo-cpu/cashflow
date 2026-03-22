using System.Net;
using TransactionService.Application.Common.Errors;

namespace TransactionService.Application.Common.Exceptions;

/// <summary>
/// Represents a validation exception.
/// </summary>
public sealed class ValidationApplicationException : ApplicationExceptionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationApplicationException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public ValidationApplicationException(string message)
        : base(new ApplicationError(ErrorCodes.Validation, message, (int)HttpStatusCode.BadRequest))
    {
    }
}
