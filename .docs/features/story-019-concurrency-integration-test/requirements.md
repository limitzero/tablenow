# Requirements: Concurrency Integration Test

## Summary

The double-booking prevention in STORY-014 must be provably correct. A unit test cannot verify it — the `DbUpdateConcurrencyException` path requires a real database with row-version locking. This integration test fires two simultaneous requests and verifies the invariant: exactly one booking succeeds and the slot capacity is decremented by exactly one party size.

## Goals

- Two concurrent POST /api/reservations for same slot → exactly one 201 and one 409
- `TimeSlot.RemainingCapacity` decremented by exactly one party size (not two)
- Test runs as part of `dotnet test`

## Acceptance Criteria

- [ ] `Task.WhenAll` fires two requests simultaneously
- [ ] Exactly one response is 201 Created
- [ ] Exactly one response is 409 Conflict
- [ ] After the test, slot `RemainingCapacity` reflects exactly one booking

## Technical Constraints

- `WebApplicationFactory<Program>` for the test host
- Use SQLite in-memory for speed (or Testcontainers SQL Server for full fidelity)
- Must authenticate test users to get valid JWTs
