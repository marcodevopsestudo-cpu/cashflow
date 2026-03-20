using FluentAssertions;
using TransactionService.Domain.Entities;
using Xunit;

namespace TransactionService.UnitTests.Domain;

public sealed class IdempotencyEntryTests
{
    [Fact]
    public void Complete_ShouldStoreTransactionId()
    {
        var entry = IdempotencyEntry.Create("idem-1", "hash-1");
        var transactionId = Guid.NewGuid();

        entry.Complete(transactionId);

        entry.IsCompleted.Should().BeTrue();
        entry.TransactionId.Should().Be(transactionId);
    }

    [Fact]
    public void Complete_ShouldThrow_WhenTransactionIdIsEmpty()
    {
        var entry = IdempotencyEntry.Create("idem-1", "hash-1");

        var act = () => entry.Complete(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }
}
