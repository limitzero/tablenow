# Requirements: Concurrency Integration Test

## Summary

Proves the double-booking prevention works end-to-end. Two concurrent HTTP requests for the same last-capacity slot are fired via `Task.WhenAll`. Exactly one must get 201; the other must get 409. Uses a real database (Testcontainers SQL Server or SQLite in-memory for CI).

## Goals

- Integration test fires two simultaneous POST /api/reservations
- Exactly one 201 and one 409
- TimeSlot.RemainingCapacity decremented by exactly one partySize
- `dotnet test` includes and passes this test

## Acceptance Criteria

- [ ] `describe_concurrent_booking` / `when_two_users_book_the_last_slot` test exists
- [ ] `Task.WhenAll` fires two simultaneous requests
- [ ] Exactly one 201, one 409
- [ ] Final RemainingCapacity = 0 (decremented once only)

## Technical Constraints

- Extend `api_fixture` (WebApplicationFactory<Program>)
- Consider Testcontainers for SQL Server; SQLite acceptable for CI speed
- BDD naming: `describe_concurrent_booking` namespace, `when_two_users_book_the_last_slot` class
