using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Application.Common.Exceptions;
using TransactionService.Application.Transactions.Queries.GetTransactionById;
using TransactionService.Domain.Entities;
using Xunit;

namespace TransactionService.UnitTests.Application.Transactions.Queries;

/// <summary>
/// Contains unit tests for the transaction retrieval query handler.
/// </summary>
public sealed class GetTransactionByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnTransactionDto_WhenTransactionExists()
    {
        var repositoryMock = new Mock<ITransactionRepository>();

        var transactionId = Guid.NewGuid();
        var correlationId = Guid.NewGuid().ToString("N");

        var transaction = Transaction.Create(
            "ACC-001",
            Domain.Enums.TransactionKind.Credit,
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

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccountId.Should().Be(transaction.AccountId);
        result.Amount.Should().Be(transaction.Amount);
        result.Currency.Should().Be(transaction.Currency);
        result.Status.Should().Be(transaction.Status.ToString());

        repositoryMock.Verify(
            x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundApplicationException_WhenTransactionDoesNotExist()
    {
        var repositoryMock = new Mock<ITransactionRepository>();
        var transactionId = Guid.NewGuid();

        repositoryMock
            .Setup(x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);

        var handler = new GetTransactionByIdQueryHandler(
            repositoryMock.Object,
            NullLogger<GetTransactionByIdQueryHandler>.Instance);

        var query = new GetTransactionByIdQuery(transactionId);
        var act = async () => await handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundApplicationException>();

        repositoryMock.Verify(
            x => x.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
