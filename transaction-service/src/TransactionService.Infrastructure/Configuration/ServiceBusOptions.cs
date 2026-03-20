namespace TransactionService.Infrastructure.Configuration;

/// <summary>
/// Represents Service Bus settings.
/// </summary>
public sealed class ServiceBusOptions
{
    /// <summary>
    /// Gets or sets the topic name.
    /// </summary>
    public string TopicName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fully qualified namespace.
    /// </summary>
    public string? FullyQualifiedNamespace { get; set; }

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether managed identity is enabled.
    /// </summary>
    public bool UseManagedIdentity { get; set; } = true;
}
