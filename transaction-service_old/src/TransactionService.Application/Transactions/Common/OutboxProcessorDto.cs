namespace TransactionService.Application.Transactions.Common;

/// <summary>
/// Represents the result of an outbox processing operation.
/// </summary>
/// <param name="ProcessedItems">
/// The number of items successfully processed during the operation.
/// </param>
public sealed record OutboxProcessorDto(
    int ProcessedItems
);
