# Task 12: Concurrency Integration Test

## Status

pending

## Phase

6

## Description

Write an integration test that proves double-booking is prevented under concurrent load. Two simultaneous `POST /api/reservations` requests targeting the same slot with only 1 remaining seat must produce exactly one 201 and one 409. The test uses `WebApplicationFactory<Program>` with a real database (SQLite in-memory or Testcontainers SQL Server) and `Task.WhenAll` to fire both requests at the same instant.

## Dependencies

**Depends on:** task-09-reservation-creation-backend  
**Blocks:** (none)

**Context from dependencies:** task-09 implemented `POST /api/v1/reservations` with EF Core optimistic concurrency on `TimeSlot.RowVersion`. The `[Timestamp]` concurrency token means EF throws `DbUpdateConcurrencyException` when two transactions try to update the same slot simultaneously, and the handler retries once before returning 409. `AppDbContext` is registered with SQLite via `"Data Source=tablenow.db"` in `appsettings.json`. The `Program` class is `public partial` for `WebApplicationFactory` compatibility.

## Files to Create

- `server/tests/IntegrationTests/Reservations/describe_concurrent_booking.cs`
- `server/tests/IntegrationTests/Fixtures/api_fixture.cs` — `WebApplicationFactory<Program>` base
- `server/tests/IntegrationTests/Fixtures/database_seeder.cs` — creates minimal test data

## Files to Modify

- `server/tests/IntegrationTests/TableNow.IntegrationTests.csproj` — ensure `Microsoft.AspNetCore.Mvc.Testing` is referenced

## Technical Details

### Implementation Steps

1. **Create `api_fixture`** — extends `WebApplicationFactory<Program>`, overrides services to use in-memory SQLite with a unique database name per test run (prevents cross-test contamination).

2. **Create `database_seeder`** — helper that inserts a test restaurant, a time slot with `RemainingCapacity = 1`, and two test users into the test database.

3. **Create `describe_concurrent_booking`** — the BDD-named test class.

4. **Write the concurrency test** — authenticate two clients, fire concurrent requests with `Task.WhenAll`, assert results.

### Code Snippets

**`api_fixture.cs`:**
```csharp
// server/tests/IntegrationTests/Fixtures/api_fixture.cs
namespace TableNow.IntegrationTests.Fixtures;

public class api_fixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Use a unique in-memory SQLite database per test run
            var dbName = $"TestDb_{Guid.NewGuid():N}";
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbName};Mode=Memory;Cache=Shared")
                       .EnableSensitiveDataLogging());
        });

        builder.UseEnvironment("Testing");
    }

    public AppDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}
```

**`database_seeder.cs`:**
```csharp
// server/tests/IntegrationTests/Fixtures/database_seeder.cs
namespace TableNow.IntegrationTests.Fixtures;

public static class database_seeder
{
    public static async Task<(Guid slotId, Guid userId1, Guid userId2)> SeedConcurrencyScenario(
        AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        var restaurant = new RestaurantModel
        {
            Id = Guid.NewGuid(), Name = "Test Restaurant",
            Cuisine = "Test", Address = "1 Test St",
            Description = "Test", ThumbnailUrl = null,
        };

        var slot = new TimeSlotModel
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurant.Id,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Time = new TimeOnly(19, 0),
            TotalCapacity = 1,
            RemainingCapacity = 1, // only ONE seat available
        };

        var user1 = new UserModel
        {
            Id = Guid.NewGuid(), Name = "User One", Email = "user1@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass", 4), // low workFactor for tests
            Role = "diner", CreatedAt = DateTime.UtcNow,
        };

        var user2 = new UserModel
        {
            Id = Guid.NewGuid(), Name = "User Two", Email = "user2@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass", 4),
            Role = "diner", CreatedAt = DateTime.UtcNow,
        };

        db.AddRange(restaurant, slot, user1, user2);
        await db.SaveChangesAsync();

        return (slot.Id, user1.Id, user2.Id);
    }
}
```

