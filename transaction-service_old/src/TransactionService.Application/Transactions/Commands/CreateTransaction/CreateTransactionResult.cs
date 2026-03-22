using TransactionService.Application.Transactions.Common;

namespace TransactionService.Application.Transactions.Commands.CreateTransaction;

/// <summary>
/// Represents the result of creating a transaction.
/// </summary>
public sealed record CreateTransactionResult(TransactionDto Transaction, bool IsIdempotentReplay);
