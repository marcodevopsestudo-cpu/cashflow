using MediatR;

namespace TransactionService.Application.Transactions.Commands.CreateTransaction;

/// <summary>
/// Represents the create transaction command.
/// </summary>
public sealed record CreateTransactionCommand(
    string AccountId,
    string Kind,
    decimal Amount,
    string Currency,
    DateTime TransactionDateUtc,
    string? Description,
    string CorrelationId,
    string IdempotencyKey) : IRequest<CreateTransactionResult>;