**`describe_concurrent_booking.cs`:**
```csharp
// server/tests/IntegrationTests/Reservations/describe_concurrent_booking.cs
namespace describe_concurrent_booking;

public class when_two_users_book_the_last_slot : IClassFixture<api_fixture>
{
    private readonly api_fixture _factory;

    public when_two_users_book_the_last_slot(api_fixture factory)
        => _factory = factory;

    [Fact]
    public async Task it_should_allow_exactly_one_booking()
    {
        // Arrange
        using var db = _factory.CreateDbContext();
        var (slotId, userId1, userId2) = await database_seeder.SeedConcurrencyScenario(db);

        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();

        // Generate JWTs directly (or call login endpoint)
        client1.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateTestJwt(userId1));
        client2.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateTestJwt(userId2));

        var requestBody = JsonContent.Create(new { slotId = slotId, partySize = 1 });

        // Act — fire both requests simultaneously
        var (response1, response2) = await (
            client1.PostAsync("/api/v1/reservations", requestBody),
            client2.PostAsync("/api/v1/reservations", requestBody)
        ).WhenBoth();

        var statuses = new[] { (int)response1.StatusCode, (int)response2.StatusCode };

        // Assert
        statuses.Should().Contain(201);
        statuses.Should().Contain(409);
        statuses.Should().HaveCount(2);

        // Verify slot capacity was decremented exactly once
        await db.Entry(await db.TimeSlots.FindAsync(slotId)!).ReloadAsync();
        var slot = await db.TimeSlots.FindAsync(slotId);
        slot!.RemainingCapacity.Should().Be(0);
    }

    private static string CreateTestJwt(Guid userId)
    {
        // Build a minimal JWT for testing — matches the claim structure from task-06
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("test-secret-key-at-least-32-chars!!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "tablenow-api",
            audience: "tablenow-client",
            claims: new[] { new Claim("userId", userId.ToString()) },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// Helper to await two tasks and deconstruct the tuple
public static class TaskExtensions
{
    public static async Task<(T1, T2)> WhenBoth<T1, T2>(this (Task<T1> t1, Task<T2> t2) tasks)
    {
        await Task.WhenAll(tasks.t1, tasks.t2);
        return (tasks.t1.Result, tasks.t2.Result);
    }
}
```

**Run the test:**
```powershell
dotnet test server/tests/IntegrationTests --filter "FullyQualifiedName~describe_concurrent_booking"
```

### Key Packages Required in IntegrationTests.csproj

```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*" />
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="BCrypt.Net-Next" Version="4.*" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.*" />
```

## Acceptance Criteria

- [ ] The test runs with `dotnet test --filter "FullyQualifiedName~describe_concurrent_booking"`
- [ ] When two concurrent requests are made for the same slot with `RemainingCapacity = 1`, exactly one returns 201 and exactly one returns 409
- [ ] After the test, the slot's `RemainingCapacity` is 0 (decremented exactly once)
- [ ] The test uses `Task.WhenAll` to fire both requests simultaneously
- [ ] The test uses `api_fixture` (WebApplicationFactory) — not mocks

## Notes

- The JWT secret used in the test fixture must match the secret configured for the test environment. Override `appsettings.json` in `ConfigureWebHost` if needed: `builder.ConfigureAppConfiguration(c => c.AddInMemoryCollection(new Dictionary<string,string> { ["Jwt:Secret"] = "test-secret..." }))`.
- Use `workFactor: 4` in `database_seeder` for BCrypt — the default (12) is intentionally slow and would make tests take minutes.
- `IClassFixture<api_fixture>` reuses the same factory across all tests in the class; `IAsyncLifetime` can be added to reset DB state between tests if needed.
- The `TaskExtensions.WhenBoth` helper is a convenience — `Task.WhenAll` + indexing the array is equally valid.
