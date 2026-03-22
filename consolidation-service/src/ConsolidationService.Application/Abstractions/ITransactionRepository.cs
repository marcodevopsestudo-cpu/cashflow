using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;

namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Provides data access for transaction rows created by Transaction Service.
/// </summary>
public interface ITransactionRepository
{
    Task<IReadOnlyCollection<Transaction>> GetPendingByIdsAsync(IReadOnlyCollection<long> transactionIds, CancellationToken cancellationToken);

    Task MarkAsConsolidatedAsync(IReadOnlyCollection<long> transactionIds, Guid batchId, DateTime consolidatedAtUtc, CancellationToken cancellationToken);

    Task MarkAsFailedAsync(IReadOnlyCollection<long> transactionIds, Guid batchId, int attemptCount, TransactionProcessingStatus status, CancellationToken cancellationToken);
}
