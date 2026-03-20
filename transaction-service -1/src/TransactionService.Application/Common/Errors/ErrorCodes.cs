namespace TransactionService.Application.Common.Errors;

/// <summary>
/// Centralizes normalized error codes.
/// </summary>
public static class ErrorCodes
{
    public const string Validation = "validation_error";
    public const string NotFound = "not_found";
    public const string Integration = "integration_error";
    public const string Unexpected = "unexpected_error";
    public const string IdempotencyConflict = "idempotency_conflict";
    public const string RequestInProgress = "request_in_progress";
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";
}
