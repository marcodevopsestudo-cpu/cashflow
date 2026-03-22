using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Contracts;
using ConsolidationService.Application.Models;
using ConsolidationService.Application.Orchestration;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Enums;
using ConsolidationService.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace ConsolidationService.Application.Tests.Orchestration;

public sealed class ConsolidationWorkflowTests
{
    [Fact]
    public async Task Should_execute_all_registered_steps_when_batch_is_valid()
    {
        var register = Substitute.For<IConsolidationWorkflowStep>();
        var loadTransaction = Substitute.For<IConsolidationWorkflowStep>();
        var aggregateTransactions = Substitute.For<IConsolidationWorkflowStep>();
        var upsertDailyBalance = Substitute.For<IConsolidationWorkflowStep>();
        var finalizeBatch = Substitute.For<IConsolidationWorkflowStep>();
        var batchRepository = Substitute.For<IDailyBatchRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var errorRepository = Substitute.For<ITransactionProcessingErrorRepository>();
        var retryPolicy = Substitute.For<IRetryPolicy>();

        retryPolicy.ExecuteAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task>>().Invoke(CancellationToken.None));

        var sut = new ConsolidationWorkflow(
            register,
            loadTransaction,
            aggregateTransactions,
            upsertDailyBalance,
            finalizeBatch,
            batchRepository,
            transactionRepository,
            errorRepository,
            retryPolicy,
            NullLogger<ConsolidationWorkflow>.Instance);

        var message = new ConsolidationBatchMessage
        {
            BatchId = Guid.NewGuid(),
            CorrelationId = "corr-003",
            PublishedAtUtc = DateTime.UtcNow,
            TransactionIds = new[] { 1L, 2L }
        };

        await sut.ExecuteAsync(message, "msg-003", CancellationToken.None);

        await register.Received(1).ExecuteAsync(Arg.Any<BatchExecutionContext>(), CancellationToken.None);
        await loadTransaction.Received(1).ExecuteAsync(Arg.Any<BatchExecutionContext>(), CancellationToken.None);
        await aggregateTransactions.Received(1).ExecuteAsync(Arg.Any<BatchExecutionContext>(), CancellationToken.None);
        await upsertDailyBalance.Received(1).ExecuteAsync(Arg.Any<BatchExecutionContext>(), CancellationToken.None);
        await finalizeBatch.Received(1).ExecuteAsync(Arg.Any<BatchExecutionContext>(), CancellationToken.None);

        await errorRepository.DidNotReceive()
            .InsertAsync(Arg.Any<IReadOnlyCollection<TransactionProcessingError>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_mark_batch_for_manual_review_when_processing_fails()
    {
        var register = Substitute.For<IConsolidationWorkflowStep>();
        var loadTransaction = Substitute.For<IConsolidationWorkflowStep>();
        var aggregateTransactions = Substitute.For<IConsolidationWorkflowStep>();
        var upsertDailyBalance = Substitute.For<IConsolidationWorkflowStep>();
        var finalizeBatch = Substitute.For<IConsolidationWorkflowStep>();
        var batchRepository = Substitute.For<IDailyBatchRepository>();
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var errorRepository = Substitute.For<ITransactionProcessingErrorRepository>();
        var retryPolicy = Substitute.For<IRetryPolicy>();

        retryPolicy.ExecuteAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task>>().Invoke(CancellationToken.None));

        // 🔥 força falha no primeiro step
        register
            .ExecuteAsync(Arg.Any<BatchExecutionContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new Exception("boom"));

        var sut = new ConsolidationWorkflow(
            register,
            loadTransaction,
            aggregateTransactions,
            upsertDailyBalance,
            finalizeBatch,
            batchRepository,
            transactionRepository,
            errorRepository,
            retryPolicy,
            NullLogger<ConsolidationWorkflow>.Instance);

        var message = new ConsolidationBatchMessage
        {
            BatchId = Guid.NewGuid(),
            CorrelationId = "corr-004",
            PublishedAtUtc = DateTime.UtcNow,
            TransactionIds = new[] { 9L, 10L }
        };

        var action = async () => await sut.ExecuteAsync(message, "msg-004", CancellationToken.None);

        await action.Should().ThrowAsync<BatchProcessingException>();

        await batchRepository.Received(1)
        .MarkAsFailedAsync(
          message.BatchId,
          "boom",
          1,
          CancellationToken.None);

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
}
