using System.Net;
using TransactionService.Application.Common.Errors;

namespace TransactionService.Application.Common.Exceptions;

/// <summary>
/// Represents an in-flight request for the same idempotency key.
/// </summary>
public sealed class RequestInProgressApplicationException : ApplicationExceptionBase
{
    public RequestInProgressApplicationException()
        : base(new ApplicationError(
            ErrorCodes.RequestInProgress,
            "A request with the same Idempotency-Key is already being processed.",
            (int)HttpStatusCode.Conflict))
    {
    }
}
