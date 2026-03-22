using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Models;
using ConsolidationService.Domain.Enums;
using ConsolidationService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Steps;

/// <summary>
/// Aggregates credit and debit totals by balance date.
/// </summary>
public sealed class AggregateTransactionsStep : IConsolidationWorkflowStep
{
    private readonly ILogger<AggregateTransactionsStep> _logger;

    public AggregateTransactionsStep(ILogger<AggregateTransactionsStep> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        context.Aggregates = context.Transactions
            .GroupBy(x => x.BalanceDate)
            .Select(group => new DailyAggregate(
                group.Key,
                group.Where(x => x.Type == TransactionType.Credit).Sum(x => x.Amount),
                group.Where(x => x.Type == TransactionType.Debit).Sum(x => x.Amount)))
            .OrderBy(x => x.BalanceDate)
            .ToArray();

        _logger.LogInformation(
            "Aggregated batch transactions by date. BatchId: {BatchId}, AggregateCount: {AggregateCount}",
            context.Message.BatchId,
            context.Aggregates.Count);

        return Task.CompletedTask;
    }
}
