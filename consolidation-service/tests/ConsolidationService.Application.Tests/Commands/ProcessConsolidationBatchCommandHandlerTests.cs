using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Commands.ProcessConsolidationBatch;
using ConsolidationService.Application.Contracts;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace ConsolidationService.Application.Tests.Commands;

/// <summary>
/// Unit tests for <see cref="ProcessConsolidationBatchCommandHandler"/>.
/// </summary>
public sealed class ProcessConsolidationBatchCommandHandlerTests
{
    /// <summary>
    /// Ensures that the handler delegates the request to the workflow with the correct parameters.
    /// </summary>
    [Fact]
    public async Task Should_forward_request_to_workflow()
    {
        // Arrange
        var workflow = Substitute.For<IConsolidationWorkflow>();

        var handler = new ProcessConsolidationBatchCommandHandler(
            workflow,
            NullLogger<ProcessConsolidationBatchCommandHandler>.Instance);


        var guidA = Guid.NewGuid();
        var guidB = Guid.NewGuid();
        var message = new ConsolidationBatchMessage
        {
            BatchId = Guid.NewGuid(),
            CorrelationId = "corr-002",
            PublishedAtUtc = DateTime.UtcNow,
            TransactionIds = new[] { guidA, guidB }
        };

        var command = new ProcessConsolidationBatchCommand(message, "msg-002");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await workflow.Received(1)
            .ExecuteAsync(message, CancellationToken.None);
    }
}
