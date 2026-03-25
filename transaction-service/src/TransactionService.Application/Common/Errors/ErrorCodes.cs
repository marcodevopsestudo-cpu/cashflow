namespace TransactionService.Application.Common.Errors;

/// <summary>
/// Centralizes normalized error codes.
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// Error code used for validation failures.
    /// </summary>
    public const string Validation = "validation_error";

    /// <summary>
    /// Error code used when a requested resource is not found.
    /// </summary>
    public const string NotFound = "not_found";

    /// <summary>
    /// Error code used for integration-related failures (e.g., external services).
    /// </summary>
    public const string Integration = "integration_error";

    /// <summary>
    /// Error code used for unexpected or unhandled errors.
    /// </summary>
    public const string Unexpected = "unexpected_error";

    /// <summary>
    /// Error code used when an idempotency conflict occurs.
    /// </summary>
    public const string IdempotencyConflict = "idempotency_conflict";

    /// <summary>
    /// Error code used when a request with the same idempotency key is already in progress.
    /// </summary>
    public const string RequestInProgress = "request_in_progress";

    /// <summary>
    /// Error code used when the caller is not authenticated.
    /// </summary>
    public const string Unauthorized = "unauthorized";

    /// <summary>
    /// Error code used when the caller is authenticated but not authorized to perform the operation.
    /// </summary>
    public const string Forbidden = "forbidden";
}
