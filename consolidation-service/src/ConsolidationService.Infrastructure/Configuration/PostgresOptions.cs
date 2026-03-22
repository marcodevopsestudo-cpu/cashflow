namespace ConsolidationService.Infrastructure.Configuration;

/// <summary>
/// Represents the PostgreSQL configuration settings used by the service.
/// </summary>
/// <remarks>
/// This configuration is typically bound from the application configuration
/// (e.g., appsettings.json) using the <c>Postgres</c> section.
/// </remarks>
public sealed class PostgresOptions
{
    /// <summary>
    /// Gets the configuration section name used to bind PostgreSQL settings.
    /// </summary>
    public const string SectionName = "Postgres";

    /// <summary>
    /// Gets or sets the connection string used to connect to the PostgreSQL database.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}
