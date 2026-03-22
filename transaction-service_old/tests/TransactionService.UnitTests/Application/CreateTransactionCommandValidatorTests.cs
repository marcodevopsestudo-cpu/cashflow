using FluentAssertions;
using TransactionService.Application.Transactions.Commands.CreateTransaction;
using Xunit;

namespace TransactionService.UnitTests.Application.Transactions.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTransactionCommandValidator"/>.
/// </summary>
public sealed class CreateTransactionCommandValidatorTests
{
    private readonly CreateTransactionCommandValidator _sut = new();

    /// <summary>
    /// Verifies that validation fails when required fields are missing or invalid.
    /// </summary>
    [Fact]
    public void Validate_ShouldFail_WhenRequiredFieldsAreMissing()
    {
        // Arrange
        var command = new CreateTransactionCommand(
            string.Empty,
            string.Empty,
            0,
            string.Empty,
            DateTime.UtcNow,
            null,
            string.Empty,
            string.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(6);
    }

    /// <summary>
    /// Verifies that validation succeeds when the command is valid.
    /// </summary>
    [Fact]
    public void Validate_ShouldPass_ForValidCommand()
    {
        // Arrange
        var command = new CreateTransactionCommand(
            "ACC-001",
            "Credit",
            10m,
            "BRL",
            DateTime.UtcNow,
            "desc",
            "corr-1",
            "idem-1");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
