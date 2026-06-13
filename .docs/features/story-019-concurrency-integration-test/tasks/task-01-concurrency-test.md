# Task 01: Concurrent Booking Integration Test

## Status

pending

## Wave

1

## Description

Create the `describe_concurrent_booking` integration test that fires two simultaneous `POST /api/reservations` requests for the same time slot, asserts exactly one 201 and one 409, and verifies the slot's `RemainingCapacity` was decremented by exactly one party size. Uses `WebApplicationFactory<Program>` and a real database to exercise EF Core's optimistic concurrency mechanism end-to-end.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** None

**Context from dependencies:** STORY-014 implemented `POST /api/reservations` with `DbUpdateConcurrencyException` handling on `TimeSlot.RowVersion`. The `api_fixture` base class in `tests/IntegrationTests/` extends `WebApplicationFactory<Program>` and exposes `Client` (authenticated `HttpClient`) and access to the test database. Two distinct JWT tokens (for `alice@tablenow.com` and `bob@tablenow.com` from seed data) are needed.

## Files to Create

- `server/tests/IntegrationTests/Reservations/describe_concurrent_booking.cs` — Integration test class.

## Files to Modify

- None.

## Technical Details

### Implementation Steps

1. In the test class, seed a `TimeSlot` with `RemainingCapacity = 2` and `partySize = 2` (so exactly one request can succeed).
2. Create two `HttpClient` instances, each authenticated with a different test JWT (Alice and Bob).
3. Fire both requests simultaneously with `Task.WhenAll`:
   ```csharp
   var (r1, r2) = await Task.WhenAll(
       aliceClient.PostAsJsonAsync("/api/reservations", new { slotId, partySize = 2 }),
       bobClient.PostAsJsonAsync("/api/reservations", new { slotId, partySize = 2 })
   );
   ```
4. Assert that the response status codes are exactly `{201, 409}` (order-insensitive).
5. Query the database and assert `slot.RemainingCapacity == 0` (not -2 or 2).

### Code Snippets

```csharp
namespace describe_concurrent_booking;

public class when_two_users_book_the_last_slot : api_fixture
{
    [Fact]
    public async Task it_should_allow_only_one_booking()
    {
        // Arrange
        var slotId = await SeedSlotWithCapacity(partySize: 2);
        var aliceClient = CreateClientWithJwt(aliceUserId);
        var bobClient = CreateClientWithJwt(bobUserId);

        // Act
        var (r1, r2) = await Task.WhenAll(
            aliceClient.PostAsJsonAsync("/api/reservations",
                new { slotId, partySize = 2 }),
            bobClient.PostAsJsonAsync("/api/reservations",
                new { slotId, partySize = 2 })
        );

        // Assert
        var statuses = new[] { (int)r1.StatusCode, (int)r2.StatusCode }
            .Order().ToArray();
        statuses.Should().BeEquivalentTo([201, 409]);

        var slot = await GetSlotFromDb(slotId);
        slot.RemainingCapacity.Should().Be(0);
    }
}
```

### Helper Notes

- `SeedSlotWithCapacity(int partySize)` seeds a restaurant + slot with `RemainingCapacity = partySize`, returns the slot ID.
- `CreateClientWithJwt(Guid userId)` creates a new `HttpClient` from the `WebApplicationFactory` with an `Authorization: Bearer <token>` header where the token contains the given `userId`.
- `GetSlotFromDb(Guid slotId)` queries the test database directly for the slot.

## Acceptance Criteria

- [ ] Exactly one response is 201 and one is 409.
- [ ] `TimeSlot.RemainingCapacity == 0` after both requests (decremented once, not twice).
- [ ] `dotnet test` runs and passes this test.
- [ ] Test uses a real database (no in-memory EF mocks).
- [ ] Test is in namespace `describe_concurrent_booking` with class `when_two_users_book_the_last_slot`.

## Notes

- SQLite in-memory with a shared named connection can be used for the test database — it supports concurrent connections within the same process. However, SQLite's concurrency token support differs from SQL Server's rowversion. If the test is intended to cover SQL Server-specific behavior, use Testcontainers.
- `Task.WhenAll` on the same thread does not guarantee true parallelism, but the EF DbContext lifecycle and `SaveChangesAsync` timing will produce the race condition in practice. For deterministic concurrency testing, use a `SemaphoreSlim` to synchronize both requests to the `SaveChangesAsync` boundary.
