namespace ConsolidationService.Infrastructure.Configuration;

/// <summary>
/// Represents the PostgreSQL configuration used by the service.
/// </summary>
public sealed class PostgresOptions
{
    public const string SectionName = "Postgres";

    public string ConnectionString { get; set; } = string.Empty;
}
