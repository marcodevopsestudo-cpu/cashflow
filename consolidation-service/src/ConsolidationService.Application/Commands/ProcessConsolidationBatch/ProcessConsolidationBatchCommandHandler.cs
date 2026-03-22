using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Messages.Logs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Application.Commands.ProcessConsolidationBatch;

/// <summary>
/// Handles <see cref="ProcessConsolidationBatchCommand"/> requests by creating the logging scope
/// for the current batch and delegating execution to the consolidation workflow.
/// </summary>
public sealed class ProcessConsolidationBatchCommandHandler : IRequestHandler<ProcessConsolidationBatchCommand>
{
    private readonly IConsolidationWorkflow _workflow;
    private readonly ILogger<ProcessConsolidationBatchCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessConsolidationBatchCommandHandler"/> class.
    /// </summary>
    /// <param name="workflow">
    /// The workflow responsible for orchestrating the consolidation batch processing steps.
    /// </param>
    /// <param name="logger">
    /// The logger used to record structured information about the batch processing lifecycle.
    /// </param>
    public ProcessConsolidationBatchCommandHandler(
        IConsolidationWorkflow workflow,
        ILogger<ProcessConsolidationBatchCommandHandler> logger)
    {
        _workflow = workflow;
        _logger = logger;
    }

    /// <summary>
    /// Handles the incoming batch-processing command.
    /// </summary>
    /// <param name="request">
    /// The command containing the consolidation batch message and the messaging infrastructure identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous handling operation.
    /// </returns>
    public async Task Handle(ProcessConsolidationBatchCommand request, CancellationToken cancellationToken)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = request.Message.CorrelationId,
            ["BatchId"] = request.Message.BatchId,
            ["MessageId"] = request.MessageId
        }))
        {
            _logger.LogInformation(
                BatchLogMessages.Workflow.ProcessingStarted,
                request.Message.BatchId,
                request.MessageId,
                request.Message.TransactionIds.Count);

            await _workflow.ExecuteAsync(request.Message, cancellationToken);

            _logger.LogInformation(
                BatchLogMessages.Workflow.ProcessingFinished,
                request.Message.BatchId,
                request.MessageId);
        }
    }
}
