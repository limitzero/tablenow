# Task 01: SQLite Data Connection & Migration Startup Script

## Status

complete

## Wave

1

## Description

Three things need to happen to make `startup.bat` self-contained for a fresh checkout:

1. **Register `ReservationsDbContext` with SQLite** — `ReservationsModuleExtensions.AddReservationsModule()` has a TODO placeholder; the context must be wired up the same way Auth and Restaurants already are.
2. **Create `migrate.ps1`** — a PowerShell script at the repo root that applies all three EF migration projects (`Auth`, `Restaurants`, `Reservations`) against the SQLite dev database by running `dotnet ef database update` for each project in dependency order.
3. **Update `startup.bat`** — call `migrate.ps1` (via `powershell`) before launching the API so the database is always current before the backend process starts.

## Dependencies

**Depends on:** STORY-003 (migrations exist), STORY-004 (seed data runs post-migrate)
**Blocks:** None

## Files to Create

- `migrate.ps1` (repo root) — runs all three `dotnet ef database update` commands

## Files to Modify

- `server/src/Infrastructure/Reservations/Extensions/ReservationsModuleExtensions.cs` — add `AddDbContext<ReservationsDbContext>` with `UseSqlite`
- `server/src/Infrastructure/Reservations/CM.TableNow.Reservations.Infrastructure.csproj` — add `<ProjectReference>` to `CM.TableNow.Reservations.Data` and package references for EF SQLite if not already present
- `startup.bat` — invoke `migrate.ps1` before `dotnet run`

## Technical Details

### 1 — ReservationsModuleExtensions

Replace the existing TODO comment body with the same pattern used by Auth and Restaurants:

```csharp
using CM.TableNow.Reservations.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CM.TableNow.Reservations.Infrastructure.Extensions;

public static class ReservationsModuleExtensions
{
    public static IServiceCollection AddReservationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ReservationsDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        return services;
    }
}
```

The connection string `"Default"` is already defined in `appsettings.Development.json` as `Data Source=tablenow.dev.db`.

### 2 — Infrastructure project file

`CM.TableNow.Reservations.Infrastructure.csproj` must reference the Data project and carry the SQLite EF provider:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\Data\Reservations\CM.TableNow.Reservations.Data.csproj" />
</ItemGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="*" />
</ItemGroup>
```

Verify the Auth and Restaurants infrastructure `.csproj` files to confirm this is the expected pattern before adding.

### 3 — migrate.ps1

Create at the repo root. Each `--project` points to the migration project; `--startup-project` is the API (needed so EF can resolve the connection string from `appsettings.Development.json`):

```powershell
#!/usr/bin/env pwsh
# Applies all EF Core migrations to the local SQLite dev database.
# Run from the repo root before starting the API.

$ErrorActionPreference = 'Stop'

$api     = "server/src/Api/CM.TableNow.Api.csproj"
$authMig = "server/src/Migrations/Auth/CM.TableNow.Auth.Migrations.csproj"
$restMig = "server/src/Migrations/Restaurants/CM.TableNow.Restaurants.Migrations.csproj"
$resvMig = "server/src/Migrations/Reservations/CM.TableNow.Reservations.Migrations.csproj"

Write-Host "Applying Auth migrations..."
dotnet ef database update --project $authMig --startup-project $api

Write-Host "Applying Restaurants migrations..."
dotnet ef database update --project $restMig --startup-project $api

Write-Host "Applying Reservations migrations..."
dotnet ef database update --project $resvMig --startup-project $api

Write-Host "All migrations applied."
```

### 4 — startup.bat

Update to run `migrate.ps1` synchronously before launching the backend. The migration step must complete (exit 0) before `dotnet run` starts:

```bat
@echo off

echo Running EF migrations...
powershell -ExecutionPolicy Bypass -File "%~dp0migrate.ps1"
if errorlevel 1 (
    echo Migration failed. Aborting startup.
    exit /b 1
)

start "TableNow - BackEnd" dotnet run --project server\src\Api\CM.TableNow.Api.csproj

pushd client
ng serve
popd
```

Note the corrected path `server\src\Api\...` (original had a leading `.` which is invalid for `start`).

## Acceptance Criteria

- [ ] `dotnet build` succeeds with no errors after the `ReservationsModuleExtensions` change.
- [ ] Running `powershell -File migrate.ps1` from the repo root applies all three migration sets to `tablenow.dev.db` without error.
- [ ] Running `migrate.ps1` a second time is idempotent (EF reports "No pending migrations").
- [ ] `startup.bat` aborts with a non-zero exit code if `migrate.ps1` fails.
- [ ] `startup.bat` launches both the Angular dev server and the .NET API when migrations succeed.

## Notes

- `dotnet ef database update` is idempotent by design — re-running it on an up-to-date database is safe.
- The `tablenow.dev.db` file lives in `server/src/Api/` (the working directory of `dotnet run`). The `Data Source=tablenow.dev.db` connection string is relative to the process working directory, so `migrate.ps1` and the API must resolve to the same file. Since `migrate.ps1` passes `--startup-project` pointing at the API project, EF will use the API's output directory as the base. This is consistent with what the `IDesignTimeDbContextFactory` implementations already do.
- Do not add migration application to `Program.cs` (`MigrateAsync()` at startup). Keeping migrations in the shell script keeps the runtime lean and makes schema changes explicit.
