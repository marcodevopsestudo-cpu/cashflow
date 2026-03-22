using FluentAssertions;
using TransactionService.Domain.Entities;
using Xunit;

namespace TransactionService.UnitTests.Domain;

/// <summary>
/// Unit tests for <see cref="OutboxMessage"/>.
/// </summary>
public sealed class OutboxMessageTests
{
    /// <summary>
    /// Verifies that calling <see cref="OutboxMessage.MarkAsProcessed"/> sets the processed timestamp
    /// and clears any previous error.
    /// </summary>
    [Fact]
    public void MarkAsProcessed_ShouldSetProcessedTimestampAndClearError()
    {
        // Arrange
        var message = OutboxMessage.Create(
            Guid.NewGuid(),
            "TransactionCreated",
            1,
            Guid.NewGuid().ToString(),
            "{}",
            "corr-123",
            DateTime.UtcNow);

        message.MarkAsFailed("boom");

        // Act
        message.MarkAsProcessed();

        // Assert
        message.ProcessedOnUtc.Should().NotBeNull();
        message.Error.Should().BeNull();
    }

    /// <summary>
    /// Verifies that calling <see cref="OutboxMessage.MarkAsFailed"/> increases the retry count
    /// and stores the error message.
    /// </summary>
    [Fact]
    public void MarkAsFailed_ShouldIncreaseRetryCountAndStoreError()
    {
        // Arrange
        var message = OutboxMessage.Create(
            Guid.NewGuid(),
            "TransactionCreated",
            1,
            Guid.NewGuid().ToString(),
            "{}",
            "corr-123",
            DateTime.UtcNow);

        // Act
        message.MarkAsFailed("failure");

        // Assert
        message.RetryCount.Should().Be(1);
        message.Error.Should().Be("failure");
    }
}
