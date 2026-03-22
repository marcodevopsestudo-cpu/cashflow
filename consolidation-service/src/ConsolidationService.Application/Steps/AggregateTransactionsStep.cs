using ConsolidationService.Application.Messages.Logs;
using ConsolidationService.Application.Models;
using ConsolidationService.Domain.Enums;
using ConsolidationService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Steps;

/// <summary>
/// Aggregates transaction amounts by balance date, producing daily credit and debit totals.
/// </summary>
public sealed class AggregateTransactionsStep
{
    private readonly ILogger<AggregateTransactionsStep> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateTransactionsStep"/> class.
    /// </summary>
    /// <param name="logger">
    /// The logger used to record structured information about transaction aggregation.
    /// </param>
    public AggregateTransactionsStep(ILogger<AggregateTransactionsStep> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Aggregates the transactions available in the execution context by balance date.
    /// </summary>
    /// <param name="context">
    /// The workflow execution context containing the transactions to be aggregated.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the completion of the aggregation step.
    /// </returns>
    /// <remarks>
    /// For each balance date, this step calculates the total credited amount and the total
    /// debited amount, storing the resulting aggregates in the execution context.
    /// </remarks>
    public Task ExecuteAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        context.Aggregates = context.Transactions
            .GroupBy(transaction => transaction.BalanceDate)
            .Select(group => new DailyAggregate(
                group.Key,
                group.Where(transaction => transaction.Type == TransactionType.Credit).Sum(transaction => transaction.Amount),
                group.Where(transaction => transaction.Type == TransactionType.Debit).Sum(transaction => transaction.Amount)))
            .OrderBy(aggregate => aggregate.BalanceDate)
            .ToArray();

        _logger.LogInformation(
            BatchLogMessages.Workflow.AggregatedTransactions,
            context.Message.BatchId,
            context.Aggregates.Count);

        return Task.CompletedTask;
    }
}
