using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TransactionService.Application.Abstractions.Idempotency;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Application.Common.Exceptions;
using TransactionService.Application.Transactions.Commands.CreateTransaction;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using Xunit;

namespace TransactionService.UnitTests;

/// <summary>
/// Unit tests for <see cref="CreateTransactionCommandHandler"/>.
/// </summary>
public sealed class CreateTransactionCommandHandlerTests
{
    /// <summary>
    /// Verifies that on first execution the handler persists:
    /// - transaction
    /// - outbox message
    /// - idempotency entry
    /// and commits the unit of work.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPersistTransactionOutboxAndIdempotency_OnFirstExecution()
    {
        // Arrange
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

        // Act
        var result = await handler.Handle(new CreateTransactionCommand(
            "ACC-001",
            "Credit",
            100,
            "BRL",
            DateTime.UtcNow,
            "Initial deposit",
            "corr-1",
            "idem-1"), CancellationToken.None);

        // Assert
        result.IsIdempotentReplay.Should().BeFalse();
        result.Transaction.AccountId.Should().Be("ACC-001");
        result.Transaction.Status.Should().Be("Received");

        idempotencyRepository.Verify(x =>
            x.AddAsync(It.IsAny<IdempotencyEntry>(), It.IsAny<CancellationToken>()), Times.Once);

        idempotencyRepository.Verify(x =>
            x.UpdateAsync(It.IsAny<IdempotencyEntry>(), It.IsAny<CancellationToken>()), Times.Never);

        repository.Verify(x =>
            x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);

        outboxRepository.Verify(x =>
            x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        unitOfWork.Verify(x =>
            x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that when the same idempotency key and payload are used,
    /// the handler returns a replay instead of creating new data.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnReplay_WhenIdempotencyEntryAlreadyExistsWithSamePayload()
    {
        // Arrange
        var repository = new Mock<ITransactionRepository>();
        var outboxRepository = new Mock<IOutboxRepository>();
        var idempotencyRepository = new Mock<IIdempotencyRepository>();
        var requestHashService = new Mock<IRequestHashService>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var persistedTransaction = Transaction.Create(
            "ACC-001",
            TransactionKind.Credit,
            100,
            "BRL",
            DateTime.UtcNow,
            "Initial deposit",
            "corr-original");

        var entry = IdempotencyEntry.Create("idem-1", "hash-1");
        entry.Complete(persistedTransaction.TransactionId);

        requestHashService
            .Setup(x => x.ComputeHash(It.IsAny<CreateTransactionCommand>()))
            .Returns("hash-1");

        idempotencyRepository
            .Setup(x => x.GetByKeyAsync("idem-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        repository
            .Setup(x => x.GetByIdAsync(persistedTransaction.TransactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistedTransaction);

        var handler = new CreateTransactionCommandHandler(
            repository.Object,
            outboxRepository.Object,
            idempotencyRepository.Object,
            requestHashService.Object,
            unitOfWork.Object,
            NullLogger<CreateTransactionCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(new CreateTransactionCommand(
            "ACC-001",
            "Credit",
            100,
            "BRL",
            DateTime.UtcNow,
            "Initial deposit",
            "corr-retry",
            "idem-1"), CancellationToken.None);

        // Assert
        result.IsIdempotentReplay.Should().BeTrue();
        result.Transaction.Id.Should().Be(persistedTransaction.TransactionId);
        result.Transaction.CorrelationId.Should().Be("corr-original");

        repository.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
        outboxRepository.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        idempotencyRepository.Verify(x => x.UpdateAsync(It.IsAny<IdempotencyEntry>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that reusing an idempotency key with a different payload
    /// results in a conflict exception.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenIdempotencyKeyIsReusedWithDifferentPayload()
    {
        // Arrange
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

        // Act
        var act = async () => await handler.Handle(new CreateTransactionCommand(
            "ACC-001",
            "Credit",
            100,
            "BRL",
            DateTime.UtcNow,
            "Initial deposit",
            "corr-1",
            "idem-1"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<IdempotencyConflictApplicationException>();
    }
}
