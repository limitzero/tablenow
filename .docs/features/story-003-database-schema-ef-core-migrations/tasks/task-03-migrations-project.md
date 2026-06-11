# Task 03: Migrations Project

## Status

pending

## Wave

3

## Description

Configures the `server/src/Migrations/` project so EF Core CLI can generate and apply migrations, then runs the initial migration that creates the full database schema. The migration must apply cleanly on SQLite (local dev) and SQL Server (production).

## Dependencies

**Depends on:** task-02-ef-models-configs.md
**Blocks:** STORY-004 (seed data needs the schema to exist)

**Context from dependencies:** task-02 created `AppDbContext` with all four entities and their Fluent API configurations. task-01 created the domain entities. The Migrations project (from STORY-001's solution structure) already exists as an empty class library — this task configures it with a design-time factory and generates the initial migration.

## Files to Create

- `server/src/Migrations/DesignTimeDbContextFactory.cs` — enables EF CLI tools
- `server/src/Api/appsettings.Development.json` — SQLite connection string (may already exist from STORY-001)
- `server/src/Migrations/Migrations/` — generated migration files (via CLI)

## Technical Details

### Implementation Steps

1. Create `DesignTimeDbContextFactory` so `dotnet ef` can instantiate `AppDbContext` without running the app:
   ```csharp
   // server/src/Migrations/DesignTimeDbContextFactory.cs
   namespace TableNow.Migrations;

   public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
   {
       public AppDbContext CreateDbContext(string[] args)
       {
           var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
           optionsBuilder.UseSqlite("Data Source=tablenow-design.db");
           return new AppDbContext(optionsBuilder.Options);
       }
   }
   ```

2. Ensure `Migrations.csproj` references `Data/Auth`, `Data/Restaurants`, `Data/Reservations` projects (these hold the configurations) and the `Microsoft.EntityFrameworkCore.Design` package.

3. Run from the solution root:
   ```powershell
   dotnet ef migrations add InitialCreate --project server/src/Migrations --startup-project server/src/Migrations --output-dir Migrations
   ```

4. Verify the generated migration file includes tables for `Users`, `Restaurants`, `TimeSlots`, `Reservations` and the `RowVersion` column on `TimeSlots`.

5. Apply migration to verify it runs:
   ```powershell
   dotnet ef database update --project server/src/Migrations --startup-project server/src/Migrations
   ```

### Environment Variables

- Connection string for production: `ConnectionStrings__Default` environment variable (overrides `appsettings.json`)

## Acceptance Criteria

- [ ] `DesignTimeDbContextFactory` exists and implements `IDesignTimeDbContextFactory<AppDbContext>`
- [ ] `dotnet ef migrations add InitialCreate` generates migration files without errors
- [ ] Generated migration creates `Users`, `Restaurants`, `TimeSlots`, `Reservations` tables
- [ ] `TimeSlots` table has `RowVersion` column of type `rowversion` (SQL Server) or `BLOB` (SQLite)
- [ ] `dotnet ef database update` applies without errors on SQLite

## Notes

The `RowVersion` column maps to `rowversion` on SQL Server and to `BLOB` on SQLite — EF Core handles both transparently. The concurrency behavior is slightly different on SQLite (emulated), but correctness is verified in the SQL Server integration test (STORY-019).
