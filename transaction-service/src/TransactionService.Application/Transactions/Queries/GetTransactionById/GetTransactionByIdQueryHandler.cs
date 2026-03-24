using MediatR;
using Microsoft.Extensions.Logging;
using Npgsql;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Application.Common.Diagnostics;
using TransactionService.Application.Common.Exceptions;
using TransactionService.Application.Resources;
using TransactionService.Application.Transactions.Common;

namespace TransactionService.Application.Transactions.Queries.GetTransactionById;

/// <summary>
/// Handles transaction retrieval by identifier.
/// </summary>
public sealed class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    private readonly ITransactionRepository _repository;
    private readonly ILogger<GetTransactionByIdQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTransactionByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="logger">The logger.</param>
    public GetTransactionByIdQueryHandler(ITransactionRepository repository, ILogger<GetTransactionByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the query.
    /// </summary>
    /// <param name="request">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transaction DTO.</returns>
    public async Task<TransactionDto> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting transaction by id. TransactionId={TransactionId}",
            request.TransactionId);

        try
        {
            var transaction = await _repository.GetByIdAsync(request.TransactionId, cancellationToken);

            if (transaction is null)
            {
                _logger.TransactionNotFound(request.TransactionId);

                _logger.LogWarning(
                    "Transaction not found. TransactionId={TransactionId}",
                    request.TransactionId);

                throw new EntityNotFoundApplicationException(
                    string.Format(MessageCatalog.TransactionNotFound, request.TransactionId));
            }

            _logger.LogInformation(
                "Transaction retrieved successfully. TransactionId={TransactionId}, AccountId={AccountId}, Amount={Amount}, Currency={Currency}, Kind={Kind}",
                transaction.TransactionId,
                transaction.AccountId,
                transaction.Amount,
                transaction.Currency,
                transaction.Kind);

            return transaction.ToDto();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "GetTransactionById was canceled. TransactionId={TransactionId}",
                request.TransactionId);

            throw;
        }
  

        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while retrieving transaction. TransactionId={TransactionId}",
                request.TransactionId);

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
                    "Postgres exception while savingtransaction. SqlState={SqlState}, Detail={Detail}, ConstraintName={ConstraintName}, TableName={TableName}, ColumnName={ColumnName}",
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
