# db-migrator

Console tool that applies PostgreSQL schema migrations in order and stores execution history in `__schema_migrations`.

## Configuration

The tool reads the database connection string from one of these variables:

- `ConnectionStrings__Postgres`
- `POSTGRES_CONNECTION`

The migrations folder can be provided with:

- `--migrations-path=/path/to/migrations`
- `MIGRATIONS_PATH`

If no path is provided, the tool falls back to a local `migrations` folder next to the executable.

## Example

```bash
set ConnectionStrings__Postgres=Host=localhost;Port=5432;Database=transactiondb;Username=postgres;Password=postgres
set MIGRATIONS_PATH=../transaction-service/scripts

dotnet run --project ./db-migrator
```
