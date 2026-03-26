namespace ConsolidationService.Infrastructure.Resources;

/// <summary>
/// Centralizes infrastructure-layer messages.
/// </summary>
public static class InfrastructureMessageCatalog
{
    /// <summary>
    /// Message indicating that the PostgreSQL connection string was not found.
    /// </summary>
    public const string PostgresConnectionStringNotFound =
        "Postgres connection string not found. Configure ConnectionStrings__Postgres.";

}
