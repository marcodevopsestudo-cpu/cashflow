using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TransactionService.Api.Common.Extensions;
using TransactionService.Application.Transactions.Commands.ProcessOutBox;
using TransactionService.Api.Resources;

namespace TransactionService.Api.Functions;

/// <summary>
/// Timer-triggered function that drains the outbox table and publishes pending integration events.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ProcessOutboxFunction"/> class.
/// </remarks>
public sealed class ProcessOutboxFunction(IMediator mediator, ILogger<ProcessOutboxFunction> logger)
{
    private const int BatchSize = 50;
    private readonly ILogger<ProcessOutboxFunction> _logger = logger;
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Executes the timer-triggered outbox processor.
    /// </summary>
    [Function(nameof(ProcessOutboxFunction))]
    public async Task Run([TimerTrigger("*/30 * * * * *")] TimerInfo timer,  FunctionContext context, CancellationToken cancellationToken)
    {
        var correlationId = context.GetCorrelationId();
        var result = await _mediator.Send(new ProcessOutboxCommand(BatchSize, correlationId), cancellationToken);

        _logger.LogInformation(
            ApiMessageCatalog.OutboxProcessingCompleted,
            timer.ScheduleStatus?.Next,
            result.ProcessedItems);
    }
}
