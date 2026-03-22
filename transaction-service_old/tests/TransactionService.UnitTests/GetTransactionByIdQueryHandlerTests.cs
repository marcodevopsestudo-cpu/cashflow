using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Application.Common.Exceptions;
using TransactionService.Application.Transactions.Queries.GetTransactionById;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using Xunit;

namespace TransactionService.UnitTests;

/// <summary>
/// Unit tests for <see cref="GetTransactionByIdQueryHandler"/>.
/// </summary>
public sealed class GetTransactionByIdQueryHandlerTests
{
    /// <summary>
    /// Verifies that the handler returns a transaction DTO when the transaction exists.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnTransactionDto_WhenTransactionExists()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();

        var transactionId = Guid.NewGuid();
        var correlationId = Guid.NewGuid().ToString("N");

        var transaction = Transaction.Create(
            "ACC-001",
            TransactionKind.Credit,
            150.75m,
            "BRL",
            DateTime.UtcNow,
            "Initial deposit",
            correlationId);

        repositoryMock
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var handler = new GetTransactionByIdQueryHandler(
            repositoryMock.Object,
            NullLogger<GetTransactionByIdQueryHandler>.Instance);

        var query = new GetTransactionByIdQuery(transactionId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(transaction.AccountId);
        result.Amount.Should().Be(transaction.Amount);
        result.Currency.Should().Be(transaction.Currency);
        result.Status.Should().Be(transaction.Status.ToString());

        repositoryMock.Verify(
            x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that the handler throws <see cref="EntityNotFoundApplicationException"/>
    /// when the transaction does not exist.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundApplicationException_WhenTransactionDoesNotExist()
    {
        // Arrange
        var repositoryMock = new Mock<ITransactionRepository>();
        var transactionId = Guid.NewGuid();

        repositoryMock
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);

        var handler = new GetTransactionByIdQueryHandler(
            repositoryMock.Object,
            NullLogger<GetTransactionByIdQueryHandler>.Instance);

        var query = new GetTransactionByIdQuery(transactionId);

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundApplicationException>();

        repositoryMock.Verify(
            x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
