using MediatR;
using Microsoft.Extensions.Logging;
using Npgsql;
using TransactionService.Application.Abstractions.Messaging;
using TransactionService.Application.Common.Diagnostics;
using TransactionService.Application.Resources;
using TransactionService.Application.Transactions.Common;

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
            MessageCatalog.Logs.ProcessOutboxStarted,
            request.CorrelationId,
            request.BatchSize);

        try
        {
            var items = await _outboxProcessor.ProcessPendingMessagesAsync(request.BatchSize, cancellationToken);

            _logger.OutboxProcessingFinished(items);


            return new OutboxProcessorDto(items);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                MessageCatalog.Logs.ProcessOutboxCanceled,
                request.CorrelationId,
                request.BatchSize);

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                MessageCatalog.Logs.ProcessOutboxUnexpectedError,
                request.CorrelationId,
                request.BatchSize);

            if (ex is NpgsqlException npgsqlEx)
            {
                _logger.LogError(
                    npgsqlEx,
                    MessageCatalog.Logs.ProcessOutboxNpgsqlError,
                    request.CorrelationId,
                    npgsqlEx.Message);
            }

            if (ex.InnerException is NpgsqlException innerNpgsqlEx)
            {
                _logger.LogError(
                    innerNpgsqlEx,
                    MessageCatalog.Logs.ProcessOutboxInnerNpgsqlError,
                    request.CorrelationId,
                    innerNpgsqlEx.Message);
            }

            if (ex is PostgresException pgEx)
            {
                _logger.LogError(
                    pgEx,
                    MessageCatalog.Logs.ProcessOutboxPostgresError,
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
                    MessageCatalog.Logs.ProcessOutboxInnerPostgresError,
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
