using ConsolidationService.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ConsolidationService.Infrastructure.Data;

/// <summary>
/// Factory responsible for creating <see cref="NpgsqlConnection"/> instances
/// used by repositories to interact with PostgreSQL.
/// </summary>
public sealed class NpgsqlConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="NpgsqlConnectionFactory"/> class.
    /// </summary>
    /// <param name="options">
    /// The PostgreSQL configuration options containing the connection string.
    /// </param>
    public NpgsqlConnectionFactory(IOptions<PostgresOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    /// <summary>
    /// Creates a new <see cref="NpgsqlConnection"/> instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="NpgsqlConnection"/> configured with the connection string.
    /// </returns>
    /// <remarks>
    /// The returned connection is not opened automatically. The caller is responsible
    /// for opening and disposing the connection.
    /// </remarks>
    public NpgsqlConnection Create() => new(_connectionString);
}
