using ConsolidationService.Application.Models;

namespace ConsolidationService.Application.Abstractions;

/// <summary>
/// Represents a single step in the consolidation workflow.
/// </summary>
public interface IConsolidationWorkflowStep
{
    Task ExecuteAsync(BatchExecutionContext context, CancellationToken cancellationToken);
}
