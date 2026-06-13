# Requirements: Dev Startup SQLite & Migration Script

## Problem

A fresh checkout of the repo cannot start cleanly:

1. `ReservationsDbContext` is not registered in DI — any code path that tries to resolve it at runtime will throw.
2. There is no automated way to apply EF migrations before starting the API. Developers must manually run three separate `dotnet ef database update` commands against three different migration projects.
3. `startup.bat` starts the API without first ensuring the schema is current.

## Goals

- All three EF contexts (`Auth`, `Restaurants`, `Reservations`) are fully wired to the SQLite dev database via DI.
- A single script (`migrate.ps1`) applies all pending migrations to the dev database.
- `startup.bat` calls `migrate.ps1` before the API process launches, aborting if migrations fail.

## Out of Scope

- Production database connection (SQL Server) — that is a separate infrastructure concern.
- Running migrations inside `Program.cs` at boot — this is intentionally kept in the shell layer.
- Seeding data — already handled by `WebApplicationExtensions.SeedDatabaseAsync`.

## Acceptance Criteria

1. `dotnet build` succeeds for the full solution after changes.
2. `ReservationsDbContext` is resolvable from the DI container at runtime.
3. `powershell -File migrate.ps1` (run from repo root) applies all three migration sets without error.
4. Re-running `migrate.ps1` on an already-migrated database is idempotent.
5. `startup.bat` exits non-zero if `migrate.ps1` fails; otherwise both the API and Angular dev server launch.
