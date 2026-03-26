using System.Text.Json;
using ConsolidationService.Application.Commands.ProcessConsolidationBatch;
using ConsolidationService.Application.Contracts;
using ConsolidationService.Application.Messages.Logs;
using ConsolidationService.Domain.Constants;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Worker.Functions;

/// <summary>
/// Azure Function responsible for receiving consolidation batch messages from Azure Service Bus
/// and dispatching them to the application layer for processing.
/// </summary>
public sealed class ConsolidationBatchFunction
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IMediator _mediator;
    private readonly ILogger<ConsolidationBatchFunction> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsolidationBatchFunction"/> class.
    /// </summary>
    /// <param name="mediator">
    /// Mediator used to dispatch the batch-processing command to the application layer.
    /// </param>
    /// <param name="logger">
    /// Logger used to record structured information about message reception and execution flow.
    /// </param>
    public ConsolidationBatchFunction(
        IMediator mediator,
        ILogger<ConsolidationBatchFunction> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Receives a batch message from Azure Service Bus, deserializes its payload,
    /// creates the logging scope, and forwards the request to the application workflow.
    /// </summary>
    /// <param name="messageBody">
    /// The raw JSON payload received from Azure Service Bus.
    /// </param>
    /// <param name="functionContext">
    /// The Azure Functions execution context containing binding and invocation metadata.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous function execution.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the received message cannot be deserialized into a valid
    /// <see cref="ConsolidationBatchMessage"/>.
    /// </exception>
    [Function(nameof(ConsolidationBatchFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger("%ServiceBusTopicName%", "%ServiceBusSubscriptionName%", Connection = "ServiceBusConnection")]
        string messageBody,
        FunctionContext functionContext,
        CancellationToken cancellationToken)


    {
        var message = JsonSerializer.Deserialize<ConsolidationBatchMessage>(messageBody, SerializerOptions)
            ?? throw new InvalidOperationException(ErrorMessages.Worker.InvalidServiceBusMessage);

        var messageId = functionContext.BindingContext.BindingData.TryGetValue("MessageId", out var value)
            ? value?.ToString() ?? Guid.NewGuid().ToString("N")
            : Guid.NewGuid().ToString("N");

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["BatchId"] = message.BatchId,
            ["TransactionIds"] =  message.TransactionIds,
            ["CorrelationId"] = message.CorrelationId
        }))
        {
            _logger.LogInformation(
                BatchLogMessages.Worker.MessageReceived,
                message.BatchId,
                messageId,
                message.TransactionIds.Count);

            message.CorrelationId = Guid.NewGuid().ToString("N");   
            await _mediator.Send(
                new ProcessConsolidationBatchCommand(message, messageId),
                cancellationToken);
        }
    }

   
}
