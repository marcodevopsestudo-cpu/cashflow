namespace TransactionService.Application.Common.Errors;

/// <summary>
/// Represents a normalized application error.
/// </summary>
public sealed record ApplicationError(string Code, string Message, int HttpStatusCode);
