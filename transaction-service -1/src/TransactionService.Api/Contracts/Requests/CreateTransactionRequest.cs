namespace TransactionService.Api.Contracts.Requests;

/// <summary>
/// Represents the create transaction HTTP request.
/// </summary>
public sealed class CreateTransactionRequest
{
    /// <summary>
    /// Gets or sets the account identifier.
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction kind.
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction date in UTC.
    /// </summary>
    public DateTime TransactionDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the correlation id.
    /// </summary>
    public string? CorrelationId { get; set; }
}
