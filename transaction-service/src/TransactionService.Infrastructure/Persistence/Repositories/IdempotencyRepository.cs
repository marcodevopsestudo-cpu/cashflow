using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Resources;

namespace TransactionService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides idempotency entry persistence using Entity Framework Core.
/// </summary>
/// <remarks>
/// This repository is responsible for retrieving, adding, and updating
/// <see cref="IdempotencyEntry"/> instances within the persistence store.
/// </remarks>
public sealed class IdempotencyRepository : IIdempotencyRepository
{
    private readonly TransactionDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdempotencyRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context used for persistence operations.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="dbContext"/> is null.
    /// </exception>
    public IdempotencyRepository(TransactionDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Retrieves an idempotency entry by its key.
    /// </summary>
    /// <param name="idempotencyKey">The unique idempotency key associated with the request.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// The matching <see cref="IdempotencyEntry"/> if found; otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="idempotencyKey"/> is null or whitespace.
    /// </exception>
    public Task<IdempotencyEntry?> GetByKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException(InfrastructureMessageCatalog.IdempotencyKeyCannotBeNullOrWhitespace, nameof(idempotencyKey));
        }

        return _dbContext.IdempotencyEntries
            .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    /// <summary>
    /// Adds a new idempotency entry to the database context.
    /// </summary>
    /// <param name="entry">The idempotency entry to be added.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="entry"/> is null.
    /// </exception>
    public Task AddAsync(IdempotencyEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return _dbContext.IdempotencyEntries
            .AddAsync(entry, cancellationToken)
            .AsTask();
    }

    /// <summary>
    /// Marks an existing idempotency entry as modified in the database context.
    /// </summary>
    /// <param name="entry">The idempotency entry to be updated.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A completed <see cref="Task"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="entry"/> is null.
    /// </exception>
    public Task UpdateAsync(IdempotencyEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _dbContext.IdempotencyEntries.Update(entry);
        return Task.CompletedTask;
    }
}
