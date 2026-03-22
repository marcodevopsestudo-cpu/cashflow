using FluentAssertions;
using TransactionService.Domain.Entities;
using Xunit;

namespace TransactionService.UnitTests.Domain;

/// <summary>
/// Unit tests for <see cref="IdempotencyEntry"/>.
/// </summary>
public sealed class IdempotencyEntryTests
{
    /// <summary>
    /// Verifies that calling <see cref="IdempotencyEntry.Complete"/> stores
    /// the transaction identifier and marks the entry as completed.
    /// </summary>
    [Fact]
    public void Complete_ShouldStoreTransactionId()
    {
        // Arrange
        var entry = IdempotencyEntry.Create("idem-1", "hash-1");
        var transactionId = Guid.NewGuid();

        // Act
        entry.Complete(transactionId);

        // Assert
        entry.IsCompleted.Should().BeTrue();
        entry.TransactionId.Should().Be(transactionId);
    }

    /// <summary>
    /// Verifies that calling <see cref="IdempotencyEntry.Complete"/> with an empty
    /// transaction identifier throws an exception.
    /// </summary>
    [Fact]
    public void Complete_ShouldThrow_WhenTransactionIdIsEmpty()
    {
        // Arrange
        var entry = IdempotencyEntry.Create("idem-1", "hash-1");

        // Act
        var act = () => entry.Complete(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*TransactionId*");
    }
}
