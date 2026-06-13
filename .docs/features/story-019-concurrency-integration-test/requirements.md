# Requirements: Concurrency Integration Test

## Summary

The optimistic concurrency mechanism in STORY-014 must be demonstrably correct. This story adds an integration test that fires two simultaneous `POST /api/reservations` requests for the same time slot with exactly enough remaining capacity for one party, then asserts that exactly one request succeeds (201) and the other fails (409). The test also verifies that `TimeSlot.RemainingCapacity` was decremented by exactly one party size, not two.

The test extends `api_fixture` (`WebApplicationFactory<Program>`) and uses a real test database so the EF Core concurrency token behavior is exercised exactly as it would be in production.

## Goals

- Two concurrent requests for the same last-capacity slot yield exactly one 201 and one 409.
- `TimeSlot.RemainingCapacity` is decremented by exactly one `partySize`, not two.
- Test is included in the `dotnet test` run and passes reliably.
- Test extends `api_fixture` and uses a real (SQLite or Testcontainers SQL Server) database.

## Non-Goals

- No load or stress test — just two concurrent requests.
- No test for three or more concurrent requests.
- No test for the happy path (single booking) — that is covered in STORY-014's BDD tests.

## Acceptance Criteria

- [ ] Exactly one response is 201 and one is 409 when two requests race for the last slot.
- [ ] `TimeSlot.RemainingCapacity` after both requests equals `initial - partySize` (not `initial - 2*partySize`).
- [ ] `dotnet test` runs the test and it passes.
- [ ] Test uses a real database (not mocked EF).

## Assumptions

- STORY-014 has implemented the optimistic concurrency logic.
- A test database (SQLite in-memory with a shared connection, or Testcontainers) is available.
- The test infrastructure (`api_fixture`, `WebApplicationFactory<Program>`) from the project test base classes is set up.

## Technical Constraints

- Test class: `describe_concurrent_booking` / `when_two_users_book_the_last_slot`.
- Use `Task.WhenAll` to fire two HTTP requests concurrently.
- Each request uses a distinct valid JWT (two different test users).
- The test slot is seeded with `RemainingCapacity == partySize` so only one request can succeed.
