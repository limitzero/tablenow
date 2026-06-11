# Task 02: User Accounts Seed

## Status

pending

## Wave

1

## Description

Creates a hosted seeder that inserts two test user accounts into the database on app startup — one diner and one operator. Passwords are hashed with BCrypt work factor 12. Runs idempotently. This is parallel to task-01 since it touches different tables (Users vs. Restaurants/TimeSlots).

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-003 schema and STORY-001 solution)
**Blocks:** STORY-005 (registration), STORY-006 (login integration tests), STORY-019 (concurrent booking test needs authenticated users)

**Context from dependencies:** STORY-003 created `AppDbContext` with `DbSet<User>`. This task populates that table. Does not touch Restaurants or TimeSlots (covered in task-01).

## Files to Create

- `server/src/Data/Auth/Seeding/UserSeeder.cs` — IHostedService that seeds test accounts

## Files to Modify

- Auth module registration — wire `UserSeeder` as a hosted service

## Technical Details

### Implementation Steps

1. Create `UserSeeder : IHostedService` in `server/src/Data/Auth/Seeding/`.

2. In `StartAsync`: check `await ctx.Users.AnyAsync()` — if true, skip.

3. If empty: create two users with known credentials hashed via `BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12)`.

4. Register in `AddAuthModule`: `services.AddHostedService<UserSeeder>()`.

### Code Snippets

```csharp
// server/src/Data/Auth/Seeding/UserSeeder.cs
namespace TableNow.Data.Auth.Seeding;

public class UserSeeder(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await ctx.Users.AnyAsync(cancellationToken)) return;

        var users = new[]
        {
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Test Diner",
                Email = "diner@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd123", workFactor: 12),
                Role = "Diner",
                CreatedAt = DateTimeOffset.UtcNow,
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Test Operator",
                Email = "operator@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd123", workFactor: 12),
                Role = "Operator",
                CreatedAt = DateTimeOffset.UtcNow,
            },
        };

        ctx.Users.AddRange(users);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

### Known Credentials

| Email | Password | Role |
|-------|----------|------|
| diner@test.com | P@ssw0rd123 | Diner |
| operator@test.com | P@ssw0rd123 | Operator |

## Acceptance Criteria

- [ ] `diner@test.com` exists in Users table with Role="Diner"
- [ ] `operator@test.com` exists in Users table with Role="Operator"
- [ ] Passwords are BCrypt hashed with work factor 12 (not stored as plaintext)
- [ ] Seeder is idempotent — skips if any users already exist
- [ ] `UserSeeder` is registered as `AddHostedService<UserSeeder>()`
