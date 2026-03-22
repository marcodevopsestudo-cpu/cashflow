using ConsolidationService.Application.Contracts;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.ValueObjects;

namespace ConsolidationService.Application.Models;

/// <summary>
/// Represents the execution context shared across all steps of the consolidation workflow.
/// </summary>
/// <remarks>
/// This context acts as a state container, allowing each step in the pipeline to read and enrich
/// data as the batch progresses through the processing stages.
/// </remarks>
public sealed class BatchExecutionContext
{
    /// <summary>
    /// Gets the original batch message that initiated the workflow execution.
    /// </summary>
    /// <remarks>
    /// This property is required and must be provided at context initialization.
    /// </remarks>
    public required ConsolidationBatchMessage Message { get; init; }

    /// <summary>
    /// Gets or sets the current batch entity associated with the execution.
    /// </summary>
    /// <remarks>
    /// This value may be null until the batch is created or retrieved from persistence.
    /// </remarks>
    public DailyBatch? Batch { get; set; }

    /// <summary>
    /// Gets or sets the collection of transactions involved in the batch.
    /// </summary>
    /// <remarks>
    /// This collection is typically populated after retrieving pending transactions from the repository.
    /// </remarks>
    public IReadOnlyCollection<Transaction> Transactions { get; set; } = Array.Empty<Transaction>();

    /// <summary>
    /// Gets or sets the computed daily aggregates derived from the transactions.
    /// </summary>
    /// <remarks>
    /// This collection is populated during the aggregation step of the workflow.
    /// </remarks>
    public IReadOnlyCollection<DailyAggregate> Aggregates { get; set; } = Array.Empty<DailyAggregate>();
}
