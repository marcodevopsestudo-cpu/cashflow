using ConsolidationService.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ConsolidationService.Infrastructure.Data;

/// <summary>
/// Creates PostgreSQL connections for repository usage.
/// </summary>
public sealed class NpgsqlConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(IOptions<PostgresOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public NpgsqlConnection Create() => new(_connectionString);
}
