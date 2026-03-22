using TransactionService.Application.Transactions.Commands.CreateTransaction;

namespace TransactionService.Application.Abstractions.Idempotency;

/// <summary>
/// Defines a contract for generating stable hashes used in idempotent request processing.
/// </summary>
/// <remarks>
/// Implementations must ensure that the same logical request always produces
/// the same hash, regardless of serialization or property ordering.
/// </remarks>
public interface IRequestHashService
{
    /// <summary>
    /// Computes a deterministic hash for the specified request command.
    /// </summary>
    /// <param name="command">The command representing the request to be hashed.</param>
    /// <returns>A stable hash string representing the request.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="command"/> is null.
    /// </exception>
    string ComputeHash(CreateTransactionCommand command);
}
