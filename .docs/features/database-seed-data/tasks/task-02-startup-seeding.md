# Task 02: Wire Seeding into App Startup

## Status

pending

## Wave

2

## Description

Registers the seed logic as a hosted service or startup hook in `Program.cs` so that the database is seeded automatically when the application starts in Development mode. Seeds run only when `IHostEnvironment.IsDevelopment()` is true.

## Dependencies

**Depends on:** task-01-seed-data-class.md
**Blocks:** Nothing (seed data enables all subsequent demo/testing)

**Context from dependencies:** task-01 created `RestaurantSeeder` (static method `SeedAsync(RestaurantsDbContext)`) and `AuthSeeder` (static method `SeedAsync(AuthDbContext)`) in their respective Data projects. Both are static classes with async seed methods that check for existing data before inserting.

## Files to Create

- `server/src/Api/Services/DatabaseSeedService.cs` — `IHostedService` that invokes both seeders

## Files to Modify

- `server/src/Api/Program.cs` — Register `DatabaseSeedService` with `AddHostedService`

## Technical Details

### Implementation Steps

1. Create a hosted service that runs once at startup:

```csharp
// server/src/Api/Services/DatabaseSeedService.cs
using CM.TableNow.Auth.Data;
using CM.TableNow.Auth.Data.Seeding;
using CM.TableNow.Restaurants.Data;
using CM.TableNow.Restaurants.Data.Seeding;

namespace CM.TableNow.Api.Services;

public class DatabaseSeedService(
    IServiceProvider serviceProvider,
    IHostEnvironment env,
    ILogger<DatabaseSeedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!env.IsDevelopment()) return;

        await using var scope = serviceProvider.CreateAsyncScope();

        logger.LogInformation("Seeding database...");

        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await AuthSeeder.SeedAsync(authDb);

        var restaurantsDb = scope.ServiceProvider.GetRequiredService<RestaurantsDbContext>();
        await RestaurantSeeder.SeedAsync(restaurantsDb);

        logger.LogInformation("Database seeding complete.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

2. Register in `Program.cs` (add after `builder.Services.RegisterServices(...)`):
```csharp
builder.Services.AddHostedService<DatabaseSeedService>();
```

## Acceptance Criteria

- [ ] Application starts without errors in Development mode
- [ ] On first startup, 15 restaurants and 2 users are visible in the database
- [ ] Re-running the app does not create duplicate records
- [ ] Seeding does NOT run when `ASPNETCORE_ENVIRONMENT` is not `Development`
- [ ] `dotnet build` exits with code 0
