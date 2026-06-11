# Task 04: Database Seed Data

## Status

pending

## Phase

3

## Description

Populate the database with realistic demonstration data so the MVP is fully functional without any manual data entry. Seed at least 15 restaurants across 3–5 cuisine types, 30 days of future time slots per restaurant (multiple slots per day), and 2 test user accounts. The seeder runs at application startup and is idempotent — running it twice must not create duplicate data.

## Dependencies

**Depends on:** task-03-database-schema  
**Blocks:** task-07-restaurant-slot-api

**Context from dependencies:** task-03 created `AppDbContext` with `DbSet<RestaurantModel>`, `DbSet<TimeSlotModel>`, `DbSet<UserModel>`, and `DbSet<ReservationModel>`. The `TimeSlotModel` has `RemainingCapacity` and `TotalCapacity` fields. `UserModel` has `Email`, `PasswordHash`, and `Role` fields. Migrations have been applied and the schema exists.

## Files to Create

- `server/src/Data/Restaurants/Seed/RestaurantSeeder.cs` — Creates restaurants and time slots
- `server/src/Data/Auth/Seed/UserSeeder.cs` — Creates test user accounts
- `server/src/Api/SeedExtensions.cs` — `IApplicationBuilder` extension that calls seeders on startup

## Files to Modify

- `server/src/Api/Program.cs` — Call `app.SeedDatabase()` before `app.Run()`
- `server/src/Api/ServiceCollectionExtensions.cs` — Add `Bogus` package and seed services

## Technical Details

### Implementation Steps

1. **Add Bogus package for fake data generation:**
   ```powershell
   dotnet add src/Data/Restaurants package Bogus
   dotnet add src/Data/Auth package BCrypt.Net-Next
   ```

2. **Write `RestaurantSeeder`** — seeds restaurants and time slots using `Bogus`.

3. **Write `UserSeeder`** — seeds 2 test users with hashed passwords.

4. **Write `SeedExtensions`** — wire seeders together, called from `Program.cs`.

5. **Register `AppDbContext` for seeders** — seeders receive `AppDbContext` via DI.

### Code Snippets

**`RestaurantSeeder.cs`:**
```csharp
namespace TableNow.Restaurants.Data.Seed;

public static class RestaurantSeeder
{
    private static readonly string[] Cuisines =
        ["Italian", "Japanese", "Mexican", "French", "Indian", "American"];

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Restaurants.AnyAsync()) return; // idempotent

        var restaurantFaker = new Faker<RestaurantModel>()
            .RuleFor(r => r.Id, _ => Guid.NewGuid())
            .RuleFor(r => r.Name, f => f.Company.CompanyName())
            .RuleFor(r => r.Cuisine, f => f.PickRandom(Cuisines))
            .RuleFor(r => r.Address, f => $"{f.Address.StreetAddress()}, {f.Address.City()}")
            .RuleFor(r => r.Description, f => f.Lorem.Sentence(10))
            .RuleFor(r => r.ThumbnailUrl, f => null);

        var restaurants = restaurantFaker.Generate(15);
        await db.Restaurants.AddRangeAsync(restaurants);

        var slots = GenerateSlots(restaurants);
        await db.TimeSlots.AddRangeAsync(slots);

        await db.SaveChangesAsync();
    }

    private static List<TimeSlotModel> GenerateSlots(List<RestaurantModel> restaurants)
    {
        var slots = new List<TimeSlotModel>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var r in restaurants)
        {
            for (var dayOffset = 1; dayOffset <= 30; dayOffset++)
            {
                var date = today.AddDays(dayOffset);
                // 4 slots per day: 12:00, 14:00, 18:00, 20:00
                foreach (var hour in new[] { 12, 14, 18, 20 })
                {
                    slots.Add(new TimeSlotModel
                    {
                        Id = Guid.NewGuid(),
                        RestaurantId = r.Id,
                        Date = date,
                        Time = new TimeOnly(hour, 0),
                        TotalCapacity = 20,
                        RemainingCapacity = 20,
                    });
                }
            }
        }
        return slots;
    }
}
```

**`UserSeeder.cs`:**
```csharp
namespace TableNow.Auth.Data.Seed;

public static class UserSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return; // idempotent

        var users = new[]
        {
            new UserModel
            {
                Id = Guid.NewGuid(),
                Name = "Alice Diner",
                Email = "alice@tablenow.dev",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!", workFactor: 12),
                Role = "diner",
                CreatedAt = DateTime.UtcNow,
            },
            new UserModel
            {
                Id = Guid.NewGuid(),
                Name = "Bob Diner",
                Email = "bob@tablenow.dev",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!", workFactor: 12),
                Role = "diner",
                CreatedAt = DateTime.UtcNow,
            },
        };

        await db.Users.AddRangeAsync(users);
        await db.SaveChangesAsync();
    }
}
```

**`SeedExtensions.cs`:**
```csharp
namespace TableNow.Api;

public static class SeedExtensions
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(); // applies pending migrations
        await UserSeeder.SeedAsync(db);
        await RestaurantSeeder.SeedAsync(db);
    }
}
```

**`Program.cs` addition (after `var app = builder.Build()`):**
```csharp
await app.SeedDatabaseAsync();
```

### Test User Credentials

| Name | Email | Password | Role |
|------|-------|----------|------|
| Alice Diner | alice@tablenow.dev | Password1! | diner |
| Bob Diner | bob@tablenow.dev | Password1! | diner |

## Acceptance Criteria

- [ ] After `dotnet run`, the `Restaurants` table contains at least 15 rows
- [ ] Each restaurant has at least 4 × 30 = 120 time slots; all slots have `RemainingCapacity = 20`
- [ ] The `Users` table contains 2 test accounts with BCrypt-hashed passwords
- [ ] Running the application a second time does not duplicate data (idempotent check on `AnyAsync()`)
- [ ] Slot times span the next 30 days from the current date at application startup

## Notes

- Use `db.Database.MigrateAsync()` in the seeder extension — this applies any pending EF migrations automatically and is safe to call on every start.
- `BCrypt.Net.BCrypt.HashPassword` with `workFactor: 12` matches the production hashing strategy from task-05.
- `ThumbnailUrl` is nullable for MVP — images can be added later.
- Seed data is deterministic enough for demos but not truly random between runs (idempotency guard returns early after first seed).
