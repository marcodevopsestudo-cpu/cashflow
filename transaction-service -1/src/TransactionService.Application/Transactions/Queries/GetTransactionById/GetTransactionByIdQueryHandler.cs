using MediatR;
using Microsoft.Extensions.Logging;
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
        var transaction = await _repository.GetByIdAsync(request.TransactionId, cancellationToken);

        if (transaction is null)
        {
            _logger.TransactionNotFound(request.TransactionId);
            throw new EntityNotFoundApplicationException(string.Format(MessageCatalog.TransactionNotFound, request.TransactionId));
        }

        return transaction.ToDto();
    }
}
