# Applies all EF Core migrations to the local SQLite dev database.
# Run from the repo root before starting the API.
#
# Each migration project carries its own IDesignTimeDbContextFactory and is therefore
# used as both --project and --startup-project. The dev database is the single SQLite
# file that the API uses at runtime (server/src/Api/tablenow.dev.db). Because the
# design-time factory's connection string is relative to the startup project's
# directory, we pass an absolute --connection so every migration set targets that one
# shared database instead of creating a stray copy next to each migration project.

$ErrorActionPreference = 'Stop'

$root   = $PSScriptRoot
$apiDir = Join-Path $root 'server/src/Api'
$dbPath = Join-Path $apiDir 'tablenow.dev.db'
$connection = "Data Source=$dbPath"

$authMig = Join-Path $root 'server/src/Migrations/Auth/CM.TableNow.Auth.Migrations.csproj'
$restMig = Join-Path $root 'server/src/Migrations/Restaurants/CM.TableNow.Restaurants.Migrations.csproj'
$resvMig = Join-Path $root 'server/src/Migrations/Reservations/CM.TableNow.Reservations.Migrations.csproj'

function Update-Database {
    param(
        [string] $Name,
        [string] $Project
    )

    Write-Host "Applying $Name migrations..."
    dotnet ef database update --project $Project --startup-project $Project --connection $connection
    if ($LASTEXITCODE -ne 0) {
        throw "$Name migrations failed (exit code $LASTEXITCODE)."
    }
}

Update-Database -Name 'Auth' -Project $authMig
Update-Database -Name 'Restaurants' -Project $restMig
Update-Database -Name 'Reservations' -Project $resvMig

Write-Host "All migrations applied."
