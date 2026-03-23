using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides an EF Core-based implementation of <see cref="IOutboxRepository"/>.
/// Responsible for managing persistence of outbox messages.
/// </summary>
public sealed class OutboxRepository : IOutboxRepository
{
    private readonly TransactionDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context used for persistence operations.</param>
    public OutboxRepository(TransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds a new outbox message to the persistence context.
    /// </summary>
    /// <param name="message">The outbox message to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken)
        => _dbContext.OutboxMessages.AddAsync(message, cancellationToken).AsTask();

    /// <summary>
    /// Retrieves a batch of pending outbox messages ordered by creation time.
    /// </summary>
    /// <param name="batchSize">The maximum number of messages to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of pending outbox messages.</returns>
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        return await _dbContext.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Marks a single outbox message as modified in the persistence context.
    /// </summary>
    /// <param name="message">The message to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        _dbContext.OutboxMessages.Update(message);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks a collection of outbox messages as modified in the persistence context.
    /// </summary>
    /// <param name="messages">The collection of messages to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task UpdateRangeAsync(
        IEnumerable<OutboxMessage> messages,
        CancellationToken cancellationToken)
    {
        _dbContext.OutboxMessages.UpdateRange(messages);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
