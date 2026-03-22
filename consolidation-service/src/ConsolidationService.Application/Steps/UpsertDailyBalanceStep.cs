using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Messages.Logs;
using ConsolidationService.Application.Models;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Steps;

/// <summary>
/// Persists aggregated daily balances using upsert semantics in the read model.
/// </summary>
public sealed class UpsertDailyBalanceStep
{
    private readonly IDailyBalanceRepository _dailyBalanceRepository;
    private readonly ILogger<UpsertDailyBalanceStep> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpsertDailyBalanceStep"/> class.
    /// </summary>
    /// <param name="dailyBalanceRepository">
    /// Repository used to persist aggregated daily balances.
    /// </param>
    /// <param name="logger">
    /// Logger used to record structured information about balance updates.
    /// </param>
    public UpsertDailyBalanceStep(
        IDailyBalanceRepository dailyBalanceRepository,
        ILogger<UpsertDailyBalanceStep> logger)
    {
        _dailyBalanceRepository = dailyBalanceRepository;
        _logger = logger;
    }

    /// <summary>
    /// Writes aggregated values from the execution context into the daily balance read model.
    /// </summary>
    /// <param name="context">
    /// The workflow execution context containing the computed aggregates.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous upsert operation.
    /// </returns>
    public async Task ExecuteAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        var message = context.Message;

        await _dailyBalanceRepository.UpsertAsync(
            context.Aggregates,
            cancellationToken);

        _logger.LogInformation(
            BatchLogMessages.Workflow.DailyBalanceUpdated,
            message.BatchId,
            context.Aggregates.Count);
    }
}
