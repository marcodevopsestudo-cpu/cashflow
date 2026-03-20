using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TransactionService.Application.Abstractions.Idempotency;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Application.Common.Exceptions;
using TransactionService.Application.Transactions.Commands.CreateTransaction;
using TransactionService.Domain.Entities;
using Xunit;

namespace TransactionService.UnitTests;

/// <summary>
/// Contains unit tests for the transaction creation handler.
/// </summary>
public sealed class CreateTransactionCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldPersistTransactionOutboxAndIdempotency_OnFirstExecution()
    {
        var repository = new Mock<ITransactionRepository>();
        var outboxRepository = new Mock<IOutboxRepository>();
        var idempotencyRepository = new Mock<IIdempotencyRepository>();
        var requestHashService = new Mock<IRequestHashService>();
        var unitOfWork = new Mock<IUnitOfWork>();

        requestHashService
            .Setup(x => x.ComputeHash(It.IsAny<CreateTransactionCommand>()))
            .Returns("hash-1");

        idempotencyRepository
            .Setup(x => x.GetByKeyAsync("idem-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyEntry?)null);

        var handler = new CreateTransactionCommandHandler(
            repository.Object,
            outboxRepository.Object,
            idempotencyRepository.Object,
            requestHashService.Object,
            unitOfWork.Object,
            NullLogger<CreateTransactionCommandHandler>.Instance);

        var result = await handler.Handle(new CreateTransactionCommand(
            "ACC-001",
            "Credit",
            100,
            "BRL",
            DateTime.UtcNow,
            "Initial deposit",
            "corr-1",
            "idem-1"), CancellationToken.None);

        result.IsIdempotentReplay.Should().BeFalse();
        result.Transaction.AccountId.Should().Be("ACC-001");
        result.Transaction.Status.Should().Be("Received");

        idempotencyRepository.Verify(x => x.AddAsync(It.IsAny<IdempotencyEntry>(), It.IsAny<CancellationToken>()), Times.Once);
        idempotencyRepository.Verify(x => x.UpdateAsync(It.IsAny<IdempotencyEntry>(), It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
        outboxRepository.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnReplay_WhenIdempotencyEntryAlreadyExistsWithSamePayload()
    {
        var repository = new Mock<ITransactionRepository>();
        var outboxRepository = new Mock<IOutboxRepository>();
        var idempotencyRepository = new Mock<IIdempotencyRepository>();
        var requestHashService = new Mock<IRequestHashService>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var persistedTransaction = Transaction.Create(
            "ACC-001",
            Domain.Enums.TransactionKind.Credit,
            100,
            "BRL",
            DateTime.UtcNow,
            "Initial deposit",
            "corr-original");

        var entry = IdempotencyEntry.Create("idem-1", "hash-1");
        entry.Complete(persistedTransaction.Id);

        requestHashService
            .Setup(x => x.ComputeHash(It.IsAny<CreateTransactionCommand>()))
            .Returns("hash-1");

        idempotencyRepository
            .Setup(x => x.GetByKeyAsync("idem-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        repository
            .Setup(x => x.GetByIdAsync(persistedTransaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistedTransaction);

        var handler = new CreateTransactionCommandHandler(
            repository.Object,
            outboxRepository.Object,
            idempotencyRepository.Object,
            requestHashService.Object,
            unitOfWork.Object,
            NullLogger<CreateTransactionCommandHandler>.Instance);

        var result = await handler.Handle(new CreateTransactionCommand(
            "ACC-001",
            "Credit",
            100,
            "BRL",
            DateTime.UtcNow,
            "Initial deposit",
            "corr-retry",
            "idem-1"), CancellationToken.None);

        result.IsIdempotentReplay.Should().BeTrue();
        result.Transaction.TransactionId.Should().Be(persistedTransaction.Id);
        result.Transaction.CorrelationId.Should().Be("corr-original");

        repository.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
        outboxRepository.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenIdempotencyKeyIsReusedWithDifferentPayload()
    {
        var repository = new Mock<ITransactionRepository>();
        var outboxRepository = new Mock<IOutboxRepository>();
        var idempotencyRepository = new Mock<IIdempotencyRepository>();
        var requestHashService = new Mock<IRequestHashService>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var entry = IdempotencyEntry.Create("idem-1", "hash-original");
        entry.Complete(Guid.NewGuid());

        requestHashService
            .Setup(x => x.ComputeHash(It.IsAny<CreateTransactionCommand>()))
            .Returns("hash-different");

        idempotencyRepository
            .Setup(x => x.GetByKeyAsync("idem-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        var handler = new CreateTransactionCommandHandler(
            repository.Object,
            outboxRepository.Object,
            idempotencyRepository.Object,
            requestHashService.Object,
            unitOfWork.Object,
            NullLogger<CreateTransactionCommandHandler>.Instance);

        var act = async () => await handler.Handle(new CreateTransactionCommand(
            "ACC-001",
            "Credit",
            100,
            "BRL",
            DateTime.UtcNow,
            "Initial deposit",
            "corr-1",
            "idem-1"), CancellationToken.None);

        await act.Should().ThrowAsync<IdempotencyConflictApplicationException>();
    }
}
