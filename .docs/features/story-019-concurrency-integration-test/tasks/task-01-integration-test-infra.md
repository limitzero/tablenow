# Task 01: Integration Test Infrastructure

## Status

pending

## Wave

1

## Description

Creates the `api_fixture` base class using `WebApplicationFactory<Program>`, configures an in-memory SQLite test database, and adds helper methods for authenticating test users and seeding a slot with known capacity.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-001 IntegrationTests project, STORY-004 known test credentials)
**Blocks:** task-02-concurrent-booking-test.md

**Context from dependencies:** STORY-001 created `server/tests/IntegrationTests/IntegrationTests.csproj` with `Microsoft.AspNetCore.Mvc.Testing` and `Testcontainers.MsSql`. STORY-004 seeded `diner@test.com / P@ssw0rd123`. The `Program` type is the entry point in `server/src/Api/`. Known test credentials for login: `diner@test.com / P@ssw0rd123`.

## Files to Create

- `server/tests/IntegrationTests/Fixtures/api_fixture.cs`
- `server/src/Api/appsettings.Testing.json`

## Technical Details

### Code Snippets

```csharp
// server/tests/IntegrationTests/Fixtures/api_fixture.cs
namespace TableNow.IntegrationTests.Fixtures;

public class api_fixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Replace DbContext with in-memory SQLite
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite("DataSource=:memory:"));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedTestDataAsync(db);
    }

    private static async Task SeedTestDataAsync(AppDbContext db)
    {
        // Seed test user
        if (!await db.Users.AnyAsync())
        {
            db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Name = "Test Diner",
                Email = "diner@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd123", 12),
                Role = "Diner",
                CreatedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync();
        }
    }

    public async Task<string> AuthenticateAsync(string email = "diner@test.com", string password = "P@ssw0rd123")
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.Token;
    }

    public async Task<Guid> SeedSlotAsync(int remainingCapacity = 1)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var restaurant = new Restaurant
        {
            Id = Guid.NewGuid(), Name = "Test", Cuisine = "Test",
            Address = "Test", Description = "Test", ThumbnailUrl = "",
            CreatedAt = DateTimeOffset.UtcNow,
        };
        var slot = new TimeSlot
        {
            Id = Guid.NewGuid(), RestaurantId = restaurant.Id,
            DateTime = DateTimeOffset.UtcNow.AddDays(1),
            TotalCapacity = 20, RemainingCapacity = remainingCapacity,
        };
        db.Restaurants.Add(restaurant);
        db.TimeSlots.Add(slot);
        await db.SaveChangesAsync();
        return slot.Id;
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}

record LoginResponse(string Token, DateTimeOffset ExpiresAt);
```

```json
// server/src/Api/appsettings.Testing.json
{
  "ConnectionStrings": { "Default": "DataSource=:memory:" },
  "Jwt": {
    "Secret": "test-secret-key-minimum-32-characters-long!!",
    "Issuer": "tablenow",
    "Audience": "tablenow-users",
    "ExpiryHours": 1
  }
}
```

## Acceptance Criteria

- [ ] `api_fixture` extends `WebApplicationFactory<Program>` and `IAsyncLifetime`
- [ ] `ConfigureWebHost` replaces DbContext with SQLite in-memory
- [ ] `InitializeAsync` ensures schema and seeds test user
- [ ] `AuthenticateAsync` returns a valid JWT token
- [ ] `SeedSlotAsync(remainingCapacity)` creates a slot with specified capacity and returns its ID
