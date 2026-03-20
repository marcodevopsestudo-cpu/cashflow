namespace TransactionService.Api.Contracts.Responses;

/// <summary>
/// Represents a normalized API error response.
/// </summary>
public sealed record ErrorResponse(string Code, string Message, string? CorrelationId);
