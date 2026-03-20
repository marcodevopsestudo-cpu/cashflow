using FluentAssertions;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using Xunit;

namespace TransactionService.UnitTests.Domain;

public sealed class TransactionEntityTests
{
    [Fact]
    public void Create_ShouldInitializeTransactionWithReceivedStatus()
    {
        var transactionDate = new DateTime(2026, 3, 20, 12, 0, 0, DateTimeKind.Local);

        var transaction = Transaction.Create(
            "ACC-001",
            TransactionKind.Credit,
            150.50m,
            "BRL",
            transactionDate,
            "Initial deposit",
            "corr-123");

        transaction.Id.Should().NotBeEmpty();
        transaction.Status.Should().Be(TransactionStatus.Received);
        transaction.TransactionDateUtc.Kind.Should().Be(DateTimeKind.Utc);
        transaction.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsPublished_ShouldUpdateStatusAndTimestamp()
    {
        var transaction = Transaction.Create(
            "ACC-001",
            TransactionKind.Credit,
            100m,
            "BRL",
            DateTime.UtcNow,
            null,
            "corr-123");

        transaction.MarkAsPublished();

        transaction.Status.Should().Be(TransactionStatus.Published);
        transaction.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsFailedToPublish_ShouldUpdateStatusAndTimestamp()
    {
        var transaction = Transaction.Create(
            "ACC-001",
            TransactionKind.Debit,
            50m,
            "BRL",
            DateTime.UtcNow,
            null,
            "corr-123");

        transaction.MarkAsFailedToPublish();

        transaction.Status.Should().Be(TransactionStatus.FailedToPublish);
        transaction.UpdatedAtUtc.Should().NotBeNull();
    }
}
