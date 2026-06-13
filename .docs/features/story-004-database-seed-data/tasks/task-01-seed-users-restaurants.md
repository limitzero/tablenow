# Task 01: Seed Users and Restaurants

## Status

complete

## Wave

1

## Description

Create the seed data mechanism for test user accounts and restaurant records. This task adds a startup extension method (or `HasData` entries) that inserts at least 2 test users and at least 15 restaurants across 3–5 cuisine types using stable hard-coded GUIDs. BCrypt-hashed passwords are embedded so developers can sign in immediately without any manual setup. Restaurant records contain all fields the listing API returns: name, cuisine, address, description, and thumbnailUrl.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-02-seed-time-slots.md

**Context from dependencies:** This is a Wave 1 task. It assumes STORY-003 created the `User` EF model in `CM.TableNow.Auth.Data` (with `AuthDbContext`) and the `Restaurant` EF model in `CM.TableNow.Restaurants.Data` (with `RestaurantsDbContext`). The stable `Guid` values assigned to each restaurant here are used verbatim by task-02 to create the corresponding time slots.

## Files to Create

- `server/src/Data/Auth/Seed/AuthDataSeeder.cs` — Seeds test users; idempotent (checks for existing email before inserting).
- `server/src/Data/Restaurants/Seed/RestaurantDataSeeder.cs` — Seeds restaurant records; idempotent (checks count or individual IDs before inserting).

## Files to Modify

- `server/src/Api/Extensions/ServiceCollectionExtensions.cs` — Register the seeder as a hosted service or call the seed methods from the app startup pipeline.

## Technical Details

### Implementation Steps

1. Add `BCrypt.Net-Next` to the Auth Data project (if not already present from STORY-005 context): `dotnet add server/src/Data/Auth package BCrypt.Net-Next`.
2. Create `AuthDataSeeder` with a static or instance `SeedAsync(AuthDbContext db)` method. Check `db.Users.AnyAsync(u => u.Email == "alice@tablenow.com")` before inserting. Hash passwords using `BCrypt.HashPassword("Password123!", workFactor: 12)`.
3. Create `RestaurantDataSeeder` with a `SeedAsync(RestaurantsDbContext db)` method. Use `db.Restaurants.AnyAsync()` to skip if data already exists.
4. Define 15+ restaurant records with stable GUIDs (use `Guid.Parse("...")`). Cover cuisines: Italian, Japanese, Mexican, American, Indian.
5. Wire both seeders into a startup extension method called from `Program.cs` after `app.MigrateDatabase()` or equivalent.

### Code Snippets

```csharp
// AuthDataSeeder.cs
namespace CM.TableNow.Auth.Data.Seed;

public static class AuthDataSeeder
{
    public static async Task SeedAsync(AuthDbContext db, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct)) return;

        db.Users.AddRange(
            new User
            {
                Id = Guid.Parse("11111111-0000-0000-0000-000000000001"),
                Name = "Alice Demo",
                Email = "alice@tablenow.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!", workFactor: 12),
                Role = "Diner",
                CreatedAt = DateTimeOffset.UtcNow,
            },
            new User
            {
                Id = Guid.Parse("11111111-0000-0000-0000-000000000002"),
                Name = "Bob Demo",
                Email = "bob@tablenow.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!", workFactor: 12),
                Role = "Diner",
                CreatedAt = DateTimeOffset.UtcNow,
            });

        await db.SaveChangesAsync(ct);
    }
}
```

```csharp
// RestaurantDataSeeder.cs — partial sample
public static readonly Guid[] RestaurantIds = [
    Guid.Parse("22222222-0000-0000-0000-000000000001"), // La Bella Italia
    Guid.Parse("22222222-0000-0000-0000-000000000002"), // Sakura Garden
    // ... 13+ more
];
```

### Test Credentials

| Email | Password | Role |
|---|---|---|
| alice@tablenow.com | Password123! | Diner |
| bob@tablenow.com | Password123! | Diner |

## Acceptance Criteria

- [ ] At least 2 user records exist after running the seeder, with BCrypt-hashed passwords.
- [ ] At least 15 restaurant records exist across at least 3 cuisine types.
- [ ] Each restaurant has `Name`, `Cuisine`, `Address`, `Description`, and `ThumbnailUrl` populated.
- [ ] Running the seeder twice does not create duplicate records.
- [ ] `RestaurantDataSeeder` exposes the stable restaurant GUIDs array for use by task-02.

## Notes

- Stable GUIDs are critical: task-02 generates time slots keyed to these IDs. Do not use `Guid.NewGuid()` — hard-code the values.
- BCrypt hashing during startup adds ~0.5s per user account at work factor 12. With only 2 accounts this is acceptable; do not increase the count significantly.
- The `ThumbnailUrl` can point to a public placeholder image service (e.g., `https://placehold.co/400x300?text=Restaurant`) — no real images needed.
