using ConsolidationService.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Commands.ProcessConsolidationBatch;

/// <summary>
/// Handles a batch-processing request and delegates execution to the workflow orchestrator.
/// </summary>
public sealed class ProcessConsolidationBatchCommandHandler : IRequestHandler<ProcessConsolidationBatchCommand>
{
    private readonly IConsolidationWorkflow _workflow;
    private readonly ITelemetryContextAccessor _telemetryContextAccessor;
    private readonly ILogger<ProcessConsolidationBatchCommandHandler> _logger;

    public ProcessConsolidationBatchCommandHandler(
        IConsolidationWorkflow workflow,
        ITelemetryContextAccessor telemetryContextAccessor,
        ILogger<ProcessConsolidationBatchCommandHandler> logger)
    {
        _workflow = workflow;
        _telemetryContextAccessor = telemetryContextAccessor;
        _logger = logger;
    }

    public async Task Handle(ProcessConsolidationBatchCommand request, CancellationToken cancellationToken)
    {
        _telemetryContextAccessor.CorrelationId = request.Message.CorrelationId;
        _telemetryContextAccessor.BatchId = request.Message.BatchId;
        _telemetryContextAccessor.MessageId = request.MessageId;

        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   ["CorrelationId"] = request.Message.CorrelationId,
                   ["BatchId"] = request.Message.BatchId,
                   ["MessageId"] = request.MessageId
               }))
        {
            _logger.LogInformation(
                "Starting consolidation batch processing. BatchId: {BatchId}, MessageId: {MessageId}, TransactionCount: {TransactionCount}",
                request.Message.BatchId,
                request.MessageId,
                request.Message.TransactionIds.Count);

            await _workflow.ExecuteAsync(request.Message, request.MessageId, cancellationToken);

            _logger.LogInformation(
                "Finished consolidation batch processing. BatchId: {BatchId}, MessageId: {MessageId}",
                request.Message.BatchId,
                request.MessageId);
        }
    }
}
