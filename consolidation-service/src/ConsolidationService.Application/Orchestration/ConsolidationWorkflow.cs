using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Contracts;
using ConsolidationService.Application.Messages.Logs;
using ConsolidationService.Application.Models;
using ConsolidationService.Application.Steps;
using ConsolidationService.Domain.Constants;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Orchestration;

/// <summary>
/// Orchestrates the execution of the consolidation pipeline in a deterministic and ordered sequence.
/// </summary>
public sealed class ConsolidationWorkflow : IConsolidationWorkflow
{
    private readonly RegisterBatchStep _registerBatchStep;
    private readonly LoadTransactionsStep _loadTransactionsStep;
    private readonly ValidateTransactionsStep _validateTransactionsStep;
    private readonly AggregateTransactionsStep _aggregateTransactionsStep;
    private readonly UpsertDailyBalanceStep _upsertDailyBalanceStep;
    private readonly FinalizeBatchStep _finalizeBatchStep;
    private readonly IDailyBatchRepository _dailyBatchRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionProcessingErrorRepository _errorRepository;
    private readonly IRetryPolicy _retryPolicy;
    private readonly ILogger<ConsolidationWorkflow> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsolidationWorkflow"/> class.
    /// </summary>
    /// <param name="registerBatchStep">Step responsible for registering the batch.</param>
    /// <param name="loadTransactionsStep">Step responsible for loading transactions.</param>
    /// <param name="validateTransactionsStep">Step responsible for validating transactions.</param>
    /// <param name="aggregateTransactionsStep">Step responsible for aggregating transactions.</param>
    /// <param name="upsertDailyBalanceStep">Step responsible for updating daily balances.</param>
    /// <param name="finalizeBatchStep">Step responsible for finalizing the batch.</param>
    /// <param name="dailyBatchRepository">Repository for batch persistence operations.</param>
    /// <param name="transactionRepository">Repository for transaction persistence operations.</param>
    /// <param name="errorRepository">Repository for transaction processing errors.</param>
    /// <param name="retryPolicy">Retry policy used to execute the workflow with resilience.</param>
    /// <param name="logger">Logger instance.</param>
    public ConsolidationWorkflow(
        RegisterBatchStep registerBatchStep,
        LoadTransactionsStep loadTransactionsStep,
        ValidateTransactionsStep validateTransactionsStep,
        AggregateTransactionsStep aggregateTransactionsStep,
        UpsertDailyBalanceStep upsertDailyBalanceStep,
        FinalizeBatchStep finalizeBatchStep,
        IDailyBatchRepository dailyBatchRepository,
        ITransactionRepository transactionRepository,
        ITransactionProcessingErrorRepository errorRepository,
        IRetryPolicy retryPolicy,
        ILogger<ConsolidationWorkflow> logger)
    {
        _registerBatchStep = registerBatchStep;
        _loadTransactionsStep = loadTransactionsStep;
        _validateTransactionsStep = validateTransactionsStep;
        _aggregateTransactionsStep = aggregateTransactionsStep;
        _upsertDailyBalanceStep = upsertDailyBalanceStep;
        _finalizeBatchStep = finalizeBatchStep;
        _dailyBatchRepository = dailyBatchRepository;
        _transactionRepository = transactionRepository;
        _errorRepository = errorRepository;
        _retryPolicy = retryPolicy;
        _logger = logger;
    }

    /// <summary>
    /// Executes the consolidation workflow for the provided batch message.
    /// </summary>
    /// <param name="message">The batch message containing transaction identifiers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="BatchAlreadyProcessedException">Thrown when the batch was already processed.</exception>
    /// <exception cref="BatchProcessingException">Thrown when an unexpected error occurs during processing.</exception>
    public async Task ExecuteAsync(ConsolidationBatchMessage message, CancellationToken cancellationToken)
    {
        var context = new BatchExecutionContext
        {
            Message = message
        };

        try
        {
            await _retryPolicy.ExecuteAsync(
                token => ExecutePipelineAsync(context, token),
                cancellationToken);
        }
        catch (BatchAlreadyProcessedException)
        {
            _logger.LogWarning(
                BatchLogMessages.Workflow.AlreadyProcessed,
                message.BatchId);

            throw;
        }
        catch (Exception exception)
        {
            await HandleFailureAsync(context, exception, cancellationToken);

            throw new BatchProcessingException(
                string.Format(ErrorMessages.Batch.ProcessingFailed, message.BatchId),
                exception);
        }
    }

    /// <summary>
    /// Executes the internal pipeline steps in sequence.
    /// </summary>
    /// <param name="context">Execution context containing batch state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ExecutePipelineAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        await _registerBatchStep.ExecuteAsync(context, cancellationToken);
        await _loadTransactionsStep.ExecuteAsync(context, cancellationToken);
        await _validateTransactionsStep.ExecuteAsync(context, cancellationToken);

        if (context.Transactions.Count == 0)
        {
            _logger.LogWarning(
                BatchLogMessages.Workflow.NoValidTransactionsAfterValidation,
                context.Message.BatchId);

            await _finalizeBatchStep.ExecuteAsync(context, cancellationToken);
            return;
        }

        await _aggregateTransactionsStep.ExecuteAsync(context, cancellationToken);
        await _upsertDailyBalanceStep.ExecuteAsync(context, cancellationToken);
        await _finalizeBatchStep.ExecuteAsync(context, cancellationToken);
    }

    /// <summary>
    /// Handles failures during workflow execution by updating batch state,
    /// persisting errors for the remaining transactions in the pipeline,
    /// and marking only those transactions for manual review.
    /// </summary>
    /// <param name="context">Execution context.</param>
    /// <param name="exception">Exception that occurred.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleFailureAsync(
        BatchExecutionContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var retryCount = (context.Batch?.RetryCount ?? 0) + 1;
        var message = context.Message;

        await _dailyBatchRepository.MarkAsFailedAsync(
            message.BatchId,
            exception.Message,
            retryCount,
            cancellationToken);

        var remainingTransactionIds = context.Transactions
            .Select(transaction => transaction.Id)
            .Distinct()
            .ToArray();

        if (remainingTransactionIds.Length == 0)
        {
            _logger.LogError(
                exception,
                BatchLogMessages.Workflow.MovedToManualReview,
                message.BatchId,
                retryCount,
                0);

            return;
        }

        var occurredOnUtc = DateTime.UtcNow;

        var failures = remainingTransactionIds
            .Select(id => new TransactionProcessingError(
                id,
                message.BatchId,
                exception.Message,
                occurredOnUtc))
            .ToArray();

        await _errorRepository.InsertAsync(failures, cancellationToken);

        await _transactionRepository.MarkAsFailedAsync(
            remainingTransactionIds,
            message.BatchId,
            retryCount,
            Domain.Enums.TransactionProcessingStatus.PendingManualReview,
            cancellationToken);

        _logger.LogError(
            exception,
            BatchLogMessages.Workflow.MovedToManualReview,
            message.BatchId,
            retryCount,
            remainingTransactionIds.Length);
    }
}
