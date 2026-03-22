using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Models;
using ConsolidationService.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Steps;

/// <summary>
/// Loads all pending transactions that belong to the current batch.
/// </summary>
public sealed class LoadTransactionsStep : IConsolidationWorkflowStep
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<LoadTransactionsStep> _logger;

    public LoadTransactionsStep(ITransactionRepository transactionRepository, ILogger<LoadTransactionsStep> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetPendingByIdsAsync(context.Message.TransactionIds, cancellationToken);
        if (transactions.Count == 0)
        {
            throw new InvalidBatchException($"Batch '{context.Message.BatchId}' does not contain pending transactions.");
        }

        context.Transactions = transactions;

        _logger.LogInformation(
            "Loaded pending transactions for batch. BatchId: {BatchId}, LoadedTransactions: {LoadedTransactions}",
            context.Message.BatchId,
            transactions.Count);
    }
}
