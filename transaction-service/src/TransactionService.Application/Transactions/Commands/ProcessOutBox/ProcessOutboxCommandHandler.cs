using MediatR;
using Microsoft.Extensions.Logging;
using Npgsql;
using TransactionService.Application.Abstractions.Messaging;
using TransactionService.Application.Common.Diagnostics;
using TransactionService.Application.Transactions.Common;
using TransactionService.Domain.Entities;

namespace TransactionService.Application.Transactions.Commands.ProcessOutBox;

/// <summary>
/// Handles outbox batch processing.
/// </summary>
public sealed class ProcessOutboxCommandHandler(
    IOutboxProcessor outboxProcessor,
    ILogger<ProcessOutboxCommandHandler> logger) : IRequestHandler<ProcessOutboxCommand, OutboxProcessorDto>
{
    private readonly ILogger<ProcessOutboxCommandHandler> _logger = logger;
    private readonly IOutboxProcessor _outboxProcessor = outboxProcessor;

    /// <summary>
    /// ProcessOutbox Command Handler
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OutboxProcessorDto> Handle(ProcessOutboxCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting outbox processing. CorrelationId={CorrelationId}, BatchSize={BatchSize}",
            request.CorrelationId,
            request.BatchSize);

        try
        {
            _logger.LogDebug(
                "Invoking outbox processor. CorrelationId={CorrelationId}, BatchSize={BatchSize}",
                request.CorrelationId,
                request.BatchSize);

            var items = await _outboxProcessor.ProcessPendingMessagesAsync(request.BatchSize, cancellationToken);

            _logger.OutboxProcessingFinished(items);

            _logger.LogInformation(
                "Outbox processing completed successfully. CorrelationId={CorrelationId}, BatchSize={BatchSize}, ProcessedItems={ProcessedItems}",
                request.CorrelationId,
                request.BatchSize,
                items);

            if (items == 0)
            {
                _logger.LogInformation(
                    "Outbox processing finished with no pending messages. CorrelationId={CorrelationId}, BatchSize={BatchSize}",
                    request.CorrelationId,
                    request.BatchSize);
            }

            return new OutboxProcessorDto(items);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Outbox processing was canceled. CorrelationId={CorrelationId}, BatchSize={BatchSize}",
                request.CorrelationId,
                request.BatchSize);

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while processing outbox messages. CorrelationId={CorrelationId}, BatchSize={BatchSize}",
                request.CorrelationId,
                request.BatchSize);

            if (ex is NpgsqlException npgsqlEx)
            {
                _logger.LogError(
                    npgsqlEx,
                    "Npgsql exception while processing outbox messages. CorrelationId={CorrelationId}, Message={Message}",
                    request.CorrelationId,
                    npgsqlEx.Message);
            }

            if (ex.InnerException is NpgsqlException innerNpgsqlEx)
            {
                _logger.LogError(
                    innerNpgsqlEx,
                    "Inner Npgsql exception while processing outbox messages. CorrelationId={CorrelationId}, Message={Message}",
                    request.CorrelationId,
                    innerNpgsqlEx.Message);
            }

            if (ex is PostgresException pgEx)
            {
                _logger.LogError(
                    pgEx,
                    "Postgres exception while processing outbox messages. CorrelationId={CorrelationId}, SqlState={SqlState}, Detail={Detail}, ConstraintName={ConstraintName}, TableName={TableName}, ColumnName={ColumnName}",
                    request.CorrelationId,
                    pgEx.SqlState,
                    pgEx.Detail,
                    pgEx.ConstraintName,
                    pgEx.TableName,
                    pgEx.ColumnName);
            }

            if (ex.InnerException is PostgresException innerPgEx)
            {
                _logger.LogError(
                    innerPgEx,
                    "Inner Postgres exception while processing outbox messages. CorrelationId={CorrelationId}, SqlState={SqlState}, Detail={Detail}, ConstraintName={ConstraintName}, TableName={TableName}, ColumnName={ColumnName}",
                    request.CorrelationId,
                    innerPgEx.SqlState,
                    innerPgEx.Detail,
                    innerPgEx.ConstraintName,
                    innerPgEx.TableName,
                    innerPgEx.ColumnName);
            }

            throw;
        }
    }
}
