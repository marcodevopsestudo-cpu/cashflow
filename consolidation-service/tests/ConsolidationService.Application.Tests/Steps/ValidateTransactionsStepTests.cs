using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Contracts;
using ConsolidationService.Application.Models;
using ConsolidationService.Application.Steps;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace ConsolidationService.Application.Tests.Steps;

/// <summary>
/// Unit tests for <see cref="ValidateTransactionsStep"/>.
/// </summary>
public sealed class ValidateTransactionsStepTests
{
    /// <summary>
    /// Ensures that valid transactions are kept in the execution context
    /// and invalid transactions are moved to manual review.
    /// </summary>
    [Fact]
    public async Task Should_keep_valid_transactions_and_move_invalid_ones_to_manual_review()
    {
        var errorRepository = Substitute.For<ITransactionProcessingErrorRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();

        var validTransactionId = Guid.NewGuid();
        var invalidTransactionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();

        var context = new BatchExecutionContext
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = batchId,
                CorrelationId = "corr-validate",
                PublishedAtUtc = DateTime.UtcNow,
                TransactionIds = [validTransactionId, invalidTransactionId]
            },
            Transactions =
            [
                new Transaction
                {
                    Id = validTransactionId,
                    Amount = 100m,
                    Type = TransactionType.Credit,
                    OccurredAtUtc = new DateTime(2026, 3, 22, 10, 0, 0, DateTimeKind.Utc)
                },
                new Transaction
                {
                    Id = invalidTransactionId,
                    Amount = 0m,
                    Type = TransactionType.Debit,
                    OccurredAtUtc = new DateTime(2026, 3, 22, 11, 0, 0, DateTimeKind.Utc)
                }
            ]
        };

        var step = new ValidateTransactionsStep(
            errorRepository,
            transactionRepository,
            NullLogger<ValidateTransactionsStep>.Instance);

        await step.ExecuteAsync(context, CancellationToken.None);

        context.Transactions.Should().HaveCount(1);
        context.Transactions.Single().Id.Should().Be(validTransactionId);

        await errorRepository.Received(1).InsertAsync(
            Arg.Is<IReadOnlyCollection<TransactionProcessingError>>(errors =>
                errors.Count == 1 &&
                errors.Single().TransactionId == invalidTransactionId &&
                errors.Single().BatchId == batchId),
            CancellationToken.None);

        var expectedIds = new[] { invalidTransactionId };

        await transactionRepository.Received(1).MarkAsFailedAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(expectedIds)),
            batchId,
            1,
            TransactionProcessingStatus.PendingManualReview,
            CancellationToken.None);
    }
}
