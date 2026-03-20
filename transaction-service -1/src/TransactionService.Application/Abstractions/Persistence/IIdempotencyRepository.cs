using TransactionService.Domain.Entities;

namespace TransactionService.Application.Abstractions.Persistence;

/// <summary>
/// Defines persistence operations for idempotency entries.
/// </summary>
public interface IIdempotencyRepository
{
    Task<IdempotencyEntry?> GetByKeyAsync(string idempotencyKey, CancellationToken cancellationToken);

    Task AddAsync(IdempotencyEntry entry, CancellationToken cancellationToken);

    Task UpdateAsync(IdempotencyEntry entry, CancellationToken cancellationToken);
}
