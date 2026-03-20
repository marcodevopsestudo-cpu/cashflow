using System.Net;
using TransactionService.Application.Common.Errors;

namespace TransactionService.Application.Common.Exceptions;

/// <summary>
/// Represents a not-found exception.
/// </summary>
public sealed class EntityNotFoundApplicationException : ApplicationExceptionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundApplicationException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public EntityNotFoundApplicationException(string message)
        : base(new ApplicationError(ErrorCodes.NotFound, message, (int)HttpStatusCode.NotFound))
    {
    }
}
