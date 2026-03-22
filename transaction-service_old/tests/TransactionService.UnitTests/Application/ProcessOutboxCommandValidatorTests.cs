using FluentAssertions;
using TransactionService.Application.Transactions.Commands.ProcessOutbox;
using TransactionService.Application.Transactions.Commands.ProcessOutBox;
using Xunit;

namespace TransactionService.UnitTests.Application.Outbox;

/// <summary>
/// Unit tests for <see cref="ProcessOutboxCommandValidator"/>.
/// </summary>
public sealed class ProcessOutboxCommandValidatorTests
{
    private readonly ProcessOutboxCommandValidator _sut = new();

    /// <summary>
    /// Verifies that validation fails when the batch size is not a positive value.
    /// </summary>
    [Fact]
    public void Validate_ShouldFail_WhenBatchSizeIsNotPositive()
    {
        // Arrange
        var command = new ProcessOutboxCommand(0, "corr-1");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that validation succeeds when the batch size is a positive value.
    /// </summary>
    [Fact]
    public void Validate_ShouldPass_WhenBatchSizeIsPositive()
    {
        // Arrange
        var command = new ProcessOutboxCommand(50, "corr-1");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
