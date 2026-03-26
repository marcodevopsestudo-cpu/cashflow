using ConsolidationService.Application.Abstractions;
using ConsolidationService.Domain.Entities;
using TransactionService.Infrastructure.Persistence;

namespace ConsolidationService.Infrastructure.Persistence;

/// <summary>
/// Provides PostgreSQL persistence operations for transaction processing errors
/// that require manual review.
/// </summary>
public sealed class TransactionProcessingErrorRepository : ITransactionProcessingErrorRepository
{
    private readonly TransactionDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionProcessingErrorRepository"/> class.
    /// </summary>
    /// <param name="dbContext">
    /// DbContext used for repository operations.
    /// </param>
    public TransactionProcessingErrorRepository(TransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Inserts transaction or batch processing errors into the persistence store.
    /// </summary>
    /// <param name="items">
    /// The collection of processing errors to be persisted.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous insert operation.
    /// </returns>
    public async Task InsertAsync(IReadOnlyCollection<TransactionProcessingError> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return;
        }

        await _dbContext.Set<TransactionProcessingError>()
            .AddRangeAsync(items, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
