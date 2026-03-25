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
    /// Ensures validation passes when the command contains valid data.
    /// </summary>
    [Fact]
    public void Should_pass_validation_when_message_is_valid()
    {
        var command = CreateValidCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Ensures validation fails when the transaction list is empty.
    /// </summary>
    [Fact]
    public void Should_fail_when_transaction_list_is_empty()
    {
        var command = CreateValidCommand() with
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = Guid.NewGuid(),
                CorrelationId = "corr-001",
                PublishedAtUtc = DateTime.UtcNow,
                TransactionIds = Array.Empty<Guid>()
            }
        };

        var action = () => _validator.ValidateAndThrow(command);

        action.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Ensures validation fails when any transaction identifier is empty.
    /// </summary>
    [Fact]
    public void Should_fail_when_any_transaction_id_is_empty()
    {
        var command = CreateValidCommand() with
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = Guid.NewGuid(),
                CorrelationId = "corr-001",
                PublishedAtUtc = DateTime.UtcNow,
                TransactionIds = [Guid.NewGuid(), Guid.Empty]
            }
        };

        var action = () => _validator.ValidateAndThrow(command);

        action.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Creates a valid command instance for testing purposes.
    /// </summary>
    /// <returns>A valid <see cref="ProcessConsolidationBatchCommand"/>.</returns>
    private static ProcessConsolidationBatchCommand CreateValidCommand()
        => new(
            new ConsolidationBatchMessage
            {
                BatchId = Guid.NewGuid(),
                CorrelationId = "corr-001",
                PublishedAtUtc = DateTime.UtcNow,
                TransactionIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]
            },
            "msg-001");
}
