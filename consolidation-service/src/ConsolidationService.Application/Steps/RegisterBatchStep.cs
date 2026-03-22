using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Models;
using ConsolidationService.Domain.Enums;
using ConsolidationService.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Steps;

/// <summary>
/// Creates the batch row or restores the latest persisted state before processing continues.
/// </summary>
public sealed class RegisterBatchStep : IConsolidationWorkflowStep
{
    private readonly IDailyBatchRepository _dailyBatchRepository;
    private readonly ILogger<RegisterBatchStep> _logger;

    public RegisterBatchStep(IDailyBatchRepository dailyBatchRepository, ILogger<RegisterBatchStep> logger)
    {
        _dailyBatchRepository = dailyBatchRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        var currentBatch = await _dailyBatchRepository.GetAsync(context.Message.BatchId, cancellationToken);
        if (currentBatch is { Status: BatchStatus.Succeeded or BatchStatus.Ignored })
        {
            throw new BatchAlreadyProcessedException(context.Message.BatchId);
        }

        context.Batch = await _dailyBatchRepository.UpsertPendingAsync(
            context.Message.BatchId,
            context.Message.CorrelationId,
            context.Message.TransactionIds.Count,
            cancellationToken);

        await _dailyBatchRepository.MarkAsProcessingAsync(context.Message.BatchId, cancellationToken);

        _logger.LogInformation(
            "Batch registered for processing. BatchId: {BatchId}, TransactionCount: {TransactionCount}",
            context.Message.BatchId,
            context.Message.TransactionIds.Count);
    }
}
