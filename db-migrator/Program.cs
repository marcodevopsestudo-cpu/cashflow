using System.Security.Cryptography;
using Npgsql;

const string MigrationTable = "__schema_migrations";

var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
    ?? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")
    ?? throw new InvalidOperationException("Connection string not found. Use ConnectionStrings__Postgres or POSTGRES_CONNECTION.");

var migrationsPath = ResolveMigrationsPath(args);
Console.WriteLine($"[Migrator] Using migrations path: {migrationsPath}");

if (!Directory.Exists(migrationsPath))
{
    throw new DirectoryNotFoundException($"Migrations path not found: {migrationsPath}");
}

var scripts = Directory.GetFiles(migrationsPath, "*.sql", SearchOption.TopDirectoryOnly)
    .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
    .ToList();

if (scripts.Count == 0)
{
    Console.WriteLine("[Migrator] No migration scripts found.");
    return;
}

await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();

await EnsureMigrationTableAsync(connection);

foreach (var scriptPath in scripts)
{
    var scriptName = Path.GetFileName(scriptPath);
    var checksum = CalculateChecksum(await File.ReadAllTextAsync(scriptPath));

    var existingChecksum = await GetExistingChecksumAsync(connection, scriptName);
    if (existingChecksum is not null)
    {
        if (!string.Equals(existingChecksum, checksum, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Migration '{scriptName}' was already applied with a different checksum. Create a new migration instead of editing an existing file.");
        }

        Console.WriteLine($"[Migrator] SKIPPED: {scriptName}");
        continue;
    }

    Console.WriteLine($"[Migrator] RUNNING: {scriptName}");

    var sql = await File.ReadAllTextAsync(scriptPath);
    await using var transaction = await connection.BeginTransactionAsync();

    try
    {
        await using var executeCommand = new NpgsqlCommand(sql, connection, transaction);
        await executeCommand.ExecuteNonQueryAsync();

        await using var insertCommand = new NpgsqlCommand(
            $"INSERT INTO {MigrationTable} (script_name, checksum) VALUES (@name, @checksum)",
            connection,
            transaction);
        insertCommand.Parameters.AddWithValue("name", scriptName);
        insertCommand.Parameters.AddWithValue("checksum", checksum);
        await insertCommand.ExecuteNonQueryAsync();

        await transaction.CommitAsync();
        Console.WriteLine($"[Migrator] DONE: {scriptName}");
    }
    catch
    {
        await transaction.RollbackAsync();
        Console.WriteLine($"[Migrator] FAILED: {scriptName}");
        throw;
    }
}

Console.WriteLine("[Migrator] All migrations completed successfully.");

static string ResolveMigrationsPath(string[] args)
{
    var cliArgument = args.FirstOrDefault(static value => value.StartsWith("--migrations-path=", StringComparison.OrdinalIgnoreCase));
    if (!string.IsNullOrWhiteSpace(cliArgument))
    {
        return cliArgument.Split('=', 2)[1];
    }

    return Environment.GetEnvironmentVariable("MIGRATIONS_PATH")
        ?? Path.Combine(AppContext.BaseDirectory, "migrations");
}

static async Task EnsureMigrationTableAsync(NpgsqlConnection connection)
{
    var sql = $@"
CREATE TABLE IF NOT EXISTS {MigrationTable} (
    id SERIAL PRIMARY KEY,
    script_name TEXT NOT NULL UNIQUE,
    checksum TEXT NOT NULL,
    executed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);";

    await using var command = new NpgsqlCommand(sql, connection);
    await command.ExecuteNonQueryAsync();
}

static async Task<string?> GetExistingChecksumAsync(NpgsqlConnection connection, string scriptName)
{
    await using var command = new NpgsqlCommand(
        $"SELECT checksum FROM {MigrationTable} WHERE script_name = @name",
        connection);
    command.Parameters.AddWithValue("name", scriptName);

    var result = await command.ExecuteScalarAsync();
    return result as string;
}

static string CalculateChecksum(string content)
{
    var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(content));
    return Convert.ToHexString(bytes);
}
