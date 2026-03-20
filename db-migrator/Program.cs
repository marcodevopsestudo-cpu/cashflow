using Npgsql;
using System;
using System.IO;
using System.Linq;

const string MigrationTable = "__schema_migrations";

// -----------------------------
// Resolve connection string
// -----------------------------
var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
    ?? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")
    ?? throw new InvalidOperationException("Connection string not found. Use ConnectionStrings__Postgres or POSTGRES_CONNECTION.");

// -----------------------------
// Resolve migrations path
// -----------------------------
string? argPath = args
    .FirstOrDefault(a => a.StartsWith("--migrations-path="))
    ?.Split("=", 2)[1];

var migrationsPath =
    argPath
    ?? Environment.GetEnvironmentVariable("MIGRATIONS_PATH")
    ?? Path.Combine(AppContext.BaseDirectory, "migrations");

Console.WriteLine($"[Migrator] Using migrations path: {migrationsPath}");

if (!Directory.Exists(migrationsPath))
    throw new DirectoryNotFoundException($"Migrations path not found: {migrationsPath}");

// -----------------------------
// Load scripts
// -----------------------------
var scripts = Directory.GetFiles(migrationsPath, "*.sql")
    .OrderBy(x => x)
    .ToList();

if (!scripts.Any())
{
    Console.WriteLine("[Migrator] No migration scripts found.");
    return;
}

// -----------------------------
// Connect DB
// -----------------------------
await using var conn = new NpgsqlConnection(connectionString);

try
{
    await conn.OpenAsync();
}
catch (Exception ex)
{
    Console.WriteLine("[Migrator] Failed to connect to database.");
    Console.WriteLine(ex);
    throw;
}

// -----------------------------
// Ensure migration table
// -----------------------------
var createTableSql = $@"
CREATE TABLE IF NOT EXISTS {MigrationTable} (
    id SERIAL PRIMARY KEY,
    script_name TEXT NOT NULL UNIQUE,
    executed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);";

await using (var cmd = new NpgsqlCommand(createTableSql, conn))
{
    await cmd.ExecuteNonQueryAsync();
}

// -----------------------------
// Execute migrations
// -----------------------------
foreach (var script in scripts)
{
    var scriptName = Path.GetFileName(script);

    // Check if already executed
    await using var checkCmd = new NpgsqlCommand(
        $"SELECT COUNT(1) FROM {MigrationTable} WHERE script_name = @name",
        conn
    );

    checkCmd.Parameters.AddWithValue("name", scriptName);

    var alreadyExecuted = (long)(await checkCmd.ExecuteScalarAsync() ?? 0) > 0;

    if (alreadyExecuted)
    {
        Console.WriteLine($"[Migrator] SKIPPED: {scriptName}");
        continue;
    }

    Console.WriteLine($"[Migrator] RUNNING: {scriptName}");

    var sql = await File.ReadAllTextAsync(script);

    await using var tx = await conn.BeginTransactionAsync();

    try
    {
        // Execute script
        await using var execCmd = new NpgsqlCommand(sql, conn, tx);
        await execCmd.ExecuteNonQueryAsync();

        // Register migration
        await using var insertCmd = new NpgsqlCommand(
            $"INSERT INTO {MigrationTable} (script_name) VALUES (@name)",
            conn, tx
        );
        insertCmd.Parameters.AddWithValue("name", scriptName);
        await insertCmd.ExecuteNonQueryAsync();

        await tx.CommitAsync();

        Console.WriteLine($"[Migrator] DONE: {scriptName}");
    }
    catch (Exception ex)
    {
        await tx.RollbackAsync();

        Console.WriteLine($"[Migrator] FAILED: {scriptName}");
        Console.WriteLine(ex);

        throw;
    }
}

Console.WriteLine("[Migrator] All migrations completed successfully.");