using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;

namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Provides persistence operations for batch processing control.
/// </summary>
public interface IDailyBatchRepository
{
    Task<DailyBatch?> GetAsync(Guid batchId, CancellationToken cancellationToken);

    Task<DailyBatch> UpsertPendingAsync(Guid batchId, string correlationId, int transactionCount, CancellationToken cancellationToken);

    Task MarkAsProcessingAsync(Guid batchId, CancellationToken cancellationToken);

    Task MarkAsSucceededAsync(Guid batchId, CancellationToken cancellationToken);

    Task MarkAsFailedAsync(Guid batchId, string errorMessage, int retryCount, CancellationToken cancellationToken);
}
