# Task 01: Integration Test Infrastructure

## Status

pending

## Wave

1

## Description

Sets up `api_fixture` — a `WebApplicationFactory<Program>`-based base class for integration tests — and optionally Testcontainers for a real SQL Server database. After this task, integration tests can spin up the full API in-process against a real database.

## Dependencies

**Depends on:** STORY-001 task-03-api-startup.md (`public partial class Program { }` must exist in Program.cs)
**Blocks:** task-02-concurrency-test.md

**Context from dependencies:** STORY-001 task-03 added `public partial class Program { }` at the bottom of `Program.cs` to expose the type for `WebApplicationFactory`. The integration test project `CM.TableNow.IntegrationTests` was created in STORY-001 task-01 with a project reference to `CM.TableNow.Api`.

## Files to Create

- `server/tests/IntegrationTests/Fixtures/api_fixture.cs` — Base class for all integration tests
- `server/tests/IntegrationTests/Fixtures/data_context_fixture.cs` — EF in-memory fixture for unit-style data tests

## Files to Modify

- `server/tests/IntegrationTests/CM.TableNow.IntegrationTests.csproj` — Add Testcontainers NuGet (optional)

## Technical Details

### Code Snippets

```csharp
// Fixtures/api_fixture.cs
using Microsoft.AspNetCore.Mvc.Testing;

namespace CM.TableNow.IntegrationTests.Fixtures;

public class api_fixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Replace DbContexts with SQLite in-memory for test isolation
            ReplaceDbContext<AuthDbContext>(services, "auth-test");
            ReplaceDbContext<RestaurantsDbContext>(services, "restaurants-test");
            ReplaceDbContext<ReservationsDbContext>(services, "reservations-test");
        });
    }

    private static void ReplaceDbContext<TContext>(IServiceCollection services, string dbName)
        where TContext : DbContext
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (descriptor is not null) services.Remove(descriptor);
        services.AddDbContext<TContext>(opts => opts.UseSqlite($"Data Source={dbName}.db"));
    }

    public async Task InitializeAsync()
    {
        // Run migrations against the test database
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<RestaurantsDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<ReservationsDbContext>().Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        // Clean up test database files
        foreach (var db in new[] { "auth-test.db", "restaurants-test.db", "reservations-test.db" })
            if (File.Exists(db)) File.Delete(db);
        await base.DisposeAsync();
    }
}
```

### NuGet Packages

```powershell
# In IntegrationTests project:
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.EntityFrameworkCore.Sqlite

# Optional (for SQL Server-based tests):
dotnet add package Testcontainers.MsSql
```

## Acceptance Criteria

- [ ] `api_fixture` extends `WebApplicationFactory<Program>` and replaces DbContexts with SQLite test instances
- [ ] `InitializeAsync` runs EF migrations against test databases
- [ ] `dotnet test` runs with zero failures (even if no tests yet)
- [ ] `dotnet build` exits with code 0

## Notes

SQLite is used instead of Testcontainers to avoid Docker dependency in CI. If Docker is available, replace `UseSqlite` with `UseSqlServer(testContainerConnectionString)` using the `Testcontainers.MsSql` package.
