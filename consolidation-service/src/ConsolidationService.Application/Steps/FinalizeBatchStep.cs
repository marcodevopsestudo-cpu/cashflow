using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Messages.Logs;
using ConsolidationService.Application.Models;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Steps;

/// <summary>
/// Finalizes the batch processing by marking transactions as consolidated
/// and updating the batch status to succeeded.
/// </summary>
public sealed class FinalizeBatchStep
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IDailyBatchRepository _dailyBatchRepository;
    private readonly ILogger<FinalizeBatchStep> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FinalizeBatchStep"/> class.
    /// </summary>
    /// <param name="transactionRepository">
    /// Repository used to update transaction processing states.
    /// </param>
    /// <param name="dailyBatchRepository">
    /// Repository used to update the batch lifecycle state.
    /// </param>
    /// <param name="logger">
    /// Logger used to record structured information about batch finalization.
    /// </param>
    public FinalizeBatchStep(
        ITransactionRepository transactionRepository,
        IDailyBatchRepository dailyBatchRepository,
        ILogger<FinalizeBatchStep> logger)
    {
        _transactionRepository = transactionRepository;
        _dailyBatchRepository = dailyBatchRepository;
        _logger = logger;
    }

    /// <summary>
    /// Marks all transactions in the context as consolidated and completes the batch successfully.
    /// </summary>
    /// <param name="context">
    /// The workflow execution context containing the transactions and batch information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous finalization operation.
    /// </returns>
    public async Task ExecuteAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        var message = context.Message;

        var transactionIds = context.Transactions
            .Select(transaction => transaction.Id)
            .ToArray();

        var consolidatedAtUtc = DateTime.UtcNow;

        await _transactionRepository.MarkAsConsolidatedAsync(
            transactionIds,
            message.BatchId,
            consolidatedAtUtc,
            cancellationToken);

        await _dailyBatchRepository.MarkAsSucceededAsync(
            message.BatchId,
            cancellationToken);

        _logger.LogInformation(
            BatchLogMessages.Workflow.FinalizedSuccessfully,
            message.BatchId,
            transactionIds.Length);
    }
}
