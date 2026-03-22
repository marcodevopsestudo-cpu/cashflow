using ConsolidationService.Application.Commands.ProcessConsolidationBatch;
using ConsolidationService.Application.Contracts;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace ConsolidationService.Application.Tests.Commands;

/// <summary>
/// Unit tests for <see cref="ProcessConsolidationBatchCommandValidator"/>.
/// </summary>
public sealed class ProcessConsolidationBatchCommandValidatorTests
{
    private readonly ProcessConsolidationBatchCommandValidator _validator = new();

    /// <summary>
    /// Ensures validation succeeds when the command contains valid data.
    /// </summary>
    [Fact]
    public void Should_pass_validation_when_message_is_valid()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Ensures validation fails when the transaction list is empty.
    /// </summary>
    [Fact]
    public void Should_fail_when_transaction_list_is_empty()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = Guid.NewGuid(),
                CorrelationId = "corr-001",
                PublishedAtUtc = DateTime.UtcNow,
                TransactionIds = Array.Empty<long>()
            }
        };

        // Act
        var action = () => _validator.ValidateAndThrow(command);

        // Assert
        action.Should().Throw<ValidationException>();
    }

    private static ProcessConsolidationBatchCommand CreateValidCommand()
        => new(
            new ConsolidationBatchMessage
            {
                BatchId = Guid.NewGuid(),
                CorrelationId = "corr-001",
                PublishedAtUtc = DateTime.UtcNow,
                TransactionIds = new[] { 1L, 2L, 3L }
            },
            "msg-001");
}
