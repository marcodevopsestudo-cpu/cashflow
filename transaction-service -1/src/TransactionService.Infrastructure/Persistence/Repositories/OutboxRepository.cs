using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements outbox persistence using EF Core.
/// </summary>
public sealed class OutboxRepository : IOutboxRepository
{
    private readonly TransactionDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The db context.</param>
    public OutboxRepository(TransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds a new outbox message.
    /// </summary>
    /// <param name="message">The outbox message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken)
        => _dbContext.OutboxMessages.AddAsync(message, cancellationToken).AsTask();

    /// <summary>
    /// Gets pending outbox messages ordered by creation time.
    /// </summary>
    /// <param name="batchSize">The maximum number of messages to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The pending outbox messages.</returns>
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        return await _dbContext.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Updates an outbox message.
    /// </summary>
    /// <param name="message">The message to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        _dbContext.OutboxMessages.Update(message);
        return Task.CompletedTask;
    }
}
