using MediatR;
using TransactionService.Application.Transactions.Common;

namespace TransactionService.Application.Transactions.Queries.GetDailyBalance;

/// <summary>
/// Represents a query to retrieve the daily balance for a specific date.
/// </summary>
/// <param name="Date">The date for which the daily balance should be retrieved.</param>
public sealed record GetDailyBalanceQuery(DateOnly Date) : IRequest<DailyBalanceDto>;
