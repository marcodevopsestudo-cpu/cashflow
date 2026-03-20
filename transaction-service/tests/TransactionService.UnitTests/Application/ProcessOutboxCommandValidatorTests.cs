using FluentAssertions;
using TransactionService.Application.Transactions.Commands.ProcessOutBox;
using TransactionService.Application.Transactions.Commands.ProcessOutbox;
using Xunit;

namespace TransactionService.UnitTests.Application.Outbox;

public sealed class ProcessOutboxCommandValidatorTests
{
    private readonly ProcessOutboxCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldFail_WhenBatchSizeIsNotPositive()
    {
        var command = new ProcessOutboxCommand(0, "corr-1");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldPass_WhenBatchSizeIsPositive()
    {
        var command = new ProcessOutboxCommand(50, "corr-1");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
