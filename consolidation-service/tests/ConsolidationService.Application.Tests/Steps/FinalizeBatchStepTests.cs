using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Contracts;
using ConsolidationService.Application.Models;
using ConsolidationService.Application.Steps;
using ConsolidationService.Domain.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ConsolidationService.Tests.Application.Steps;

/// <summary>
/// Contains unit tests for <see cref="FinalizeBatchStep"/>.
/// </summary>
public class FinalizeBatchStepTests
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IDailyBatchRepository _dailyBatchRepository;
    private readonly ILogger<FinalizeBatchStep> _logger;
    private readonly FinalizeBatchStep _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="FinalizeBatchStepTests"/> class.
    /// </summary>
    public FinalizeBatchStepTests()
    {
        _transactionRepository = Substitute.For<ITransactionRepository>();
        _dailyBatchRepository = Substitute.For<IDailyBatchRepository>();
        _logger = Substitute.For<ILogger<FinalizeBatchStep>>();

        _sut = new FinalizeBatchStep(
            _transactionRepository,
            _dailyBatchRepository,
            _logger);
    }

    /// <summary>
    /// Verifies that the step marks all transactions as consolidated
    /// and marks the batch as succeeded.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldMarkTransactionsAsConsolidated_AndBatchAsSucceeded()
    {
        // Arrange
        var batchId = Guid.NewGuid();

        var transactions = new[]
        {
            new Transaction { TransactionId = Guid.NewGuid() },
            new Transaction { TransactionId = Guid.NewGuid() }
        };

        var context = new BatchExecutionContext
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = batchId,
                TransactionIds = transactions.Select(x => x.TransactionId).ToArray()
            },
            Transactions = transactions.ToList()
        };

        // Act
        await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        await _transactionRepository.Received(1).MarkAsConsolidatedAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids =>
                ids.Count == transactions.Length &&
                transactions.All(t => ids.Contains(t.TransactionId))),
            batchId,
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());

        await _dailyBatchRepository.Received(1).MarkAsSucceededAsync(
            batchId,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that the step passes the correct batch identifier
    /// to the persistence operations.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoriesWithCorrectBatchId()
    {
        // Arrange
        var batchId = Guid.NewGuid();

        var transactions = new[]
        {
            new Transaction { TransactionId = Guid.NewGuid() }
        };

        var context = new BatchExecutionContext
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = batchId,
                TransactionIds = transactions.Select(x => x.TransactionId).ToArray()
            },
            Transactions = transactions.ToList()
        };

        // Act
        await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        await _transactionRepository.Received(1).MarkAsConsolidatedAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            batchId,
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());

        await _dailyBatchRepository.Received(1).MarkAsSucceededAsync(
            batchId,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that the step handles an empty transaction list
    /// while still finalizing the batch successfully.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldHandleEmptyTransactionList()
    {
        // Arrange
        var batchId = Guid.NewGuid();

        var context = new BatchExecutionContext
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = batchId,
                TransactionIds = Array.Empty<Guid>()
            },
            Transactions = new System.Collections.Generic.List<Transaction>()
        };

        // Act
        await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        await _transactionRepository.Received(1).MarkAsConsolidatedAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 0),
            batchId,
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());

        await _dailyBatchRepository.Received(1).MarkAsSucceededAsync(
            batchId,
            Arg.Any<CancellationToken>());
    }
}
