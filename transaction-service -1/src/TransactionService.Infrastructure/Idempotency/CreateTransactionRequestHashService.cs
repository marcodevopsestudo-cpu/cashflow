using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TransactionService.Application.Abstractions.Idempotency;
using TransactionService.Application.Transactions.Commands.CreateTransaction;

namespace TransactionService.Infrastructure.Idempotency;

/// <summary>
/// Creates deterministic hashes for create transaction commands.
/// </summary>
public sealed class CreateTransactionRequestHashService : IRequestHashService
{
    public string ComputeHash(CreateTransactionCommand command)
    {
        var payload = new
        {
            command.AccountId,
            command.Kind,
            command.Amount,
            command.Currency,
            command.TransactionDateUtc,
            command.Description
        };

        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}
