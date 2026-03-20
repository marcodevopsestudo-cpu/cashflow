using System.Net;
using TransactionService.Application.Common.Errors;

namespace TransactionService.Application.Common.Exceptions;

/// <summary>
/// Represents an idempotency key conflict caused by a payload mismatch.
/// </summary>
public sealed class IdempotencyConflictApplicationException : ApplicationExceptionBase
{
    public IdempotencyConflictApplicationException()
        : base(new ApplicationError(
            ErrorCodes.IdempotencyConflict,
            "The provided Idempotency-Key was already used with a different payload.",
            (int)HttpStatusCode.Conflict))
    {
    }
}
