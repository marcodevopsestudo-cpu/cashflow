using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Models;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Steps;

/// <summary>
/// Writes the aggregated values into the daily balance read model using upsert semantics.
/// </summary>
public sealed class UpsertDailyBalanceStep : IConsolidationWorkflowStep
{
    private readonly IDailyBalanceRepository _dailyBalanceRepository;
    private readonly ILogger<UpsertDailyBalanceStep> _logger;

    public UpsertDailyBalanceStep(IDailyBalanceRepository dailyBalanceRepository, ILogger<UpsertDailyBalanceStep> logger)
    {
        _dailyBalanceRepository = dailyBalanceRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        await _dailyBalanceRepository.UpsertAsync(context.Aggregates, cancellationToken);

        _logger.LogInformation(
            "Daily balance updated. BatchId: {BatchId}, AggregateCount: {AggregateCount}",
            context.Message.BatchId,
            context.Aggregates.Count);
    }
}
