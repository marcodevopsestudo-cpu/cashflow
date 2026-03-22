using ConsolidationService.Application.Models;

namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Represents a single step in the consolidation workflow.
/// </summary>
public interface IConsolidationWorkflowStep
{
    /// <summary>
    /// execute a consolidate workflow step
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteAsync(BatchExecutionContext context, CancellationToken cancellationToken);
}
