using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Application.Common.Exceptions;
using TransactionService.Application.Resources;
using TransactionService.Application.Transactions.Common;

namespace TransactionService.Application.Transactions.Queries.GetDailyBalance;

/// <summary>
/// Handles the query to retrieve the daily balance for a specific date.
/// </summary>
public sealed class GetDailyBalanceQueryHandler
    : IRequestHandler<GetDailyBalanceQuery, DailyBalanceDto>
{
    private readonly IDailyBalanceRepository _repository;
    private readonly ILogger<GetDailyBalanceQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDailyBalanceQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The repository used to access daily balance data.</param>
    /// <param name="logger">The logger instance.</param>
    public GetDailyBalanceQueryHandler(
        IDailyBalanceRepository repository,
        ILogger<GetDailyBalanceQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the query to retrieve the daily balance.
    /// </summary>
    /// <param name="request">The query containing the target date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <see cref="DailyBalanceDto"/> for the specified date.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no daily balance is found for the specified date.
    /// </exception>
    public async Task<DailyBalanceDto> Handle(
        GetDailyBalanceQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting daily balance for date {Date}", request.Date);

        var balance = await _repository.GetByDateAsync(request.Date, cancellationToken);

        if (balance is null)
        {
            throw new EntityNotFoundApplicationException(
                  string.Format(MessageCatalog.TransactionNotFound, request.Date));
        }

        return new DailyBalanceDto
        {
            BalanceDate = balance.BalanceDate,
            TotalCredits = balance.TotalCredits,
            TotalDebits = balance.TotalDebits,
            Balance = balance.Balance,
            UpdatedAtUtc = balance.UpdatedAtUtc
        };
    }
}
