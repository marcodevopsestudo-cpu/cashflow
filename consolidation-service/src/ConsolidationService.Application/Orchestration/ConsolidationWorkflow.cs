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
/// Orchestrates the full consolidation workflow pipeline for a given batch message.
/// This workflow is responsible for executing all processing steps in order,
/// handling retries, logging execution progress, and managing failure scenarios.
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
    private readonly IUnitOfWork _unitOfWork;

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
    /// <param name="errorRepository">Repository for storing transaction processing errors.</param>
    /// <param name="retryPolicy">Retry policy used to handle transient failures.</param>
    /// <param name="logger">Logger instance for observability.</param>
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
        IUnitOfWork unitOfWork,
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
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Executes the consolidation workflow for a given batch message.
    /// This method controls the full execution lifecycle including retry handling,
    /// logging, and failure management.
    /// </summary>
    /// <param name="message">The batch message containing transaction identifiers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ExecuteAsync(ConsolidationBatchMessage message, CancellationToken cancellationToken)
    {
        var context = new BatchExecutionContext
        {
            Message = message
        };

        _logger.LogInformation(
            "Starting consolidation workflow. BatchId={BatchId}, CorrelationId={CorrelationId}, TransactionCount={TransactionCount}",
            message.BatchId,
            message.CorrelationId,
            message.TransactionIds?.Count ?? 0);

        try
        {
            await _retryPolicy.ExecuteAsync(
                token => ExecutePipelineAsync(context, token),
            cancellationToken);

            _logger.LogInformation(
                "Consolidation workflow completed successfully. BatchId={BatchId}",
                message.BatchId);
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
            _logger.LogError(
                exception,
                "Unhandled exception during consolidation workflow. BatchId={BatchId}, CorrelationId={CorrelationId}",
                message.BatchId,
                message.CorrelationId);

            await HandleFailureAsync(context, exception, cancellationToken);

            throw new BatchProcessingException(
                string.Format(ErrorMessages.Batch.ProcessingFailed, message.BatchId),
                exception);
        }
    }

    private async Task ExecutePipelineAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Pipeline started. BatchId={BatchId}", context.Message.BatchId);

        _logger.LogInformation("Step: RegisterBatch");
        await _registerBatchStep.ExecuteAsync(context, cancellationToken);

        _logger.LogInformation("Step: LoadTransactions");
        await _loadTransactionsStep.ExecuteAsync(context, cancellationToken);

        _logger.LogInformation(
            "Transactions loaded. BatchId={BatchId}, Count={Count}",
            context.Message.BatchId,
            context.Transactions?.Count ?? 0);

        _logger.LogInformation("Step: ValidateTransactions");
        await _validateTransactionsStep.ExecuteAsync(context, cancellationToken);

        _logger.LogInformation(
            "Transactions after validation. BatchId={BatchId}, Count={Count}",
            context.Message.BatchId,
            context.Transactions?.Count ?? 0);

        if (context.Transactions?.Count == 0)
        {
            _logger.LogWarning(
                BatchLogMessages.Workflow.NoValidTransactionsAfterValidation,
                context.Message.BatchId);

            _logger.LogInformation(
                "Finalizing batch with no valid transactions. BatchId={BatchId}",
                context.Message.BatchId);

            await _dailyBatchRepository.MarkAsSucceededAsync(
            context.Message.BatchId,
            cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        _logger.LogInformation("Step: AggregateTransactions");
        await _aggregateTransactionsStep.ExecuteAsync(context, cancellationToken);

        _logger.LogInformation("Step: UpsertDailyBalance");
        await _upsertDailyBalanceStep.ExecuteAsync(context, cancellationToken);

        _logger.LogInformation("Step: FinalizeBatch");
        await _finalizeBatchStep.ExecuteAsync(context, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Pipeline finished. BatchId={BatchId}", context.Message.BatchId);



        _logger.LogInformation("Pipeline finished. BatchId={BatchId}", context.Message.BatchId);
    }

    private async Task HandleFailureAsync(
        BatchExecutionContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var retryCount = (context.Batch?.RetryCount ?? 0) + 1;
        var message = context.Message;

        _logger.LogError(
            exception,
            "Handling failure. BatchId={BatchId}, RetryCount={RetryCount}",
            message.BatchId,
            retryCount);

        await _dailyBatchRepository.MarkAsFailedAsync(
            message.BatchId,
            exception.Message,
            retryCount,
            cancellationToken);

        var remainingTransactionIds = context.Transactions
            .Select(transaction => transaction.TransactionId)
            .Distinct()
            .ToArray();

        _logger.LogInformation(
            "Remaining transactions to handle after failure. BatchId={BatchId}, Count={Count}",
            message.BatchId,
            remainingTransactionIds.Length);

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
            .Select(id => new TransactionProcessingError { Id =  id, BatchId = message.BatchId, ErrorMessage =  exception.Message,CreatedAtUtc =  occurredOnUtc})
            .ToArray();

        await _errorRepository.InsertAsync(failures, cancellationToken);

        await _transactionRepository.MarkAsFailedAsync(
            remainingTransactionIds,
            message.BatchId,
            retryCount,
            Domain.Enums.TransactionStatus.PendingManualReview,
            cancellationToken);

        _logger.LogError(
            exception,
            BatchLogMessages.Workflow.MovedToManualReview,
            message.BatchId,
            retryCount,
            remainingTransactionIds.Length);
    }
}
