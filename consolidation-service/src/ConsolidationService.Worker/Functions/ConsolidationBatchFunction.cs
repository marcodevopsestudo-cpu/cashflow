using System.Text.Json;
using ConsolidationService.Application.Commands.ProcessConsolidationBatch;
using ConsolidationService.Application.Contracts;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ConsolidationService.Worker.Functions;

/// <summary>
/// Receives batch messages from Azure Service Bus and starts the application workflow.
/// </summary>
public sealed class ConsolidationBatchFunction
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IMediator _mediator;
    private readonly ILogger<ConsolidationBatchFunction> _logger;

    public ConsolidationBatchFunction(IMediator mediator, ILogger<ConsolidationBatchFunction> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [Function(nameof(ConsolidationBatchFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger("%ServiceBusTopicName%", "%ServiceBusSubscriptionName%", Connection = "ServiceBusConnection")]
        string messageBody,
        FunctionContext functionContext,
        CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<ConsolidationBatchMessage>(messageBody, SerializerOptions)
                      ?? throw new InvalidOperationException("Service Bus message body could not be deserialized.");

        var messageId = functionContext.BindingContext.BindingData.TryGetValue("MessageId", out var value)
            ? value?.ToString() ?? Guid.NewGuid().ToString("N")
            : Guid.NewGuid().ToString("N");

        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   ["CorrelationId"] = message.CorrelationId,
                   ["BatchId"] = message.BatchId,
                   ["MessageId"] = messageId
               }))
        {
            _logger.LogInformation(
                "Service Bus message received. BatchId: {BatchId}, MessageId: {MessageId}, TransactionCount: {TransactionCount}",
                message.BatchId,
                messageId,
                message.TransactionIds.Count);

            await _mediator.Send(new ProcessConsolidationBatchCommand(message, messageId), cancellationToken);
        }
    }
}
