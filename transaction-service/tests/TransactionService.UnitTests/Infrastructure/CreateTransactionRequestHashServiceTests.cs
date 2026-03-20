using FluentAssertions;
using TransactionService.Application.Transactions.Commands.CreateTransaction;
using TransactionService.Infrastructure.Idempotency;
using Xunit;

namespace TransactionService.UnitTests.Infrastructure;

public sealed class CreateTransactionRequestHashServiceTests
{
    private readonly CreateTransactionRequestHashService _sut = new();

    [Fact]
    public void ComputeHash_ShouldReturnSameHash_ForSameLogicalPayload()
    {
        var transactionDate = new DateTime(2026, 3, 20, 10, 0, 0, DateTimeKind.Utc);
        var first = new CreateTransactionCommand("ACC-001", "Credit", 100m, "BRL", transactionDate, "deposit", "corr-1", "idem-1");
        var second = new CreateTransactionCommand("ACC-001", "Credit", 100m, "BRL", transactionDate, "deposit", "corr-2", "idem-2");

        var firstHash = _sut.ComputeHash(first);
        var secondHash = _sut.ComputeHash(second);

        firstHash.Should().Be(secondHash);
    }

    [Fact]
    public void ComputeHash_ShouldReturnDifferentHash_WhenBusinessPayloadChanges()
    {
        var transactionDate = new DateTime(2026, 3, 20, 10, 0, 0, DateTimeKind.Utc);
        var first = new CreateTransactionCommand("ACC-001", "Credit", 100m, "BRL", transactionDate, "deposit", "corr-1", "idem-1");
        var second = new CreateTransactionCommand("ACC-001", "Credit", 200m, "BRL", transactionDate, "deposit", "corr-1", "idem-1");

        var firstHash = _sut.ComputeHash(first);
        var secondHash = _sut.ComputeHash(second);

        firstHash.Should().NotBe(secondHash);
    }
}
