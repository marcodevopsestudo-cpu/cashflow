using FluentAssertions;
using TransactionService.Application.Transactions.Commands.CreateTransaction;
using TransactionService.Infrastructure.Idempotency;
using Xunit;

namespace TransactionService.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for <see cref="CreateTransactionRequestHashService"/>.
/// </summary>
public sealed class CreateTransactionRequestHashServiceTests
{
    private readonly CreateTransactionRequestHashService _sut = new();

    /// <summary>
    /// Verifies that logically equivalent requests produce the same hash,
    /// even when non-business fields (e.g., correlation or idempotency keys) differ.
    /// </summary>
    [Fact]
    public void ComputeHash_ShouldReturnSameHash_ForSameLogicalPayload()
    {
        // Arrange
        var transactionDate = new DateTime(2026, 3, 20, 10, 0, 0, DateTimeKind.Utc);

        var first = new CreateTransactionCommand(
            "ACC-001", "Credit", 100m, "BRL",
            transactionDate, "deposit",
            "corr-1", "idem-1");

        var second = new CreateTransactionCommand(
            "ACC-001", "Credit", 100m, "BRL",
            transactionDate, "deposit",
            "corr-2", "idem-2");

        // Act
        var firstHash = _sut.ComputeHash(first);
        var secondHash = _sut.ComputeHash(second);

        // Assert
        firstHash.Should().Be(secondHash);
    }

    /// <summary>
    /// Verifies that changing business-relevant fields results in a different hash.
    /// </summary>
    [Fact]
    public void ComputeHash_ShouldReturnDifferentHash_WhenBusinessPayloadChanges()
    {
        // Arrange
        var transactionDate = new DateTime(2026, 3, 20, 10, 0, 0, DateTimeKind.Utc);

        var first = new CreateTransactionCommand(
            "ACC-001", "Credit", 100m, "BRL",
            transactionDate, "deposit",
            "corr-1", "idem-1");

        var second = new CreateTransactionCommand(
            "ACC-001", "Credit", 200m, "BRL",
            transactionDate, "deposit",
            "corr-1", "idem-1");

        // Act
        var firstHash = _sut.ComputeHash(first);
        var secondHash = _sut.ComputeHash(second);

        // Assert
        firstHash.Should().NotBe(secondHash);
    }
}
