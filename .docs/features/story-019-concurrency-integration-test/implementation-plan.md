# Implementation Plan: Concurrency Integration Test

## Phase 1 — Concurrent Booking Test

Single phase — one test file.

- [ ] **task-01-concurrency-test** — Create the `describe_concurrent_booking` test class under `tests/IntegrationTests/` that seeds a slot with `RemainingCapacity == partySize`, fires two concurrent `POST /api/reservations` requests with `Task.WhenAll`, asserts exactly one 201 and one 409, and verifies the slot capacity in the database.

### Technical Details

```csharp
namespace describe_concurrent_booking;

public class when_two_users_book_the_last_slot : api_fixture
{
    [Fact]
    public async Task it_should_allow_only_one_booking()
    {
        // Seed a slot with RemainingCapacity = 2 (partySize = 2)
        // Create JWTs for two test users
        var (r1, r2) = await Task.WhenAll(
            Client.PostAsJsonAsync("/api/reservations", new { slotId = slotId, partySize = 2 }),
            Client.PostAsJsonAsync("/api/reservations", new { slotId = slotId, partySize = 2 })
        );

        var statuses = new[] { (int)r1.StatusCode, (int)r2.StatusCode }.OrderBy(s => s).ToArray();
        statuses.Should().BeEquivalentTo([201, 409]);

        // Verify DB: RemainingCapacity = 0 (not -2)
        var slot = await Db.TimeSlots.FindAsync(slotId);
        slot!.RemainingCapacity.Should().Be(0);
    }
}
```

The `api_fixture` base class extends `WebApplicationFactory<Program>`, exposes `Client` (HttpClient) and `Db` (test DbContext). Seed the slot in the test constructor or a `[Before]` hook with `RemainingCapacity = partySize`.
