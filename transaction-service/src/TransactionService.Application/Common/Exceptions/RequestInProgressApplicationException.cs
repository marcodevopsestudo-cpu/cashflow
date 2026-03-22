using System.Net;
using TransactionService.Application.Common.Errors;
using TransactionService.Application.Resources;

namespace TransactionService.Application.Common.Exceptions;

/// <summary>
/// Represents an application exception thrown when a request with the same
/// idempotency key is already being processed.
/// </summary>
/// <remarks>
/// This exception indicates a concurrency scenario where a duplicate request
/// is received while the original request is still in progress.
/// </remarks>
public sealed class RequestInProgressApplicationException : ApplicationExceptionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequestInProgressApplicationException"/> class.
    /// </summary>
    public RequestInProgressApplicationException()
        : base(new ApplicationError(
            ErrorCodes.RequestInProgress,
            MessageCatalog.RequestWithSameIdempotencyKeyAlreadyInProgress,
            (int)HttpStatusCode.Conflict))
    {
    }
}
