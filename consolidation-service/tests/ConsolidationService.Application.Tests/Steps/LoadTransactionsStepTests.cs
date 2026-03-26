using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Contracts;
using ConsolidationService.Application.Models;
using ConsolidationService.Application.Steps;
using ConsolidationService.Domain.Entities;
using ConsolidationService.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ConsolidationService.Tests.Application.Steps;

/// <summary>
/// Contains unit tests for <see cref="LoadTransactionsStep"/>.
/// </summary>
public class LoadTransactionsStepTests
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<LoadTransactionsStep> _logger;
    private readonly LoadTransactionsStep _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadTransactionsStepTests"/> class.
    /// </summary>
    public LoadTransactionsStepTests()
    {
        _transactionRepository = Substitute.For<ITransactionRepository>();
        _logger = Substitute.For<ILogger<LoadTransactionsStep>>();

        _sut = new LoadTransactionsStep(
            _transactionRepository,
            _logger);
    }

    /// <summary>
    /// Verifies that published transactions are loaded into the execution context.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldLoadPublishedTransactionsIntoContext()
    {
        // Arrange
        var transactionIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var batchId = Guid.NewGuid();

        IReadOnlyCollection<Transaction> transactions = new List<Transaction>
        {
            new Transaction { TransactionId = transactionIds[0] },
            new Transaction { TransactionId = transactionIds[1] }
        };

        var context = new BatchExecutionContext
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = batchId,
                TransactionIds = transactionIds
            }
        };

        _transactionRepository
            .GetPublishedByIdsAsync(transactionIds, Arg.Any<CancellationToken>())
            .Returns(transactions);

        // Act
        await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        context.Transactions.Should().NotBeNull();
        context.Transactions.Should().HaveCount(2);
        context.Transactions.Select(x => x.TransactionId)
            .Should()
            .BeEquivalentTo(transactionIds);

        await _transactionRepository.Received(1).GetPublishedByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(transactionIds)),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that an <see cref="InvalidBatchException"/> is thrown when no transactions are found.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidBatchException_WhenNoTransactionsAreFound()
    {
        // Arrange
        var transactionIds = new[] { Guid.NewGuid() };
        var batchId = Guid.NewGuid();

        var context = new BatchExecutionContext
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = batchId,
                TransactionIds = transactionIds
            }
        };

        _transactionRepository
            .GetPublishedByIdsAsync(transactionIds, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Transaction>());

        // Act
        var act = async () => await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidBatchException>();

        context.Transactions.Should().BeEmpty();

        await _transactionRepository.Received(1).GetPublishedByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(transactionIds)),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that the repository is called with the transaction identifiers provided in the batch message.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryWithTransactionIdsFromMessage()
    {
        // Arrange
        var transactionIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var batchId = Guid.NewGuid();

        IReadOnlyCollection<Transaction> transactions = new List<Transaction>
        {
            new Transaction { TransactionId = transactionIds[0] }
        };

        var context = new BatchExecutionContext
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = batchId,
                TransactionIds = transactionIds
            }
        };

        _transactionRepository
            .GetPublishedByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        // Act
        await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        await _transactionRepository.Received(1).GetPublishedByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids =>
                ids.Count == transactionIds.Length &&
                transactionIds.All(ids.Contains)),
            Arg.Any<CancellationToken>());
    }
}
