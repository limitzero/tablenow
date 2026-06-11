# Task 02: Concurrent Booking Test

## Status

pending

## Wave

2

## Description

Implements the `describe_concurrent_booking` integration test. Uses `Task.WhenAll` to fire two simultaneous `POST /api/reservations` requests for the same slot with `RemainingCapacity = 1`. Asserts exactly one 201 and one 409. Verifies `RemainingCapacity` is 0 after the test.

## Dependencies

**Depends on:** task-01-test-infra.md
**Blocks:** Nothing

**Context from dependencies:**
- task-01 created `api_fixture` base class with `WebApplicationFactory<Program>` and SQLite test databases
- The full API (auth, reservations, slots) is available in-process
- Two test users need JWT tokens — obtained by calling `POST /api/auth/login` in the test setup with seeded credentials (alice@test.com / Password1!)

## Files to Create

- `server/tests/IntegrationTests/Reservations/describe_concurrent_booking/when_two_users_book_the_last_slot.cs`

## Technical Details

### Code Snippets

```csharp
// describe_concurrent_booking/when_two_users_book_the_last_slot.cs
using CM.TableNow.IntegrationTests.Fixtures;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace describe_concurrent_booking;

public class when_two_users_book_the_last_slot(api_fixture fixture)
    : IClassFixture<api_fixture>, IAsyncLifetime
{
    private HttpClient _client1 = null!;
    private HttpClient _client2 = null!;
    private Guid _slotId;

    public async Task InitializeAsync()
    {
        _client1 = fixture.CreateClient();
        _client2 = fixture.CreateClient();

        // Login as alice and attach JWT to both clients
        var loginResp = await _client1.PostAsJsonAsync("/api/auth/login",
            new { email = "alice@test.com", password = "Password1!" });
        var token = (await loginResp.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
        _client1.DefaultRequestHeaders.Authorization = new("Bearer", token);
        _client2.DefaultRequestHeaders.Authorization = new("Bearer", token);

        // Find a slot with RemainingCapacity = 1 (or set one up via seed)
        var slots = await _client1.GetFromJsonAsync<SlotResponse[]>(
            $"/api/restaurants/{SomeRestaurantId}/slots?date={DateTime.Today.AddDays(1):yyyy-MM-dd}&partySize=1");
        _slotId = slots!.First().SlotId;

        // Force RemainingCapacity to 1 directly in DB
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReservationsDbContext>();
        var slot = await db.TimeSlots.FindAsync(_slotId);
        slot!.RemainingCapacity = 1;
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task it_should_allow_exactly_one_booking()
    {
        var request1 = _client1.PostAsJsonAsync("/api/reservations",
            new { timeSlotId = _slotId, partySize = 1 });
        var request2 = _client2.PostAsJsonAsync("/api/reservations",
            new { timeSlotId = _slotId, partySize = 1 });

        var responses = await Task.WhenAll(request1, request2);

        var statusCodes = responses.Select(r => r.StatusCode).ToList();
        statusCodes.Should().Contain(HttpStatusCode.Created);
        statusCodes.Should().Contain(HttpStatusCode.Conflict);

        // Verify capacity is now 0
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReservationsDbContext>();
        var slot = await db.TimeSlots.FindAsync(_slotId);
        slot!.RemainingCapacity.Should().Be(0);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

file record LoginResponse(string Token);
file record SlotResponse(Guid SlotId, string Time, int RemainingCapacity);
```

## Acceptance Criteria

- [ ] Test class exists in namespace `describe_concurrent_booking`
- [ ] `Task.WhenAll` fires two simultaneous POST requests
- [ ] Exactly one 201 (Created) and one 409 (Conflict) in the responses
- [ ] After test, `TimeSlot.RemainingCapacity = 0`
- [ ] `dotnet test` includes and passes this test
