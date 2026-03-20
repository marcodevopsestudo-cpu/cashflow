using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TransactionService.Application.Abstractions.Messaging;
using TransactionService.Application.Transactions.Commands.ProcessOutBox;
using Xunit;

namespace TransactionService.UnitTests.Application.Outbox;

/// <summary>
/// Contains unit tests for the process outbox command handler.
/// </summary>
public sealed class ProcessOutboxCommandHandlerTests
{
    /// <summary>
    /// Verifies that the handler processes outbox messages and returns the correct result.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldProcessOutboxMessagesAndReturnProcessedCount()
    {
        // Arrange
        var outboxProcessorMock = new Mock<IOutboxProcessor>();

        var expectedProcessedCount = 10;

        outboxProcessorMock
            .Setup(x => x.ProcessPendingMessagesAsync(expectedProcessedCount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProcessedCount);

        var handler = new ProcessOutboxCommandHandler(
            outboxProcessorMock.Object,
            NullLogger<ProcessOutboxCommandHandler>.Instance);

        var command = new ProcessOutboxCommand(expectedProcessedCount, Guid.NewGuid().ToString("N"));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.processedItems.Should().Be(expectedProcessedCount);

        outboxProcessorMock.Verify(
            x => x.ProcessPendingMessagesAsync(expectedProcessedCount, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}