using MediatR;
using TransactionService.Application.Transactions.Common;

namespace TransactionService.Application.Transactions.Queries.GetTransactionById;

/// <summary>
/// Represents the get transaction by id query.
/// </summary>
public sealed record GetTransactionByIdQuery(Guid TransactionId) : IRequest<TransactionDto>;
