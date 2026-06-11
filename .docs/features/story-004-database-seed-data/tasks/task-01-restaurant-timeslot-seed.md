# Task 01: Restaurant & TimeSlot Seed

## Status

pending

## Wave

1

## Description

Creates a hosted seeder that inserts 15+ realistic restaurants and 30 days of time slots per restaurant into the database on app startup. Uses the `Bogus` library for realistic names and descriptions. Runs idempotently — skips seeding if restaurants already exist.

## Dependencies

**Depends on:** None (Wave 1 — but requires STORY-003 schema to exist)
**Blocks:** STORY-010, STORY-011 (restaurant/slot queries need data to return)

**Context from dependencies:** STORY-003 created `AppDbContext` with `DbSet<Restaurant>` and `DbSet<TimeSlot>`. This task populates those tables. Does not touch Users table (covered in task-02).

## Files to Create

- `server/src/Data/Restaurants/Seeding/RestaurantSeeder.cs` — IHostedService that seeds restaurants and time slots

## Files to Modify

- Module registration file (wire `RestaurantSeeder` as a hosted service in `AddRestaurantsModule`)

## Technical Details

### Implementation Steps

1. Install `Bogus` NuGet in `Data.Restaurants.csproj`:
   ```xml
   <PackageReference Include="Bogus" Version="35.*" />
   ```

2. Create `RestaurantSeeder : IHostedService` in `server/src/Data/Restaurants/Seeding/`:
   - Inject `IServiceScopeFactory` (not `AppDbContext` directly — hosted services are singletons).
   - In `StartAsync`: create a scope, resolve `AppDbContext`, check `await ctx.Restaurants.AnyAsync()` — if true, skip.
   - If empty: generate 15 restaurants (3 per cuisine × 5 cuisines) using Bogus.
   - Per restaurant: generate 30 days of future slots at times: 08:00, 09:00, 12:00, 13:00, 18:00, 19:00, 20:00.
   - Set each slot: `TotalCapacity = 20`, `RemainingCapacity = 20`.
   - Also add one "fully booked" slot per restaurant: same day as first slot, time 21:00, `RemainingCapacity = 0`.
   - `await ctx.SaveChangesAsync()`.

3. Register in `AddRestaurantsModule`: `services.AddHostedService<RestaurantSeeder>()`.

### Code Snippets

```csharp
// server/src/Data/Restaurants/Seeding/RestaurantSeeder.cs
namespace TableNow.Data.Restaurants.Seeding;

public class RestaurantSeeder(IServiceScopeFactory scopeFactory, ILogger<RestaurantSeeder> logger)
    : IHostedService
{
    private static readonly string[] Cuisines = ["Italian", "Mexican", "Japanese", "American", "French"];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await ctx.Restaurants.AnyAsync(cancellationToken)) return;

        logger.LogInformation("Seeding restaurant data...");

        var faker = new Faker();
        var restaurants = new List<Restaurant>();

        foreach (var cuisine in Cuisines)
        {
            for (var i = 0; i < 3; i++)
            {
                var restaurant = new Restaurant
                {
                    Id = Guid.NewGuid(),
                    Name = $"{faker.Company.CompanyName()} {cuisine} Kitchen",
                    Cuisine = cuisine,
                    Address = faker.Address.FullAddress(),
                    Description = faker.Lorem.Sentence(10),
                    ThumbnailUrl = $"https://picsum.photos/seed/{Guid.NewGuid()}/400/300",
                    CreatedAt = DateTimeOffset.UtcNow,
                };

                // 30 days of slots at 7 meal times
                var slotTimes = new[] { 8, 9, 12, 13, 18, 19, 20 };
                for (var day = 1; day <= 30; day++)
                {
                    foreach (var hour in slotTimes)
                    {
                        restaurant.TimeSlots.Add(new TimeSlot
                        {
                            Id = Guid.NewGuid(),
                            RestaurantId = restaurant.Id,
                            DateTime = DateTimeOffset.UtcNow.Date.AddDays(day).AddHours(hour),
                            TotalCapacity = 20,
                            RemainingCapacity = 20,
                        });
                    }
                }

                // One fully-booked slot
                restaurant.TimeSlots.Add(new TimeSlot
                {
                    Id = Guid.NewGuid(),
                    RestaurantId = restaurant.Id,
                    DateTime = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(21),
                    TotalCapacity = 20,
                    RemainingCapacity = 0,
                });

                restaurants.Add(restaurant);
            }
        }

        ctx.Restaurants.AddRange(restaurants);
        await ctx.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} restaurants", restaurants.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

## Acceptance Criteria

- [ ] At least 15 restaurants seeded across 5 cuisine types
- [ ] Each restaurant has 210+ time slots (30 days × 7 slots/day)
- [ ] Each slot has `TotalCapacity = 20`, `RemainingCapacity = 20` (except the fully-booked slot)
- [ ] At least one slot per restaurant has `RemainingCapacity = 0`
- [ ] Seeder is idempotent (re-running startup when data exists makes no changes)
- [ ] `RestaurantSeeder` is registered as `AddHostedService<RestaurantSeeder>()`
