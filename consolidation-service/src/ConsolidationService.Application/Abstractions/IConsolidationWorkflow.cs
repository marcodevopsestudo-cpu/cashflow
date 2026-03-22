using ConsolidationService.Application.Contracts;

namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Orchestrates the manual consolidation pipeline.
/// </summary>
public interface IConsolidationWorkflow
{
    /// <summary>
    /// execute consolidation workflow
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteAsync(ConsolidationBatchMessage message, CancellationToken cancellationToken);
}
