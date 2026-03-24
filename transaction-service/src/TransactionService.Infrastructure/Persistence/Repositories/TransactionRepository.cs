using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements transaction persistence using EF Core.
/// </summary>
public sealed class TransactionRepository : ITransactionRepository
{
    private readonly TransactionDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The db context.</param>
    public TransactionRepository(TransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds a transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task AddAsync(Transaction transaction, CancellationToken cancellationToken)
        => _dbContext.Transactions.AddAsync(transaction, cancellationToken).AsTask();

    /// <summary>
    /// Updates a transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        _dbContext.Transactions.Update(transaction);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a transaction by identifier.
    /// </summary>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transaction or null.</returns>
    public Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken)
        => _dbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(x => x.TransactionId == transactionId, cancellationToken);

    /// <summary>
    /// Gets a transaction for update tracking.
    /// </summary>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tracked transaction or null.</returns>
    public Task<Transaction?> GetForUpdateAsync(Guid transactionId, CancellationToken cancellationToken)
        => _dbContext.Transactions.FirstOrDefaultAsync(x => x.TransactionId == transactionId, cancellationToken);
}
