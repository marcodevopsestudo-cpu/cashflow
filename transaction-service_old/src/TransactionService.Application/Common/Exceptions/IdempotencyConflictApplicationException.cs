using System.Net;
using TransactionService.Application.Common.Errors;

namespace TransactionService.Application.Common.Exceptions;

/// <summary>
/// Represents an application exception thrown when an idempotency key
/// is reused with a different request payload.
/// </summary>
/// <remarks>
/// This exception indicates a conflict scenario where the same idempotency key
/// was previously associated with a different request, violating idempotency rules.
/// </remarks>
public sealed class IdempotencyConflictApplicationException : ApplicationExceptionBase
{
    private const string DefaultMessage =
        "The provided Idempotency-Key was already used with a different payload.";

    /// <summary>
    /// Initializes a new instance of the <see cref="IdempotencyConflictApplicationException"/> class.
    /// </summary>
    public IdempotencyConflictApplicationException()
        : base(new ApplicationError(
            ErrorCodes.IdempotencyConflict,
            DefaultMessage,
            (int)HttpStatusCode.Conflict))
    {
    }
}
