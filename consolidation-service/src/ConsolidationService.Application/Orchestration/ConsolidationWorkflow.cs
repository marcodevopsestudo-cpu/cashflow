using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Contracts;
using ConsolidationService.Application.Models;
using ConsolidationService.Application.Steps;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;
using ConsolidationService.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Orchestration;

/// <summary>
/// Executes the consolidation pipeline in a fixed and reviewable order.
/// </summary>
public sealed class ConsolidationWorkflow : IConsolidationWorkflow
{
    private readonly IConsolidationWorkflowStep _registerBatchStep;
    private readonly IConsolidationWorkflowStep _loadTransactionsStep;
    private readonly IConsolidationWorkflowStep _aggregateTransactionsStep;
    private readonly IConsolidationWorkflowStep _upsertDailyBalanceStep;
    private readonly IConsolidationWorkflowStep _finalizeBatchStep;
    private readonly IDailyBatchRepository _dailyBatchRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionProcessingErrorRepository _errorRepository;
    private readonly IRetryPolicy _retryPolicy;
    private readonly ILogger<ConsolidationWorkflow> _logger;

    public ConsolidationWorkflow(
        IConsolidationWorkflowStep registerBatchStep,
        IConsolidationWorkflowStep loadTransactionsStep,
        IConsolidationWorkflowStep aggregateTransactionsStep,
        IConsolidationWorkflowStep upsertDailyBalanceStep,
        IConsolidationWorkflowStep finalizeBatchStep,
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

    public async Task ExecuteAsync(ConsolidationBatchMessage message, string messageId, CancellationToken cancellationToken)
    {
        var context = new BatchExecutionContext
        {
            Message = message,
            MessageId = messageId
        };

        try
        {
           
            await _retryPolicy.ExecuteAsync(async token =>
            {
                await _registerBatchStep.ExecuteAsync(context, token);
                await _loadTransactionsStep.ExecuteAsync(context, token);
                await _aggregateTransactionsStep.ExecuteAsync(context, token);
                await _upsertDailyBalanceStep.ExecuteAsync(context, token);
                await _finalizeBatchStep.ExecuteAsync(context, token);

            }, cancellationToken);
        }
        catch (BatchAlreadyProcessedException)
        {
            _logger.LogWarning("Batch {BatchId} has already been processed or ignored.", message.BatchId);
            throw;
        }
        catch (Exception exception)
        {
            await HandleFailureAsync(context, exception, cancellationToken);
            throw new BatchProcessingException($"Failed to process batch '{message.BatchId}'.", exception);
        }
    }

    private async Task HandleFailureAsync(BatchExecutionContext context, Exception exception, CancellationToken cancellationToken)
    {
        var retryCount = (context.Batch?.RetryCount ?? 0) + 1;
        await _dailyBatchRepository.MarkAsFailedAsync(context.Message.BatchId, exception.Message, retryCount, cancellationToken);

        var failures = context.Message.TransactionIds.Select(id => new TransactionProcessingError
        {
            BatchId = context.Message.BatchId,
            TransactionId = id,
            CorrelationId = context.Message.CorrelationId,
            ErrorCode = exception.GetType().Name,
            ErrorMessage = exception.Message,
            StackTrace = exception.ToString(),
            CreatedAtUtc = DateTime.UtcNow,
            RetryCount = retryCount,
            Status = "PendingManualReview"
        }).ToArray();

        await _errorRepository.InsertAsync(failures, cancellationToken);

        await _transactionRepository.MarkAsFailedAsync(
            context.Message.TransactionIds.ToArray(),
            context.Message.BatchId,
            retryCount,
            TransactionProcessingStatus.PendingManualReview,
            cancellationToken);

        _logger.LogError(
            exception,
            "Batch processing moved to manual review. BatchId: {BatchId}, RetryCount: {RetryCount}, AffectedTransactions: {TransactionCount}",
            context.Message.BatchId,
            retryCount,
            context.Message.TransactionIds.Count);
    }
}
