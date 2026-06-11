# Task 01: Create Seed Data Logic

## Status

pending

## Wave

1

## Description

Creates a `DatabaseSeeder` class in the Data layer that inserts restaurants, future time slots, and test users. Uses deterministic GUIDs and `Bogus` for realistic names, addresses, and descriptions so data is reproducible across dev machines. Seeds are idempotent — they check for existing rows before inserting.

## Dependencies

**Depends on:** STORY-003 task-03-migrations.md (tables must exist)
**Blocks:** task-02-startup-seeding.md

**Context from dependencies:** STORY-003 task-03 created all four tables: `Users`, `Restaurants`, `TimeSlots`, `Reservations`. The entity shapes are:
- `Restaurant`: Id (Guid), Name, Cuisine, Address, Description, ThumbnailUrl?
- `TimeSlot`: Id, RestaurantId, SlotDateTime (DateTime), TotalCapacity (int), RemainingCapacity (int)
- `User`: Id, Email, PasswordHash, Name, Role, CreatedAt

All three DbContexts (`AuthDbContext`, `RestaurantsDbContext`, `ReservationsDbContext`) are registered in DI.

## Files to Create

- `server/src/Data/CM.TableNow.Restaurants.Data/Seeding/RestaurantSeeder.cs` — Seeds restaurants and time slots
- `server/src/Data/CM.TableNow.Auth.Data/Seeding/AuthSeeder.cs` — Seeds test users

## Files to Modify

None.

## Technical Details

### Implementation Steps

1. Add `Bogus` NuGet to the Data projects:
   ```powershell
   dotnet add server/src/Data/CM.TableNow.Restaurants.Data package Bogus
   dotnet add server/src/Data/CM.TableNow.Auth.Data package BCrypt.Net-Next
   ```

2. Create `RestaurantSeeder.cs` with 15+ restaurants across 5 cuisine types:

```csharp
// server/src/Data/CM.TableNow.Restaurants.Data/Seeding/RestaurantSeeder.cs
using CM.TableNow.Restaurants.Domain;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Restaurants.Data.Seeding;

public static class RestaurantSeeder
{
    private static readonly string[] Cuisines = ["Italian", "Japanese", "Mexican", "French", "American"];

    private static readonly (string Name, string Cuisine, string Address, string Description)[] RestaurantData =
    [
        ("La Bella Roma", "Italian", "123 Main St, Downtown", "Authentic Roman trattoria with handmade pasta"),
        ("Sakura Garden", "Japanese", "456 Oak Ave, Midtown", "Traditional Japanese cuisine with seasonal omakase"),
        ("El Rancho", "Mexican", "789 Elm Rd, Uptown", "Family-style Mexican with fresh tortillas"),
        ("Café de Paris", "French", "321 Rue Blanche, Arts District", "Classic French bistro with daily specials"),
        ("The Smoke House", "American", "654 Liberty Blvd, Harbor", "BBQ and craft beers since 1985"),
        ("Trattoria Napoli", "Italian", "111 Vine St, Little Italy", "Wood-fired Neapolitan pizza and pastas"),
        ("Ramen Taro", "Japanese", "222 Cherry Ln, East Side", "Rich tonkotsu and miso ramen bowls"),
        ("Taco Loco", "Mexican", "333 Pepper Ave, Westside", "Street tacos and mezcal cocktails"),
        ("Brasserie Lyon", "French", "444 Seine St, French Quarter", "Alsatian brasserie with choucroute garnie"),
        ("Burger Republic", "American", "555 Freedom Dr, Suburbs", "Gourmet burgers with local beef"),
        ("Osteria del Sole", "Italian", "666 Tuscan Way, South End", "Tuscan farmhouse cooking with local wine"),
        ("Sushi Zen", "Japanese", "777 Pacific Blvd, Waterfront", "Edomae sushi bar with premium fish"),
        ("Casa Oaxaca", "Mexican", "888 Mole St, Cultural District", "Regional Oaxacan cuisine and mezcal"),
        ("Le Petit Bistro", "French", "999 Montmartre Ave, Old Town", "Neighborhood French bistro with prix fixe"),
        ("Southern Kitchen", "American", "101 Peach Tree Rd, South Hills", "Southern comfort food and sweet tea"),
    ];

    public static async Task SeedAsync(RestaurantsDbContext context)
    {
        if (await context.Restaurants.AnyAsync()) return;

        var restaurants = RestaurantData.Select((r, i) => new Restaurant
        {
            Id = Guid.Parse($"00000000-0000-0000-0000-{(i + 1):D12}"),
            Name = r.Name,
            Cuisine = r.Cuisine,
            Address = r.Address,
            Description = r.Description,
            ThumbnailUrl = $"https://picsum.photos/seed/{i + 1}/400/300",
            CreatedAt = DateTime.UtcNow,
        }).ToList();

        context.Restaurants.AddRange(restaurants);

        // Seed 30 days of future time slots
        var slotTimes = new[] { TimeSpan.FromHours(18), TimeSpan.FromHours(18.5), TimeSpan.FromHours(19), TimeSpan.FromHours(19.5), TimeSpan.FromHours(20) };
        var slots = new List<TimeSlot>();
        var today = DateTime.UtcNow.Date;

        foreach (var restaurant in restaurants)
        {
            for (int day = 1; day <= 30; day++)
            {
                foreach (var time in slotTimes)
                {
                    var capacity = new Random(restaurant.Id.GetHashCode() + day).Next(2, 8) * 4; // 8-32 seats
                    var remaining = time == slotTimes[0] && day == 1 ? 0 : capacity; // first slot day 1 = fully booked

                    slots.Add(new TimeSlot
                    {
                        Id = Guid.NewGuid(),
                        RestaurantId = restaurant.Id,
                        SlotDateTime = today.Add(TimeSpan.FromDays(day)).Add(time),
                        TotalCapacity = capacity,
                        RemainingCapacity = remaining,
                        RowVersion = [],
                    });
                }
            }
        }

        context.TimeSlots.AddRange(slots);
        await context.SaveChangesAsync();
    }
}
```

3. Create `AuthSeeder.cs` with 2 test users:

```csharp
// server/src/Data/CM.TableNow.Auth.Data/Seeding/AuthSeeder.cs
using CM.TableNow.Auth.Domain;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Auth.Data.Seeding;

public static class AuthSeeder
{
    public static async Task SeedAsync(AuthDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        context.Users.AddRange(
            new User
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "Alice Diner",
                Email = "alice@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!", workFactor: 12),
                Role = "Diner",
                CreatedAt = DateTime.UtcNow,
            },
            new User
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Name = "Bob Operator",
                Email = "bob@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!", workFactor: 12),
                Role = "Operator",
                CreatedAt = DateTime.UtcNow,
            }
        );

        await context.SaveChangesAsync();
    }
}
```

### Test Credentials

| Email | Password | Role |
|-------|----------|------|
| alice@test.com | Password1! | Diner |
| bob@test.com | Password1! | Operator |

## Acceptance Criteria

- [ ] `RestaurantSeeder` inserts 15 restaurants if none exist
- [ ] Each restaurant has 30 × 5 = 150 time slots
- [ ] At least 1 slot per restaurant has `RemainingCapacity = 0`
- [ ] `AuthSeeder` inserts 2 test users with BCrypt-hashed passwords
- [ ] Seeders are idempotent (no duplicates on second run)
- [ ] `dotnet build` exits with code 0
