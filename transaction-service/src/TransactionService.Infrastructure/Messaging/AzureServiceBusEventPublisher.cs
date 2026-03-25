using System.Text;
using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TransactionService.Application.Abstractions.Messaging;
using TransactionService.Application.Resources;
using TransactionService.Infrastructure.Configuration;
using TransactionService.Infrastructure.Resources;

namespace TransactionService.Infrastructure.Messaging;

/// <summary>
/// Publishes integration events to Azure Service Bus.
/// </summary>
public sealed class AzureServiceBusEventPublisher : IIntegrationEventPublisher, IAsyncDisposable
{
    private readonly Lazy<ServiceBusClient> _client;
    private readonly Lazy<ServiceBusSender> _sender;
    private readonly ILogger<AzureServiceBusEventPublisher> _logger;
    private readonly string _topicName;

    public AzureServiceBusEventPublisher(
        IOptions<ServiceBusOptions> options,
        ILogger<AzureServiceBusEventPublisher> logger)
    {
        var settings = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(settings.TopicName))
        {
            throw new InvalidOperationException(MessageCatalog.ServiceBusTopicRequired);
        }

        _topicName = settings.TopicName;

        _client = new Lazy<ServiceBusClient>(() => CreateClient(settings));
        _sender = new Lazy<ServiceBusSender>(() => _client.Value.CreateSender(settings.TopicName));
    }

    /// <summary>
    /// Publishes lightweight transaction batch (BatchId + TransactionIds).
    /// </summary>
    public async Task PublishBatchAsync(
        TransactionBatchMessage message,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(message);

        var serviceBusMessage = new ServiceBusMessage(payload)
        {
            MessageId = message.BatchId.ToString(),
            Subject = "TransactionBatch",
            ContentType = "application/json"
        };

        serviceBusMessage.ApplicationProperties["batchId"] = message.BatchId;
        serviceBusMessage.ApplicationProperties["transactionCount"] = message.TransactionIds.Count;

        _logger.LogInformation(
            InfrastructureMessageCatalog.Logs.SendingTransactionBatch,
            _client.Value.FullyQualifiedNamespace,
            _topicName,
            message.BatchId,
            message.TransactionIds.Count);

        await _sender.Value.SendMessageAsync(serviceBusMessage, cancellationToken);

        _logger.LogInformation(
            InfrastructureMessageCatalog.Logs.TransactionBatchPublished,
            message.BatchId,
            message.TransactionIds.Count);
    }

    /// <summary>
    /// Publishes individual integration event (fallback).
    /// </summary>
    public async Task PublishAsync(
        StoredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(integrationEvent.Payload))
        {
            ContentType = "application/json",
            MessageId = integrationEvent.EventId.ToString(),
            CorrelationId = integrationEvent.CorrelationId,
            Subject = integrationEvent.EventName
        };

        message.ApplicationProperties["eventName"] = integrationEvent.EventName;
        message.ApplicationProperties["eventVersion"] = integrationEvent.EventVersion;
        message.ApplicationProperties["aggregateId"] = integrationEvent.AggregateId;
        message.ApplicationProperties["correlationId"] = integrationEvent.CorrelationId;
        message.ApplicationProperties["occurredOnUtc"] = integrationEvent.OccurredOnUtc;

        _logger.LogInformation(
            InfrastructureMessageCatalog.PublishingIntegrationEvent,
            integrationEvent.EventName,
            integrationEvent.EventId);

        await _sender.Value.SendMessageAsync(message, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_sender.IsValueCreated)
        {
            await _sender.Value.DisposeAsync();
        }

        if (_client.IsValueCreated)
        {
            await _client.Value.DisposeAsync();
        }
    }

    private static ServiceBusClient CreateClient(ServiceBusOptions settings)
    {
        if (settings.UseManagedIdentity)
        {
            return new ServiceBusClient(
                settings.FullyQualifiedNamespace ?? throw new InvalidOperationException(
                    InfrastructureMessageCatalog.ServiceBusNamespaceNotFound),
                new DefaultAzureCredential());
        }

        return new ServiceBusClient(
            settings.ConnectionString ?? throw new InvalidOperationException(
                InfrastructureMessageCatalog.ServiceBusConnectionStringNotFound));
    }
}
