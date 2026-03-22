using TransactionService.Domain.Entities;

namespace TransactionService.Application.Transactions.Common;

/// <summary>
/// Provides transaction mapping helpers.
/// </summary>
public static class TransactionMappings
{
    /// <summary>
    /// Maps a domain transaction to a DTO.
    /// </summary>
    /// <param name="transaction">The transaction entity.</param>
    /// <returns>The DTO.</returns>
    public static TransactionDto ToDto(this Transaction transaction)
    {
        return new TransactionDto(
            transaction.Id,
            transaction.AccountId,
            transaction.Kind,
            transaction.Amount,
            transaction.Currency,
            transaction.TransactionDateUtc,
            transaction.Description,
            transaction.Status.ToString(),
            transaction.CorrelationId,
            transaction.CreatedAtUtc,
            transaction.UpdatedAtUtc);
    }
}
