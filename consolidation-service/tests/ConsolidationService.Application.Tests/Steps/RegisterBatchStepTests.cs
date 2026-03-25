using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Contracts;
using ConsolidationService.Application.Models;
using ConsolidationService.Application.Steps;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;
using ConsolidationService.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace ConsolidationService.Application.Tests.Steps;

/// <summary>
/// Unit tests for <see cref="RegisterBatchStep"/>.
/// </summary>
public sealed class RegisterBatchStepTests
{
    /// <summary>
    /// Ensures that processing is interrupted when the batch has already been completed.
    /// </summary>
    [Fact]
    public async Task Should_throw_when_batch_has_already_succeeded()
    {
        // Arrange
        var repository = Substitute.For<IDailyBatchRepository>();

        repository.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new DailyBatch
            {
                BatchId = Guid.NewGuid(),
                Status = BatchStatus.Succeeded
            });

        var step = new RegisterBatchStep(
            repository,
            NullLogger<RegisterBatchStep>.Instance);

        var context = new BatchExecutionContext
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = Guid.NewGuid(),
                CorrelationId = "corr-006",
                PublishedAtUtc = DateTime.UtcNow,
                TransactionIds = new[] { Guid.NewGuid() }
            }
        };

        // Act
        var action = async () => await step.ExecuteAsync(context, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<BatchAlreadyProcessedException>();
    }
}
