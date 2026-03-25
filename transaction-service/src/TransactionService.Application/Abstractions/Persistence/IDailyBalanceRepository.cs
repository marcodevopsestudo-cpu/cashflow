using TransactionService.Domain.Entities;

/// <summary>
/// Defines the contract for accessing daily balance data.
/// </summary>
public interface IDailyBalanceRepository
{
    /// <summary>
    /// Retrieves the daily balance for a specific date.
    /// </summary>
    /// <param name="date">The date for which the balance should be retrieved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The <see cref="DailyBalance"/> for the specified date, or <c>null</c> if no record is found.
    /// </returns>
    Task<DailyBalance?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken);
}
