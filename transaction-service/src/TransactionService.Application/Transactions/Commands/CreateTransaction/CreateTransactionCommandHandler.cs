using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.Json;
using TransactionService.Application.Abstractions.Idempotency;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Application.Common.Diagnostics;
using TransactionService.Application.Common.Exceptions;
using TransactionService.Application.Resources;
using TransactionService.Application.Transactions.Common;
using TransactionService.Application.Transactions.IntegrationEvents;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;

namespace TransactionService.Application.Transactions.Commands.CreateTransaction;

/// <summary>
/// Handles transaction creation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateTransactionCommandHandler"/> class.
/// </remarks>
/// <param name="repository">The transaction repository.</param>
/// <param name="outboxRepository">The outbox repository.</param>
/// <param name="idempotencyRepository">idempotency repository.</param>
/// <param name="requestHashService">request hash service</param>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="logger">The logger.</param>
public sealed class CreateTransactionCommandHandler(
    ITransactionRepository repository,
    IOutboxRepository outboxRepository,
    IIdempotencyRepository idempotencyRepository,
    IRequestHashService requestHashService,
    IUnitOfWork unitOfWork,
    ILogger<CreateTransactionCommandHandler> logger) : IRequestHandler<CreateTransactionCommand, CreateTransactionResult>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly ITransactionRepository _repository = repository;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IIdempotencyRepository _idempotencyRepository = idempotencyRepository;
    private readonly IRequestHashService _requestHashService = requestHashService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<CreateTransactionCommandHandler> _logger = logger;

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="request">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created transaction DTO.</returns>
    public async Task<CreateTransactionResult> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var requestHash = _requestHashService.ComputeHash(request);

        _logger.CreateTransactionStarted(request.AccountId, request.CorrelationId, request.IdempotencyKey);

        var existing = await _idempotencyRepository.GetByKeyAsync(request.IdempotencyKey, cancellationToken);

        if (existing is not null)
        {
            return await HandleExistingAsync(existing, requestHash, request, cancellationToken);
        }

        var idempotencyEntry = IdempotencyEntry.Create(request.IdempotencyKey, requestHash);

        var kind = ParseKind(request.Kind);

        var transaction = Transaction.Create(
            request.AccountId,
            kind,
            request.Amount,
            request.Currency,
            request.TransactionDateUtc,
            request.Description,
            request.CorrelationId);

        var integrationEvent = new TransactionCreatedIntegrationEvent(
            Guid.NewGuid(),
            transaction.Id,
            transaction.AccountId,
            transaction.Kind,
            transaction.Amount,
            transaction.Currency,
            transaction.TransactionDateUtc,
            transaction.Description,
            transaction.CorrelationId,
            DateTime.UtcNow);

        var payload = JsonSerializer.Serialize(integrationEvent, SerializerOptions);

        var outboxMessage = OutboxMessage.Create(
            integrationEvent.EventId,
            integrationEvent.EventName,
            integrationEvent.EventVersion,
            integrationEvent.AggregateId,
            payload,
            integrationEvent.CorrelationId,
            integrationEvent.OccurredOnUtc);

        await _idempotencyRepository.AddAsync(idempotencyEntry, cancellationToken);
        await _repository.AddAsync(transaction, cancellationToken);
        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);

        idempotencyEntry.Complete(transaction.Id);
        await _idempotencyRepository.UpdateAsync(idempotencyEntry, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            var concurrentEntry = await _idempotencyRepository.GetByKeyAsync(request.IdempotencyKey, cancellationToken);

            if (concurrentEntry is null)
            {
                throw;
            }

            return await HandleExistingAsync(concurrentEntry, requestHash,  request, cancellationToken);
        }

        _logger.CreateTransactionPersisted(transaction.Id, outboxMessage.Id, request.CorrelationId);

        return new CreateTransactionResult(transaction.ToDto(), false);
    }

    private async Task<CreateTransactionResult> HandleExistingAsync(
        IdempotencyEntry existing,
        string requestHash,
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
        {
            _logger.CreateTransactionIdempotencyConflict(request.CorrelationId, request.IdempotencyKey);
            throw new IdempotencyConflictApplicationException();
        }

        if (!existing.IsCompleted)
        {
            _logger.CreateTransactionAlreadyInProgress(request.CorrelationId, request.IdempotencyKey);
            throw new RequestInProgressApplicationException();
        }

        var transaction = await _repository.GetByIdAsync(existing.TransactionId!.Value, cancellationToken)
            ?? throw new EntityNotFoundApplicationException(string.Format(MessageCatalog.TransactionNotFound, existing.TransactionId.Value));

        _logger.CreateTransactionReplayed(existing.TransactionId.Value, transaction.CorrelationId, request.IdempotencyKey);

        return new CreateTransactionResult(transaction.ToDto(), true);
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException &&
               postgresException.SqlState == PostgresErrorCodes.UniqueViolation;
    }

    private static TransactionKind ParseKind(string kind)
    {
        if (Enum.TryParse<TransactionKind>(kind, true, out var parsed))
        {
            return parsed;
        }

        throw new ValidationApplicationException(string.Format(MessageCatalog.UnsupportedKind, kind));
    }
}
