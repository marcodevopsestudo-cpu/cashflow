using FluentAssertions;
using TransactionService.Application.Transactions.Commands.CreateTransaction;
using Xunit;

namespace TransactionService.UnitTests.Application.Transactions.Commands;

public sealed class CreateTransactionCommandValidatorTests
{
    private readonly CreateTransactionCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldFail_WhenRequiredFieldsAreMissing()
    {
        var command = new CreateTransactionCommand(string.Empty, string.Empty, 0, string.Empty, DateTime.UtcNow, null, string.Empty, string.Empty);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(6);
    }

    [Fact]
    public void Validate_ShouldPass_ForValidCommand()
    {
        var command = new CreateTransactionCommand("ACC-001", "Credit", 10m, "BRL", DateTime.UtcNow, "desc", "corr-1", "idem-1");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
