using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements idempotency persistence using EF Core.
/// </summary>
public sealed class IdempotencyRepository : IIdempotencyRepository
{
    private readonly TransactionDbContext _dbContext;

    public IdempotencyRepository(TransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IdempotencyEntry?> GetByKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
        => _dbContext.IdempotencyEntries.SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);

    public Task AddAsync(IdempotencyEntry entry, CancellationToken cancellationToken)
        => _dbContext.IdempotencyEntries.AddAsync(entry, cancellationToken).AsTask();

    public Task UpdateAsync(IdempotencyEntry entry, CancellationToken cancellationToken)
    {
        _dbContext.IdempotencyEntries.Update(entry);
        return Task.CompletedTask;
    }
}
