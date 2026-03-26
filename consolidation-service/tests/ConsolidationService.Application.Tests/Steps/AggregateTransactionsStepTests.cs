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
                    TransactionId = transactionId1,
                    Amount = 100m,
                    Kind = TransactionKind.Credit,
                    TransactionDateUtc = new DateTime(2026, 3, 22, 10, 0, 0, DateTimeKind.Utc)
                },
                new Transaction
                {
                    TransactionId = transactionId2,
                    Amount = 25m,
                    Kind = TransactionKind.Debit,
                    TransactionDateUtc = new DateTime(2026, 3, 22, 11, 0, 0, DateTimeKind.Utc)
                },
                new Transaction
                {
                    TransactionId = transactionId3,
                    Amount = 50m,
                    Kind = TransactionKind.Credit,
                    TransactionDateUtc = new DateTime(2026, 3, 23, 9, 0, 0, DateTimeKind.Utc)
                }
            ]
        };

        // Act
        await step.ExecuteAsync(context, CancellationToken.None);

        // Assert
        context.Aggregates.Should().HaveCount(2);

        context.Aggregates.Should().Contain(x =>
            x.BalanceDate == new DateOnly(2026, 3, 22) &&
            x.TotalCredits == 100m &&
            x.TotalDebits == 25m &&
            x.Balance == 75m);

        context.Aggregates.Should().Contain(x =>
            x.BalanceDate == new DateOnly(2026, 3, 23) &&
            x.TotalCredits == 50m &&
            x.TotalDebits == 0m &&
            x.Balance == 50m);
    }
}
