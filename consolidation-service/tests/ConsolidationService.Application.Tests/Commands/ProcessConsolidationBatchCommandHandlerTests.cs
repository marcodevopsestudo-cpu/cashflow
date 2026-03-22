using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Commands.ProcessConsolidationBatch;
using ConsolidationService.Application.Contracts;
using ConsolidationService.Application.Telemetry;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace ConsolidationService.Application.Tests.Commands;

public sealed class ProcessConsolidationBatchCommandHandlerTests
{
    [Fact]
    public async Task Should_forward_request_to_workflow_and_set_telemetry_context()
    {
        var workflow = Substitute.For<IConsolidationWorkflow>();
        var telemetry = new TelemetryContextAccessor();
        var handler = new ProcessConsolidationBatchCommandHandler(workflow, telemetry, NullLogger<ProcessConsolidationBatchCommandHandler>.Instance);

        var message = new ConsolidationBatchMessage
        {
            BatchId = Guid.NewGuid(),
            CorrelationId = "corr-002",
            PublishedAtUtc = DateTime.UtcNow,
            TransactionIds = new[] { 10L, 11L }
        };

        await handler.Handle(new ProcessConsolidationBatchCommand(message, "msg-002"), CancellationToken.None);

        telemetry.CorrelationId.Should().Be("corr-002");
        telemetry.BatchId.Should().Be(message.BatchId);
        telemetry.MessageId.Should().Be("msg-002");
        await workflow.Received(1).ExecuteAsync(message, "msg-002", CancellationToken.None);
    }
}
