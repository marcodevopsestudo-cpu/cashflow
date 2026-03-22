using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Contracts;
using ConsolidationService.Application.Orchestration;
using ConsolidationService.Application.Steps;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;
using ConsolidationService.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace ConsolidationService.Application.Tests.Orchestration;

/// <summary>
/// Unit tests for <see cref="ConsolidationWorkflow"/>.
/// </summary>
public sealed class ConsolidationWorkflowTests
{
    /// <summary>
    /// Ensures that all workflow steps are executed when the batch is valid.
    /// </summary>
    [Fact]
    public async Task Should_execute_all_registered_steps_when_batch_is_valid()
    {
        // Arrange
        var batchRepository = Substitute.For<IDailyBatchRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var dailyBalanceRepository = Substitute.For<IDailyBalanceRepository>();
        var errorRepository = Substitute.For<ITransactionProcessingErrorRepository>();
        var retryPolicy = Substitute.For<IRetryPolicy>();

        retryPolicy.ExecuteAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task>>().Invoke(CancellationToken.None));

        batchRepository.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((DailyBatch?)null);

        batchRepository.UpsertPendingAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(call => new DailyBatch
            {
                BatchId = call.Arg<Guid>(),
                CorrelationId = call.ArgAt<string>(1),
                Status = BatchStatus.Pending,
                TransactionCount = call.ArgAt<int>(2)
            });

        transactionRepository.GetPendingByIdsAsync(Arg.Any<IReadOnlyCollection<long>>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                new Transaction { Id = 1, Amount = 100m, Type = TransactionType.Credit, OccurredAtUtc = new DateTime(2026, 3, 22, 10, 0, 0, DateTimeKind.Utc) },
                new Transaction { Id = 2, Amount = 40m, Type = TransactionType.Debit, OccurredAtUtc = new DateTime(2026, 3, 22, 11, 0, 0, DateTimeKind.Utc) }
            ]);

        var sut = CreateSut(
            batchRepository,
            transactionRepository,
            dailyBalanceRepository,
            errorRepository,
            retryPolicy);

        var message = CreateMessage([1L, 2L]);

        // Act
        await sut.ExecuteAsync(message, CancellationToken.None);

        // Assert
        await batchRepository.Received(1).MarkAsProcessingAsync(message.BatchId, CancellationToken.None);

        await dailyBalanceRepository.Received(1)
            .UpsertAsync(Arg.Is<IReadOnlyCollection<ConsolidationService.Domain.ValueObjects.DailyAggregate>>(x => x.Count == 1), CancellationToken.None);

        await transactionRepository.Received(1)
            .MarkAsConsolidatedAsync(
                Arg.Is<IReadOnlyCollection<long>>(ids => ids.SequenceEqual(message.TransactionIds)),
                message.BatchId,
                Arg.Any<DateTime>(),
                CancellationToken.None);

        await batchRepository.Received(1)
            .MarkAsSucceededAsync(message.BatchId, CancellationToken.None);

        await errorRepository.DidNotReceive()
            .InsertAsync(Arg.Any<IReadOnlyCollection<TransactionProcessingError>>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Ensures that the batch is marked for manual review when processing fails.
    /// </summary>
    [Fact]
    public async Task Should_mark_batch_for_manual_review_when_processing_fails()
    {
        // Arrange
        var batchRepository = Substitute.For<IDailyBatchRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var dailyBalanceRepository = Substitute.For<IDailyBalanceRepository>();
        var errorRepository = Substitute.For<ITransactionProcessingErrorRepository>();
        var retryPolicy = Substitute.For<IRetryPolicy>();

        retryPolicy.ExecuteAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task>>().Invoke(CancellationToken.None));

        batchRepository.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<DailyBatch?>(new Exception("boom")));

        var sut = CreateSut(
            batchRepository,
            transactionRepository,
            dailyBalanceRepository,
            errorRepository,
            retryPolicy);

        var message = CreateMessage([9L, 10L]);

        // Act
        var action = async () => await sut.ExecuteAsync(message, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<BatchProcessingException>();

        await batchRepository.Received(1)
            .MarkAsFailedAsync(message.BatchId, "boom", 1, CancellationToken.None);

        await transactionRepository.Received(1)
            .MarkAsFailedAsync(
                Arg.Is<IReadOnlyCollection<long>>(ids => ids.SequenceEqual(message.TransactionIds)),
                message.BatchId,
                1,
                TransactionProcessingStatus.PendingManualReview,
                CancellationToken.None);

        await errorRepository.Received(1)
            .InsertAsync(
                Arg.Is<IReadOnlyCollection<TransactionProcessingError>>(x => x.Count == 2),
                CancellationToken.None);
    }

    private static ConsolidationWorkflow CreateSut(
        IDailyBatchRepository batchRepository,
        ITransactionRepository transactionRepository,
        IDailyBalanceRepository dailyBalanceRepository,
        ITransactionProcessingErrorRepository errorRepository,
        IRetryPolicy retryPolicy)
    {
        return new ConsolidationWorkflow(
            new RegisterBatchStep(batchRepository, NullLogger<RegisterBatchStep>.Instance),
            new LoadTransactionsStep(transactionRepository, NullLogger<LoadTransactionsStep>.Instance),
            new AggregateTransactionsStep(NullLogger<AggregateTransactionsStep>.Instance),
            new UpsertDailyBalanceStep(dailyBalanceRepository, NullLogger<UpsertDailyBalanceStep>.Instance),
            new FinalizeBatchStep(transactionRepository, batchRepository, NullLogger<FinalizeBatchStep>.Instance),
            batchRepository,
            transactionRepository,
            errorRepository,
            retryPolicy,
            NullLogger<ConsolidationWorkflow>.Instance);
    }

    private static ConsolidationBatchMessage CreateMessage(IReadOnlyCollection<long> transactionIds)
        => new()
        {
            BatchId = Guid.NewGuid(),
            CorrelationId = "corr-test",
            PublishedAtUtc = DateTime.UtcNow,
            TransactionIds = transactionIds
        };
}
