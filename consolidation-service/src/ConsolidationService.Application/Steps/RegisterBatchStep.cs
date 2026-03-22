using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Messages.Logs;
using ConsolidationService.Application.Models;
using ConsolidationService.Domain.Enums;
using ConsolidationService.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Steps;

/// <summary>
/// Registers the batch for processing or restores its persisted state before the workflow continues.
/// </summary>
public sealed class RegisterBatchStep
{
    private readonly IDailyBatchRepository _dailyBatchRepository;
    private readonly ILogger<RegisterBatchStep> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterBatchStep"/> class.
    /// </summary>
    /// <param name="dailyBatchRepository">
    /// Repository used to retrieve and update the batch lifecycle state.
    /// </param>
    /// <param name="logger">
    /// Logger used to record structured information about batch registration.
    /// </param>
    public RegisterBatchStep(
        IDailyBatchRepository dailyBatchRepository,
        ILogger<RegisterBatchStep> logger)
    {
        _dailyBatchRepository = dailyBatchRepository;
        _logger = logger;
    }

    /// <summary>
    /// Ensures the batch is eligible for processing, persists its pending state,
    /// and marks it as processing.
    /// </summary>
    /// <param name="context">
    /// The workflow execution context containing the batch message and mutable execution state.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous registration operation.
    /// </returns>
    /// <exception cref="BatchAlreadyProcessedException">
    /// Thrown when the batch has already been completed or explicitly ignored.
    /// </exception>
    public async Task ExecuteAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        var message = context.Message;

        var currentBatch = await _dailyBatchRepository.GetAsync(
            message.BatchId,
            cancellationToken);

        if (currentBatch is { Status: BatchStatus.Succeeded or BatchStatus.Ignored })
        {
            throw new BatchAlreadyProcessedException(message.BatchId);
        }

        context.Batch = await _dailyBatchRepository.UpsertPendingAsync(
            message.BatchId,
            message.CorrelationId,
            message.TransactionIds.Count,
            cancellationToken);

        await _dailyBatchRepository.MarkAsProcessingAsync(
            message.BatchId,
            cancellationToken);

        _logger.LogInformation(
            BatchLogMessages.Workflow.RegisteredForProcessing,
            message.BatchId,
            message.TransactionIds.Count);
    }
}
