using ConsolidationService.Application.Commands.ProcessConsolidationBatch;
using ConsolidationService.Application.Contracts;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace ConsolidationService.Application.Tests.Commands;

public sealed class ProcessConsolidationBatchCommandValidatorTests
{
    private readonly ProcessConsolidationBatchCommandValidator _validator = new();

    [Fact]
    public void Should_pass_validation_when_message_is_valid()
    {
        var command = new ProcessConsolidationBatchCommand(
            new ConsolidationBatchMessage
            {
                BatchId = Guid.NewGuid(),
                CorrelationId = "corr-001",
                PublishedAtUtc = DateTime.UtcNow,
                TransactionIds = new[] { 1L, 2L, 3L }
            },
            "msg-001");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_fail_when_transaction_list_is_empty()
    {
        var command = new ProcessConsolidationBatchCommand(
            new ConsolidationBatchMessage
            {
                BatchId = Guid.NewGuid(),
                CorrelationId = "corr-001",
                PublishedAtUtc = DateTime.UtcNow,
                TransactionIds = Array.Empty<long>()
            },
            "msg-001");

        var action = () => _validator.ValidateAndThrow(command);

        action.Should().Throw<ValidationException>();
    }
}
