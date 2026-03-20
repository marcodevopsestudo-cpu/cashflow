using Azure.Core;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.WebSockets;
using TransactionService.Api.Common.Extensions;
using TransactionService.Application.Abstractions.Messaging;
using TransactionService.Application.Transactions.Commands.CreateTransaction;
using TransactionService.Application.Transactions.Commands.ProcessOutBox;

namespace TransactionService.Api.Functions;

/// <summary>
/// Processes pending outbox messages and publishes them to Azure Service Bus.
/// </summary>
public sealed class ProcessOutboxFunction
{
   
    private readonly ILogger<ProcessOutboxFunction> _logger;
    private readonly IMediator _mediator;
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessOutboxFunction"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="logger">The logger.</param>
    public ProcessOutboxFunction(IMediator mediator, ILogger<ProcessOutboxFunction> logger)
    {
        _mediator = mediator;   
        _logger = logger;
    }

    /// <summary>
    /// Executes the timer-triggered outbox processor.
    /// </summary>
    /// <param name="timer">The timer information.</param>
    /// <param name="context">The HTTP FunctionContext.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [Function(nameof(ProcessOutboxFunction))]
    public async Task Run([TimerTrigger("*/30 * * * * *")] TimerInfo timer, 
        CancellationToken cancellationToken,
        FunctionContext context)
    {
        var correlationId = context.GetCorrelationId();
        var result =  await _mediator.Send(new ProcessOutboxCommand(50, correlationId));
        _logger.LogInformation(
        "Outbox processing completed. Processed {ProcessedCount} messages.",
        result.processedItems);
    }

}

