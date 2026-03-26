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

        var transactionId1 = Guid.NewGuid();
        var transactionId2 = Guid.NewGuid();
        var message = CreateMessage([transactionId1, transactionId2]);

        transactionRepository.GetPendingByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                new Transaction
                {
                    TransactionId = transactionId1,
                    Amount = 100m,
                    Kind = TransactionKind.Credit,
                    UpdatedAtUtc = new DateTime(2026, 3, 22, 10, 0, 0, DateTimeKind.Utc)
                },
                new Transaction
                {
                    TransactionId = transactionId2,
                    Amount = 40m,
                    Kind = TransactionKind.Debit,
                    UpdatedAtUtc = new DateTime(2026, 3, 22, 11, 0, 0, DateTimeKind.Utc)
                }
            ]);

        var sut = CreateSut(
            batchRepository,
            transactionRepository,
            dailyBalanceRepository,
            errorRepository,
            retryPolicy);

        // Act
        await sut.ExecuteAsync(message, CancellationToken.None);

        // Assert
        await batchRepository.Received(1)
            .MarkAsProcessingAsync(message.BatchId, CancellationToken.None);

        await dailyBalanceRepository.Received(1)
            .UpsertAsync(
                Arg.Is<IReadOnlyCollection<ConsolidationService.Domain.ValueObjects.DailyAggregate>>(x => x.Count == 1),
                CancellationToken.None);

        await transactionRepository.Received(1)
            .MarkAsConsolidatedAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(message.TransactionIds)),
                message.BatchId,
                Arg.Any<DateTime>(),
                CancellationToken.None);

        await batchRepository.Received(1)
            .MarkAsSucceededAsync(message.BatchId, CancellationToken.None);

        await errorRepository.DidNotReceive()
            .InsertAsync(
                Arg.Any<IReadOnlyCollection<TransactionProcessingError>>(),
                Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Ensures that only the batch is marked as failed when processing fails before transactions are loaded.
    /// </summary>
    [Fact]
    public async Task Should_mark_only_batch_as_failed_when_processing_fails_before_loading_transactions()
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

        var message = CreateMessage([Guid.NewGuid(), Guid.NewGuid()]);

        var sut = CreateSut(
            batchRepository,
            transactionRepository,
            dailyBalanceRepository,
            errorRepository,
            retryPolicy);

        // Act
        var action = async () => await sut.ExecuteAsync(message, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<BatchProcessingException>();

        await batchRepository.Received(1)
            .MarkAsFailedAsync(message.BatchId, "boom", 1, CancellationToken.None);

        await transactionRepository.DidNotReceive()
            .MarkAsFailedAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(),
                Arg.Any<Guid>(),
                Arg.Any<int>(),
                Arg.Any<TransactionStatus>(),
                CancellationToken.None);

        await errorRepository.DidNotReceive()
            .InsertAsync(
                Arg.Any<IReadOnlyCollection<TransactionProcessingError>>(),
                CancellationToken.None);
    }

    /// <summary>
    /// Creates a fully configured instance of <see cref="ConsolidationWorkflow"/> for testing.
    /// </summary>
    /// <param name="batchRepository">Mocked batch repository.</param>
    /// <param name="transactionRepository">Mocked transaction repository.</param>
    /// <param name="dailyBalanceRepository">Mocked daily balance repository.</param>
    /// <param name="errorRepository">Mocked error repository.</param>
    /// <param name="retryPolicy">Mocked retry policy.</param>
    /// <returns>A configured <see cref="ConsolidationWorkflow"/> instance.</returns>
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
            new ValidateTransactionsStep(
                errorRepository,
                transactionRepository,
                NullLogger<ValidateTransactionsStep>.Instance),
            new AggregateTransactionsStep(NullLogger<AggregateTransactionsStep>.Instance),
            new UpsertDailyBalanceStep(dailyBalanceRepository, NullLogger<UpsertDailyBalanceStep>.Instance),
            new FinalizeBatchStep(transactionRepository, batchRepository, NullLogger<FinalizeBatchStep>.Instance),
            batchRepository,
            transactionRepository,
            errorRepository,
            retryPolicy,
            NullLogger<ConsolidationWorkflow>.Instance);
    }

    /// <summary>
    /// Creates a valid <see cref="ConsolidationBatchMessage"/> for testing.
    /// </summary>
    /// <param name="transactionIds">Transaction identifiers to include in the message.</param>
    /// <returns>A valid batch message.</returns>
    private static ConsolidationBatchMessage CreateMessage(IReadOnlyCollection<Guid> transactionIds)
        => new()
        {
            BatchId = Guid.NewGuid(),
            CorrelationId = "corr-test",
            PublishedAtUtc = DateTime.UtcNow,
            TransactionIds = transactionIds
        };
}
