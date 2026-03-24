//using FluentAssertions;
//using Microsoft.Extensions.Logging.Abstractions;
//using Moq;
//using TransactionService.Application.Abstractions.Messaging;
//using TransactionService.Application.Transactions.Commands.ProcessOutBox;
//using Xunit;

//namespace TransactionService.UnitTests.Application.Outbox;

///// <summary>
///// Unit tests for <see cref="ProcessOutboxCommandHandler"/>.
///// </summary>
//public sealed class ProcessOutboxCommandHandlerTests
//{
//    /// <summary>
//    /// Verifies that the handler processes pending outbox messages
//    /// and returns the correct number of processed items.
//    /// </summary>
//    [Fact]
//    public async Task Handle_ShouldProcessOutboxMessagesAndReturnProcessedCount()
//    {
//        // Arrange
//        var outboxProcessorMock = new Mock<IOutboxProcessor>();
//        var batchSize = 10;
//        var executionId = Guid.NewGuid().ToString("N");

//        outboxProcessorMock
//            .Setup(x => x.ProcessPendingMessagesAsync(batchSize, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(batchSize);

//        var handler = new ProcessOutboxCommandHandler(
//            outboxProcessorMock.Object,
//            NullLogger<ProcessOutboxCommandHandler>.Instance);

//        var command = new ProcessOutboxCommand(
//            batchSize,
//            executionId);

//        // Act
//        var result = await handler.Handle(command, CancellationToken.None);

//        // Assert
//        result.Should().NotBeNull();
//        result.ProcessedItems.Should().Be(batchSize);

//        outboxProcessorMock.Verify(
//            x => x.ProcessPendingMessagesAsync(batchSize, It.IsAny<CancellationToken>()),
//            Times.Once);

//        outboxProcessorMock.VerifyNoOtherCalls();
//    }
//}
