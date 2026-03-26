namespace TransactionService.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a transaction.
/// </summary>
/// <remarks>
/// Indicates the processing stage of a transaction within the system,
/// particularly in relation to event publishing or integration workflows.
/// </remarks>
public enum TransactionStatus
{
    /// <summary>
    /// The transaction has been received and recorded but not yet published.
    /// </summary>
    Received = 1,

    /// <summary>
    /// The transaction has been successfully published to downstream systems.
    /// </summary>
    Published = 2,

    /// <summary>
    /// The transaction failed to be published and may require retry or investigation.
    /// </summary>
    FailedToPublish = 3
}
