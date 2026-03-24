using TransactionService.Api.Contracts.Responses;
using TransactionService.Application.Transactions.Common;

namespace TransactionService.Api.Common.Extensions;

/// <summary>
/// Provides response mapping helpers.
/// </summary>
public static class TransactionDtoExtensions
{
    /// <summary>
    /// Maps a transaction DTO to an API response.
    /// </summary>
    /// <param name="dto">The DTO.</param>
    /// <returns>The response.</returns>
    public static TransactionResponse ToResponse(this TransactionDto dto)
        => new(dto.Id, dto.AccountId, dto.Kind, dto.Amount, dto.Currency, dto.DateUtc, dto.Description, dto.Status, dto.CorrelationId, dto.CreatedAtUtc, dto.UpdatedAtUtc);
}
