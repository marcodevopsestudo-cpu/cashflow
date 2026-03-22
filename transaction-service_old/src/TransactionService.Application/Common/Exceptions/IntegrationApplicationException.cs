using System.Net;
using TransactionService.Application.Common.Errors;

namespace TransactionService.Application.Common.Exceptions;

/// <summary>
/// Represents an integration failure exception.
/// </summary>
public sealed class IntegrationApplicationException : ApplicationExceptionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationApplicationException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public IntegrationApplicationException(string message)
        : base(new ApplicationError(ErrorCodes.Integration, message, (int)HttpStatusCode.UnprocessableEntity))
    {
    }
}
