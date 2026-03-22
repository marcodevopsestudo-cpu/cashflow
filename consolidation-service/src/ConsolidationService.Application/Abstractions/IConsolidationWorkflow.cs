using ConsolidationService.Application.Contracts;

namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Orchestrates the manual consolidation pipeline.
/// </summary>
public interface IConsolidationWorkflow
{
    Task ExecuteAsync(ConsolidationBatchMessage message, string messageId, CancellationToken cancellationToken);
}
