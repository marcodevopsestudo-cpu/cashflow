using TransactionService.Application.Transactions.Commands.CreateTransaction;

namespace TransactionService.Application.Abstractions.Idempotency;

/// <summary>
/// Generates stable hashes for idempotent requests.
/// </summary>
public interface IRequestHashService
{
    string ComputeHash(CreateTransactionCommand command);
}
