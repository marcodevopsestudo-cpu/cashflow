using ConsolidationService.Domain.Entities;

namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Persists manual-review records when a batch cannot be automatically processed.
/// </summary>
public interface ITransactionProcessingErrorRepository
{
    Task InsertAsync(IReadOnlyCollection<TransactionProcessingError> items, CancellationToken cancellationToken);
}
