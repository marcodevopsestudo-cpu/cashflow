using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Contracts;
using ConsolidationService.Application.Messages.Logs;
using ConsolidationService.Application.Models;
using ConsolidationService.Application.Steps;
using ConsolidationService.Domain.Constants;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;
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
    /// <param name="registerBatchStep">
    /// Step responsible for registering or retrieving the batch before processing begins.
    /// </param>
    /// <param name="loadTransactionsStep">
    /// Step responsible for loading pending transactions associated with the batch.
    /// </param>
    /// <param name="aggregateTransactionsStep">
    /// Step responsible for aggregating transactions into daily balances.
    /// </param>
    /// <param name="upsertDailyBalanceStep">
    /// Step responsible for persisting aggregated balances.
    /// </param>
    /// <param name="finalizeBatchStep">
    /// Step responsible for marking the batch as successfully processed.
    /// </param>
    /// <param name="dailyBatchRepository">
    /// Repository used to manage batch lifecycle state transitions.
    /// </param>
    /// <param name="transactionRepository">
    /// Repository used to update transaction processing states.
    /// </param>
    /// <param name="errorRepository">
    /// Repository used to persist transaction processing errors for manual review.
    /// </param>
    /// <param name="retryPolicy">
    /// Policy used to execute the workflow with retry and backoff strategies.
    /// </param>
    /// <param name="logger">
    /// Logger used to record structured information about workflow execution.
    /// </param>
    public ConsolidationWorkflow(
        RegisterBatchStep registerBatchStep,
        LoadTransactionsStep loadTransactionsStep,
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
    /// Executes the consolidation workflow for a given batch message.
    /// </summary>
    /// <param name="message">The batch message to be processed.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
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
                string.Format(ErrorMessages.ProcessingFailed, message.BatchId),
                exception);
        }
    }

    /// <summary>
    /// Executes all workflow steps in order.
    /// </summary>
    private async Task ExecutePipelineAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        await _registerBatchStep.ExecuteAsync(context, cancellationToken);
        await _loadTransactionsStep.ExecuteAsync(context, cancellationToken);
        await _aggregateTransactionsStep.ExecuteAsync(context, cancellationToken);
        await _upsertDailyBalanceStep.ExecuteAsync(context, cancellationToken);
        await _finalizeBatchStep.ExecuteAsync(context, cancellationToken);
    }

    /// <summary>
    /// Handles workflow failure by persisting error state and moving transactions to manual review.
    /// </summary>
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

        var failures = message.TransactionIds.Select(id => new TransactionProcessingError
        {
            BatchId = message.BatchId,
            TransactionId = id,
            CorrelationId = message.CorrelationId,
            ErrorCode = exception.GetType().Name,
            ErrorMessage = exception.Message,
            StackTrace = exception.ToString(),
            CreatedAtUtc = DateTime.UtcNow,
            RetryCount = retryCount,
            Status = ProcessingErrorStatuses.PendingManualReview
        }).ToArray();

        await _errorRepository.InsertAsync(failures, cancellationToken);

        await _transactionRepository.MarkAsFailedAsync(
            message.TransactionIds,
            message.BatchId,
            retryCount,
            TransactionProcessingStatus.PendingManualReview,
            cancellationToken);

        _logger.LogError(
            exception,
            BatchLogMessages.Workflow.MovedToManualReview,
            message.BatchId,
            retryCount,
            message.TransactionIds.Count);
    }
}
