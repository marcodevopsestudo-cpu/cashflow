# DB Migrator

## Overview

DB Migrator is the database schema evolution tool of the cashflow solution.

Its purpose is to apply migration scripts in a controlled and repeatable way, ensuring that environments remain aligned with the application and worker expectations.

In architectural terms, this component is small, but strategically important.

It helps move database change management from a manual activity to an auditable and automatable operational step.

---

## Responsibilities

- execute ordered PostgreSQL migration scripts;
- record migration execution history;
- avoid schema drift across environments;
- support repeatable local and CI/CD setup;
- provide a clean boundary for database evolution.

---

## Why This Component Matters

In distributed solutions, runtime services depend heavily on database structure being correct.

Without controlled migration management, common risks include:

- local vs cloud schema mismatch;
- broken deployments after code changes;
- hidden environment drift;
- manual operational mistakes;
- poor auditability of structural changes.

The DB Migrator reduces those risks by turning schema evolution into an explicit operational capability.

---

## Architectural Role

This tool supports both:

- local developer workflow;
- deployment automation workflow.

That makes it part of the solution’s broader maturity story:

- Infrastructure as Code provisions the platform;
- DB Migrator aligns the schema;
- GitHub Actions automates validation and deployment;
- services run against a known database structure.

---

## Configuration

The tool reads the PostgreSQL connection string from:

- `ConnectionStrings__Postgres`
- `POSTGRES_CONNECTION`

The migrations folder can be provided through:

- `--migrations-path=/path/to/migrations`
- `MIGRATIONS_PATH`

If no path is provided, the tool falls back to a local `migrations` folder next to the executable.

---

## Example

```bash
set ConnectionStrings__Postgres=Host=localhost;Port=5432;Database=transactiondb;Username=postgres;Password=postgres
set MIGRATIONS_PATH=../transaction-service/scripts

dotnet run --project ./db-migrator
```
