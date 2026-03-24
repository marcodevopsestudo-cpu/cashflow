using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.Json;
using TransactionService.Application.Abstractions.Idempotency;
using TransactionService.Application.Abstractions.Messaging;
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
        _logger.LogInformation(
            "CreateTransaction request received. AccountId={AccountId}, CorrelationId={CorrelationId}, IdempotencyKey={IdempotencyKey}, Kind={Kind}, Amount={Amount}, Currency={Currency}, TransactionDateUtc={TransactionDateUtc}, RequestHash={RequestHash}",
            request.AccountId,
            request.CorrelationId,
            request.IdempotencyKey,
            request.Kind,
            request.Amount,
            request.Currency,
            request.TransactionDateUtc,
            requestHash);

        var existing = await _idempotencyRepository.GetByKeyAsync(request.IdempotencyKey, cancellationToken);

        if (existing is not null)
        {
            _logger.LogInformation(
                "Existing idempotency entry found. IdempotencyKey={IdempotencyKey}, ExistingTransactionId={ExistingTransactionId}, ExistingRequestHash={ExistingRequestHash}, IsCompleted={IsCompleted}, CreatedAtUtc={CreatedAtUtc}",
                existing.IdempotencyKey,
                existing.TransactionId,
                existing.RequestHash,
                existing.IsCompleted,
                existing.CreatedAtUtc);

            return await HandleExistingAsync(existing, requestHash, request, cancellationToken);
        }

        var idempotencyEntry = IdempotencyEntry.Create(request.IdempotencyKey, requestHash);
        _logger.LogInformation(
            "New idempotency entry created. IdempotencyKey={IdempotencyKey}, RequestHash={RequestHash}",
            idempotencyEntry.IdempotencyKey,
            idempotencyEntry.RequestHash);

        var kind = ParseKind(request.Kind);
        _logger.LogInformation(
            "Transaction kind parsed successfully. InputKind={InputKind}, ParsedKind={ParsedKind}",
            request.Kind,
            kind);

        var transaction = Transaction.Create(
            request.AccountId,
            kind,
            request.Amount,
            request.Currency,
            request.TransactionDateUtc,
            request.Description,
            request.CorrelationId);

        _logger.LogInformation(
            "Transaction aggregate created. TransactionId={TransactionId}, AccountId={AccountId}, Kind={Kind}, Amount={Amount}, Currency={Currency}, CorrelationId={CorrelationId}",
            transaction.TransactionId,
            transaction.AccountId,
            transaction.Kind,
            transaction.Amount,
            transaction.Currency,
            transaction.CorrelationId);

        var integrationEvent = new TransactionCreatedIntegrationEvent(
            Guid.NewGuid(),
            transaction.TransactionId,
            transaction.AccountId,
            transaction.Kind,
            transaction.Amount,
            transaction.Currency,
            transaction.TransactionDateUtc,
            transaction.Description,
            transaction.CorrelationId,
            DateTime.UtcNow);

        _logger.LogInformation(
            "Integration event created. EventId={EventId}, EventName={EventName}, AggregateId={AggregateId}, CorrelationId={CorrelationId}, OccurredOnUtc={OccurredOnUtc}",
            integrationEvent.EventId,
            integrationEvent.EventName,
            integrationEvent.AggregateId,
            integrationEvent.CorrelationId,
            integrationEvent.OccurredOnUtc);

        var payload = JsonSerializer.Serialize(integrationEvent, SerializerOptions);
        _logger.LogInformation(
            "Integration event serialized. EventId={EventId}, PayloadLength={PayloadLength}",
            integrationEvent.EventId,
            payload.Length);

        var outboxMessage = OutboxMessage.Create(
            integrationEvent.EventId,
            integrationEvent.EventName,
            integrationEvent.EventVersion,
            integrationEvent.AggregateId,
            payload,
            integrationEvent.CorrelationId,
            integrationEvent.OccurredOnUtc);

        _logger.LogInformation(
            "Outbox message created. OutboxMessageId={OutboxMessageId}, AggregateId={AggregateId}, CorrelationId={CorrelationId}, EventName={EventName}, EventVersion={EventVersion}",
            outboxMessage.Id,
            outboxMessage.AggregateId,
            outboxMessage.CorrelationId,
            outboxMessage.EventName,
            outboxMessage.EventVersion);

        _logger.LogDebug(
            "CreateTransaction entities snapshot. IdempotencyEntry={IdempotencyEntryJson}",
            JsonSerializer.Serialize(idempotencyEntry, SerializerOptions));

        _logger.LogDebug(
            "CreateTransaction entities snapshot. Transaction={TransactionJson}",
            JsonSerializer.Serialize(transaction, SerializerOptions));

        _logger.LogDebug(
            "CreateTransaction entities snapshot. OutboxMessage={OutboxMessageJson}",
            JsonSerializer.Serialize(outboxMessage, SerializerOptions));

        await _idempotencyRepository.AddAsync(idempotencyEntry, cancellationToken);
        _logger.LogInformation(
            "Idempotency entry scheduled for persistence. IdempotencyKey={IdempotencyKey}",
            idempotencyEntry.IdempotencyKey);

        await _repository.AddAsync(transaction, cancellationToken);
        _logger.LogInformation(
            "Transaction scheduled for persistence. TransactionId={TransactionId}",
            transaction.TransactionId);

        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        _logger.LogInformation(
            "Outbox message scheduled for persistence. OutboxMessageId={OutboxMessageId}",
            outboxMessage.Id);

        idempotencyEntry.Complete(transaction.TransactionId);
        _logger.LogInformation(
            "Idempotency entry marked as completed. IdempotencyKey={IdempotencyKey}, TransactionId={TransactionId}",
            idempotencyEntry.IdempotencyKey,
            transaction.TransactionId);

        await _idempotencyRepository.UpdateAsync(idempotencyEntry, cancellationToken);
        _logger.LogInformation(
            "Idempotency entry scheduled for update. IdempotencyKey={IdempotencyKey}, TransactionId={TransactionId}",
            idempotencyEntry.IdempotencyKey,
            transaction.TransactionId);

        try
        {
            _logger.LogInformation(
                "Saving changes. CorrelationId={CorrelationId}, TransactionId={TransactionId}, OutboxMessageId={OutboxMessageId}, IdempotencyKey={IdempotencyKey}",
                request.CorrelationId,
                transaction.TransactionId,
                outboxMessage.Id,
                request.IdempotencyKey);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "SaveChanges completed successfully. CorrelationId={CorrelationId}, TransactionId={TransactionId}, OutboxMessageId={OutboxMessageId}",
                request.CorrelationId,
                transaction.TransactionId,
                outboxMessage.Id);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _logger.LogWarning(
                ex,
                "Unique violation detected while saving transaction. CorrelationId={CorrelationId}, IdempotencyKey={IdempotencyKey}",
                request.CorrelationId,
                request.IdempotencyKey);

            var concurrentEntry = await _idempotencyRepository.GetByKeyAsync(request.IdempotencyKey, cancellationToken);

            if (concurrentEntry is null)
            {
                _logger.LogError(
                    ex,
                    "Unique violation detected, but no concurrent idempotency entry was found afterwards. CorrelationId={CorrelationId}, IdempotencyKey={IdempotencyKey}",
                    request.CorrelationId,
                    request.IdempotencyKey);

                throw;
            }

            _logger.LogInformation(
                "Concurrent idempotency entry found after unique violation. IdempotencyKey={IdempotencyKey}, TransactionId={TransactionId}, IsCompleted={IsCompleted}",
                concurrentEntry.IdempotencyKey,
                concurrentEntry.TransactionId,
                concurrentEntry.IsCompleted);

            return await HandleExistingAsync(concurrentEntry, requestHash, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while saving transaction. CorrelationId={CorrelationId}, IdempotencyKey={IdempotencyKey}, TransactionId={TransactionId}, OutboxMessageId={OutboxMessageId}",
                request.CorrelationId,
                request.IdempotencyKey,
                transaction.TransactionId,
                outboxMessage.Id);

            if (ex.InnerException is Npgsql.NpgsqlException npgsqlEx)
            {
                _logger.LogError(
                    npgsqlEx,
                    "Npgsql exception while saving transaction. Message={Message}",
                    npgsqlEx.Message);
            }

            if (ex.InnerException is PostgresException pgEx)
            {
                _logger.LogError(
                    pgEx,
                    "Postgres exception while saving transaction. SqlState={SqlState}, Detail={Detail}, ConstraintName={ConstraintName}, TableName={TableName}, ColumnName={ColumnName}",
                    pgEx.SqlState,
                    pgEx.Detail,
                    pgEx.ConstraintName,
                    pgEx.TableName,
                    pgEx.ColumnName);
            }

            throw;
        }

        _logger.LogInformation("Before CreateTransactionPersisted");
        _logger.CreateTransactionPersisted(transaction.TransactionId, outboxMessage.Id, request.CorrelationId);
        _logger.LogInformation("After CreateTransactionPersisted");

        _logger.LogInformation("Before ToDto");
        var dto = transaction.ToDto();
        _logger.LogInformation("After ToDto");

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
