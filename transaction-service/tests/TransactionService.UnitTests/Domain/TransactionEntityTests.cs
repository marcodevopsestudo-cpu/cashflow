using FluentAssertions;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using Xunit;

namespace TransactionService.UnitTests.Domain;

/// <summary>
/// Unit tests for <see cref="Transaction"/> entity.
/// </summary>
public sealed class TransactionEntityTests
{
    /// <summary>
    /// Verifies that a transaction is correctly initialized with default values,
    /// including status, timestamps, and UTC normalization.
    /// </summary>
    [Fact]
    public void Create_ShouldInitializeTransactionWithReceivedStatus()
    {
        // Arrange
        var transactionDate = new DateTime(2026, 3, 20, 12, 0, 0, DateTimeKind.Local);

        // Act
        var transaction = Transaction.Create(
            "ACC-001",
            TransactionKind.Credit,
            150.50m,
            "BRL",
            transactionDate,
            "Initial deposit",
            "corr-123");

        // Assert
        transaction.TransactionId.Should().NotBeEmpty();
        transaction.Status.Should().Be(TransactionStatus.Received);

        transaction.TransactionDateUtc.Kind.Should().Be(DateTimeKind.Utc);

        transaction.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Verifies that calling <see cref="Transaction.MarkAsPublished"/> updates
    /// the status and sets the update timestamp.
    /// </summary>
    [Fact]
    public void MarkAsPublished_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var transaction = Transaction.Create(
            "ACC-001",
            TransactionKind.Credit,
            100m,
            "BRL",
            DateTime.UtcNow,
            null,
            "corr-123");

        // Act
        transaction.MarkAsPublished();

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Published);
        transaction.UpdatedAtUtc.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that calling <see cref="Transaction.MarkAsFailedToPublish"/> updates
    /// the status and sets the update timestamp.
    /// </summary>
    [Fact]
    public void MarkAsFailedToPublish_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var transaction = Transaction.Create(
            "ACC-001",
            TransactionKind.Debit,
            50m,
            "BRL",
            DateTime.UtcNow,
            null,
            "corr-123");

        // Act
        transaction.MarkAsFailedToPublish();

        // Assert
        transaction.Status.Should().Be(TransactionStatus.FailedToPublish);
        transaction.UpdatedAtUtc.Should().NotBeNull();
    }
}
