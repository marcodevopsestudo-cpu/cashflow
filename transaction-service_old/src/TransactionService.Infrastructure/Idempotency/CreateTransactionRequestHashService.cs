using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TransactionService.Application.Abstractions.Idempotency;
using TransactionService.Application.Transactions.Commands.CreateTransaction;

namespace TransactionService.Infrastructure.Idempotency;

/// <summary>
/// Generates deterministic hashes for <see cref="CreateTransactionCommand"/> instances,
/// enabling idempotent request processing.
/// </summary>
/// <remarks>
/// The hash is computed based on a normalized subset of the command properties,
/// ensuring that logically identical requests produce the same hash.
/// </remarks>
public sealed class CreateTransactionRequestHashService : IRequestHashService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Computes a deterministic SHA-256 hash for the specified command.
    /// </summary>
    /// <param name="command">The transaction creation command.</param>
    /// <returns>A hexadecimal string representing the computed hash.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="command"/> is null.
    /// </exception>
    public string ComputeHash(CreateTransactionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var payload = new
        {
            command.AccountId,
            command.Kind,
            command.Amount,
            command.Currency,
            command.TransactionDateUtc,
            command.Description
        };

        // Serialize deterministically
        var json = JsonSerializer.Serialize(payload, JsonOptions);

        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}
