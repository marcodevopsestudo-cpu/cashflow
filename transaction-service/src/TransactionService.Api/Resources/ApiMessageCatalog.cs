namespace TransactionService.Api.Resources;

/// <summary>
/// Centralizes API-layer messages.
/// </summary>
public static class ApiMessageCatalog
{
    public const string ValidationErrorCode = "validation_error";
    public const string RequestBodyRequired = "The request body is required.";
    public const string IdempotencyKeyNotResolved = "Idempotency-Key was not resolved.";
    public const string UnexpectedError = "An unexpected error occurred.";

    public const string AuthorizationReasonUnknown = "Unknown";
    public const string AuthorizationValueUnknown = "unknown";
    public const string MissingPrincipal = "The request is not authenticated by Microsoft Entra ID.";
    public const string MissingAppId = "The caller application identifier was not found in the token claims.";
    public const string AppIdNotAllowed = "The caller application is not authorized to access this API.";
    public const string InvalidAudience = "The token audience is not allowed for this API.";
    public const string InvalidIssuer = "The token issuer is not allowed for this API.";
    public const string UnauthorizedCaller = "The caller is not authorized to access this API.";

    public const string OutboxProcessingCompleted =
        "Outbox processing completed. Schedule status: {ScheduleStatus}. Processed {ProcessedCount} messages.";
}
