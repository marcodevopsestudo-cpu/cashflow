using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository responsible for retrieving daily balance data from the database.
/// </summary>
public sealed class DailyBalanceRepository : IDailyBalanceRepository
{
    private readonly TransactionDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DailyBalanceRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context used to access persistence.</param>
    public DailyBalanceRepository(TransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retrieves the daily balance for a specific date.
    /// </summary>
    /// <param name="date">The date for which the balance should be retrieved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The <see cref="DailyBalance"/> for the specified date, or <c>null</c> if no record is found.
    /// </returns>
    public Task<DailyBalance?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken)
    {
        return _dbContext.Set<DailyBalance>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BalanceDate == date, cancellationToken);
    }
}
