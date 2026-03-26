using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Contracts;
using ConsolidationService.Application.Models;
using ConsolidationService.Application.Steps;
using ConsolidationService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ConsolidationService.Tests.Application.Steps;

/// <summary>
/// Contains unit tests for <see cref="UpsertDailyBalanceStep"/>.
/// </summary>
public class UpsertDailyBalanceStepTests
{
    private readonly IDailyBalanceRepository _dailyBalanceRepository;
    private readonly ILogger<UpsertDailyBalanceStep> _logger;
    private readonly UpsertDailyBalanceStep _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpsertDailyBalanceStepTests"/> class.
    /// </summary>
    public UpsertDailyBalanceStepTests()
    {
        _dailyBalanceRepository = Substitute.For<IDailyBalanceRepository>();
        _logger = Substitute.For<ILogger<UpsertDailyBalanceStep>>();

        _sut = new UpsertDailyBalanceStep(
            _dailyBalanceRepository,
            _logger);
    }

    /// <summary>
    /// Verifies that the step calls the repository to upsert
    /// the aggregates provided in the execution context.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallUpsertAsync_WithAggregatesFromContext()
    {
        // Arrange
        var batchId = Guid.NewGuid();

        var aggregates = new List<DailyAggregate>
        {
           new(DateOnly.FromDateTime(DateTime.Now),10,10),
           new(DateOnly.FromDateTime(DateTime.Now),10,10),
        };
        var context = new BatchExecutionContext
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = batchId
            },
            Aggregates = aggregates
        };

        // Act
        await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        await _dailyBalanceRepository.Received(1).UpsertAsync(
            Arg.Is<IReadOnlyCollection<DailyAggregate>>(x => x.Count == 2),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that the step forwards the same aggregate collection instance
    /// from the execution context to the repository.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallUpsertAsync_WithSameAggregateReference()
    {
        // Arrange
        var batchId = Guid.NewGuid();

        var aggregates = new List<DailyAggregate>
        {
           new(DateOnly.FromDateTime(DateTime.Now),10,10),
           new(DateOnly.FromDateTime(DateTime.Now),10,10),
        };

        var context = new BatchExecutionContext
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = batchId
            },
            Aggregates = aggregates
        };

        // Act
        await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        await _dailyBalanceRepository.Received(1).UpsertAsync(
            Arg.Is<IReadOnlyCollection<DailyAggregate>>(x => ReferenceEquals(x, aggregates)),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that the step calls the repository correctly
    /// when the aggregate collection is empty.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallUpsertAsync_WhenAggregatesAreEmpty()
    {
        // Arrange
        var batchId = Guid.NewGuid();

        var aggregates = new List<DailyAggregate>();

        var context = new BatchExecutionContext
        {
            Message = new ConsolidationBatchMessage
            {
                BatchId = batchId
            },
            Aggregates = aggregates
        };

        // Act
        await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        await _dailyBalanceRepository.Received(1).UpsertAsync(
            Arg.Is<IReadOnlyCollection<DailyAggregate>>(x => x.Count == 0),
            Arg.Any<CancellationToken>());
    }
}
