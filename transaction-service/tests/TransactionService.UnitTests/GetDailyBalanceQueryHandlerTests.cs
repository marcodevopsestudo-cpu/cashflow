using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TransactionService.Application.Common.Exceptions;
using TransactionService.Application.Transactions.Queries.GetDailyBalance;
using TransactionService.Domain.Entities;
using Xunit;

namespace TransactionService.UnitTests.Application;

/// <summary>
/// Unit tests for <see cref="GetDailyBalanceQueryHandler"/>.
/// </summary>
public sealed class GetDailyBalanceQueryHandlerTests
{
    /// <summary>
    /// Ensures that a daily balance is returned when it exists in the repository.
    /// </summary>
    [Fact]
    public async Task Should_return_daily_balance_when_exists()
    {
        // Arrange
        var date = new DateOnly(2026, 03, 25);
        var repositoryMock = new Mock<IDailyBalanceRepository>();

        var entity = new DailyBalance
        {
            BalanceDate = date,
            TotalCredits = 100,
            TotalDebits = 40,
            Balance = 60,
            UpdatedAtUtc = DateTime.UtcNow
        };

        repositoryMock
            .Setup(x => x.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var handler = new GetDailyBalanceQueryHandler(
            repositoryMock.Object,
            NullLogger<GetDailyBalanceQueryHandler>.Instance);

        var query = new GetDailyBalanceQuery(date);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.BalanceDate.Should().Be(date);
        result.TotalCredits.Should().Be(100);
        result.TotalDebits.Should().Be(40);
        result.Balance.Should().Be(60);

        repositoryMock.Verify(
            x => x.GetByDateAsync(date, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Ensures that a <see cref="EntityNotFoundApplicationException"/> is thrown
    /// when no daily balance exists for the specified date.
    /// </summary>
    [Fact]
    public async Task Should_throw_not_found_when_daily_balance_does_not_exist()
    {
        // Arrange
        var repositoryMock = new Mock<IDailyBalanceRepository>();
        var date = DateOnly.FromDateTime(DateTime.Now);

        repositoryMock
            .Setup(x => x.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailyBalance?)null);

        var handler = new GetDailyBalanceQueryHandler(
            repositoryMock.Object,
            NullLogger<GetDailyBalanceQueryHandler>.Instance);

        var query = new GetDailyBalanceQuery(date);

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundApplicationException>();

        repositoryMock.Verify(
            x => x.GetByDateAsync(date, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
