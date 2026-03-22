using ConsolidationService.Application.Contracts;
using ConsolidationService.Application.Models;
using ConsolidationService.Application.Steps;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ConsolidationService.Application.Tests.Steps;

public sealed class AggregateTransactionsStepTests
{
    [Fact]
    public async Task Should_group_transactions_by_day_and_type()
    {
        var step = new AggregateTransactionsStep(NullLogger<AggregateTransactionsStep>.Instance);
        var context = new BatchExecutionContext
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = Guid.NewGuid(),
                CorrelationId = "corr-005",
                PublishedAtUtc = DateTime.UtcNow,
                TransactionIds = new[] { 1L, 2L, 3L }
            },
            Transactions = new[]
            {
                new Transaction { Id = 1, Amount = 100m, Type = TransactionType.Credit, OccurredAtUtc = new DateTime(2026, 3, 22, 10, 0, 0, DateTimeKind.Utc) },
                new Transaction { Id = 2, Amount = 25m, Type = TransactionType.Debit, OccurredAtUtc = new DateTime(2026, 3, 22, 11, 0, 0, DateTimeKind.Utc) },
                new Transaction { Id = 3, Amount = 50m, Type = TransactionType.Credit, OccurredAtUtc = new DateTime(2026, 3, 23, 9, 0, 0, DateTimeKind.Utc) }
            }
        };

        await step.ExecuteAsync(context, CancellationToken.None);

        context.Aggregates.Should().HaveCount(2);
        context.Aggregates.First().TotalCredits.Should().Be(100m);
        context.Aggregates.First().TotalDebits.Should().Be(25m);
        context.Aggregates.Last().TotalCredits.Should().Be(50m);
    }
}
