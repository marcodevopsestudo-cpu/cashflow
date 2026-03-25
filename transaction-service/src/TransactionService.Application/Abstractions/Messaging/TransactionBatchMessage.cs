namespace TransactionService.Application.Abstractions.Messaging;

/// <summary>
/// Represents a batch message containing transaction identifiers to be processed.
/// </summary>
/// <param name="BatchId">The unique identifier of the transaction batch.</param>
/// <param name="TransactionIds">The collection of transaction identifiers associated with the batch.</param>
public sealed record TransactionBatchMessage(
    Guid BatchId,
    IReadOnlyCollection<Guid> TransactionIds);
