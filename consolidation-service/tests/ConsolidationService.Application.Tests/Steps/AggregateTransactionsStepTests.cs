using ConsolidationService.Application.Contracts;
using ConsolidationService.Application.Models;
using ConsolidationService.Application.Steps;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ConsolidationService.Application.Tests.Steps;

/// <summary>
/// Unit tests for <see cref="AggregateTransactionsStep"/>.
/// </summary>
public sealed class AggregateTransactionsStepTests
{
    /// <summary>
    /// Ensures that transactions are grouped by balance date and aggregated by transaction type.
    /// </summary>
    [Fact]
    public async Task Should_group_transactions_by_day_and_type()
    {
        // Arrange
        var step = new AggregateTransactionsStep(
            NullLogger<AggregateTransactionsStep>.Instance);

        var transactionId1 = Guid.NewGuid();
        var transactionId2 = Guid.NewGuid();
        var transactionId3 = Guid.NewGuid();

        var context = new BatchExecutionContext
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = Guid.NewGuid(),
                CorrelationId = "corr-005",
                PublishedAtUtc = DateTime.UtcNow,
                TransactionIds = [transactionId1, transactionId2, transactionId3]
            },
            Transactions =
            [
                new Transaction
                {
                    Id = transactionId1,
                    Amount = 100m,
                    Type = TransactionType.Credit,
                    OccurredAtUtc = new DateTime(2026, 3, 22, 10, 0, 0, DateTimeKind.Utc)
                },
                new Transaction
                {
                    Id = transactionId2,
                    Amount = 25m,
                    Type = TransactionType.Debit,
                    OccurredAtUtc = new DateTime(2026, 3, 22, 11, 0, 0, DateTimeKind.Utc)
                },
                new Transaction
                {
                    Id = transactionId3,
                    Amount = 50m,
                    Type = TransactionType.Credit,
                    OccurredAtUtc = new DateTime(2026, 3, 23, 9, 0, 0, DateTimeKind.Utc)
                }
            ]
        };

        // Act
        await step.ExecuteAsync(context, CancellationToken.None);

        // Assert
        context.Aggregates.Should().HaveCount(2);
        context.Aggregates.First().TotalCredits.Should().Be(100m);
        context.Aggregates.First().TotalDebits.Should().Be(25m);
        context.Aggregates.Last().TotalCredits.Should().Be(50m);
        context.Aggregates.Last().TotalDebits.Should().Be(0m);
    }
}
