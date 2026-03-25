namespace TransactionService.Api.Resources;

/// <summary>
/// Centralizes API-layer messages.
/// </summary>
public static class ApiMessageCatalog
{
    /// <summary>
    /// Error code used for validation failures.
    /// </summary>
    public const string ValidationErrorCode = "validation_error";

    /// <summary>
    /// Message indicating that the request body is required.
    /// </summary>
    public const string RequestBodyRequired = "The request body is required.";

    /// <summary>
    /// Message indicating that the idempotency key could not be resolved.
    /// </summary>
    public const string IdempotencyKeyNotResolved = "Idempotency-Key was not resolved.";

    /// <summary>
    /// Message used for unexpected errors.
    /// </summary>
    public const string UnexpectedError = "An unexpected error occurred.";

    /// <summary>
    /// Default authorization reason when none is provided.
    /// </summary>
    public const string AuthorizationReasonUnknown = "Unknown";

    /// <summary>
    /// Default authorization value when none is provided.
    /// </summary>
    public const string AuthorizationValueUnknown = "unknown";

    /// <summary>
    /// Message indicating that the request is not authenticated by Microsoft Entra ID.
    /// </summary>
    public const string MissingPrincipal = "The request is not authenticated by Microsoft Entra ID.";

    /// <summary>
    /// Message indicating that the caller application ID was not found in the token.
    /// </summary>
    public const string MissingAppId = "The caller application identifier was not found in the token claims.";

    /// <summary>
    /// Message indicating that the caller application is not authorized.
    /// </summary>
    public const string AppIdNotAllowed = "The caller application is not authorized to access this API.";

    /// <summary>
    /// Message indicating that the token audience is invalid.
    /// </summary>
    public const string InvalidAudience = "The token audience is not allowed for this API.";

    /// <summary>
    /// Message indicating that the token issuer is invalid.
    /// </summary>
    public const string InvalidIssuer = "The token issuer is not allowed for this API.";

    /// <summary>
    /// Message indicating that the caller is not authorized.
    /// </summary>
    public const string UnauthorizedCaller = "The caller is not authorized to access this API.";

    /// <summary>
    /// Contains log message templates used across the API.
    /// </summary>
    public static class Logs
    {
        /// <summary>
        /// Log message indicating that outbox processing has completed.
        /// </summary>
        public const string OutboxProcessingCompleted =
            "Outbox processing completed. Schedule status: {ScheduleStatus}. Processed {ProcessedCount} messages.";

        /// <summary>
        /// Log message indicating a failure to parse the caller principal.
        /// </summary>
        public const string FailedToParseCallerPrincipal = "Failed to parse caller principal.";

        /// <summary>
        /// Log message indicating an unhandled exception for non-HTTP triggers.
        /// </summary>
        public const string UnhandledNonHttpTriggerException = "Unhandled exception for non-HTTP trigger.";

        /// <summary>
        /// Log message indicating an unhandled exception during request processing.
        /// </summary>
        public const string UnhandledRequestException = "Unhandled exception while processing request. CorrelationId={CorrelationId}";
    }
}
