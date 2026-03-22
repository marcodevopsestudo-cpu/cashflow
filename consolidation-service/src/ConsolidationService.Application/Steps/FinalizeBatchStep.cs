using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Models;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Steps;

/// <summary>
/// Marks transactions as consolidated and closes the batch successfully.
/// </summary>
public sealed class FinalizeBatchStep : IConsolidationWorkflowStep
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IDailyBatchRepository _dailyBatchRepository;
    private readonly ILogger<FinalizeBatchStep> _logger;

    public FinalizeBatchStep(
        ITransactionRepository transactionRepository,
        IDailyBatchRepository dailyBatchRepository,
        ILogger<FinalizeBatchStep> logger)
    {
        _transactionRepository = transactionRepository;
        _dailyBatchRepository = dailyBatchRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        var transactionIds = context.Transactions.Select(x => x.Id).ToArray();
        await _transactionRepository.MarkAsConsolidatedAsync(transactionIds, context.Message.BatchId, DateTime.UtcNow, cancellationToken);
        await _dailyBatchRepository.MarkAsSucceededAsync(context.Message.BatchId, cancellationToken);

        _logger.LogInformation(
            "Batch finalized successfully. BatchId: {BatchId}, ConsolidatedTransactions: {ConsolidatedTransactions}",
            context.Message.BatchId,
            transactionIds.Length);
    }
}
