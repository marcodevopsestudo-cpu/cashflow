namespace TransactionService.Api.Common.Constants;

/// <summary>
/// Correlation header constants.
/// </summary>
public static class CorrelationConstants
{
    /// <summary>
    /// Key used to store or retrieve the correlation ID from the request context.
    /// </summary>
    public const string CorrelationIdItemKey = "CorrelationId";

    /// <summary>
    /// Header name used to propagate the correlation ID across services.
    /// </summary>
    public const string CorrelationHeaderName = "x-correlation-id";
}
