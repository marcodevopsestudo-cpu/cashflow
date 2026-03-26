using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Messages.Logs;
using ConsolidationService.Application.Models;
using ConsolidationService.Domain.Constants;
using ConsolidationService.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Steps;

/// <summary>
/// Loads all pending transactions associated with the current batch execution context.
/// </summary>
public sealed class LoadTransactionsStep
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<LoadTransactionsStep> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadTransactionsStep"/> class.
    /// </summary>
    /// <param name="transactionRepository">
    /// Repository used to retrieve pending transactions for the batch.
    /// </param>
    /// <param name="logger">
    /// Logger used to record structured information about transaction loading.
    /// </param>
    public LoadTransactionsStep(
        ITransactionRepository transactionRepository,
        ILogger<LoadTransactionsStep> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Loads pending transactions for the current batch and stores them in the execution context.
    /// </summary>
    /// <param name="context">
    /// The workflow execution context containing the batch message and target transaction identifiers.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous loading operation.
    /// </returns>
    /// <exception cref="InvalidBatchException">
    /// Thrown when no pending transactions are found for the specified batch.
    /// </exception>
    public async Task ExecuteAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        var message = context.Message;

        var transactions = await _transactionRepository.GetPublishedByIdsAsync(
            message.TransactionIds,
            cancellationToken);

        if (transactions.Count == 0)
        {
            throw new InvalidBatchException(
                string.Format(ErrorMessages.Batch.NoPendingTransactions, message.BatchId));
        }

        context.Transactions = transactions;

        _logger.LogInformation(
            BatchLogMessages.Workflow.LoadedPendingTransactions,
            message.BatchId,
            transactions.Count);
    }
}
