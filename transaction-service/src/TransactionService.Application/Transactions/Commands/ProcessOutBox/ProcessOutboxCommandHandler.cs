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
            "Starting outbox processing. BatchSize={BatchSize}",
            request.BatchSize);

        try
        {
            var items = await _outboxProcessor.ProcessPendingMessagesAsync(request.BatchSize, cancellationToken);

            _logger.OutboxProcessingFinished(items);

            _logger.LogInformation(
                "Outbox processing completed successfully. BatchSize={BatchSize}, ProcessedItems={ProcessedItems}",
                request.BatchSize,
                items);

            return new OutboxProcessorDto(items);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Outbox processing was canceled. BatchSize={BatchSize}",
                request.BatchSize);

            throw;
        }
 

        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while saving outbox message. CorrelationId={CorrelationId} ",
                request.CorrelationId);

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
                    "Postgres exception while saving outbox messag. SqlState={SqlState}, Detail={Detail}, ConstraintName={ConstraintName}, TableName={TableName}, ColumnName={ColumnName}",
                    pgEx.SqlState,
                    pgEx.Detail,
                    pgEx.ConstraintName,
                    pgEx.TableName,
                    pgEx.ColumnName);
            }

            throw;
        }
    }
}
