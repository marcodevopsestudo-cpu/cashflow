namespace TransactionService.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a transaction.
/// </summary>
public enum TransactionStatus
{
    Received = 1,
    Published = 2,
    FailedToPublish = 3
}
