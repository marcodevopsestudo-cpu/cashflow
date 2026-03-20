using FluentAssertions;
using TransactionService.Domain.Entities;
using Xunit;

namespace TransactionService.UnitTests.Domain;

public sealed class OutboxMessageTests
{
    [Fact]
    public void MarkAsProcessed_ShouldSetProcessedTimestampAndClearError()
    {
        var message = OutboxMessage.Create(
            Guid.NewGuid(),
            "TransactionCreated",
            1,
            Guid.NewGuid().ToString(),
            "{}",
            "corr-123",
            DateTime.UtcNow);

        message.MarkAsFailed("boom");
        message.MarkAsProcessed();

        message.ProcessedOnUtc.Should().NotBeNull();
        message.Error.Should().BeNull();
    }

    [Fact]
    public void MarkAsFailed_ShouldIncreaseRetryCountAndStoreError()
    {
        var message = OutboxMessage.Create(
            Guid.NewGuid(),
            "TransactionCreated",
            1,
            Guid.NewGuid().ToString(),
            "{}",
            "corr-123",
            DateTime.UtcNow);

        message.MarkAsFailed("failure");

        message.RetryCount.Should().Be(1);
        message.Error.Should().Be("failure");
    }
}
