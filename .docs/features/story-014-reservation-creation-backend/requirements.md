# Requirements: Reservation Creation — Backend

## Summary

Diners book a time slot by posting `{slotId, partySize}`. The handler decrements `TimeSlot.RemainingCapacity` in the same transaction that creates the `Reservation` row. Optimistic concurrency via the `RowVersion` column ensures two simultaneous requests for the same last seat both read a valid `RemainingCapacity`, but only one write succeeds — the loser gets a `DbUpdateConcurrencyException`, re-reads the slot, confirms it's now insufficient, and returns 409.

## Goals

- `POST /api/reservations` returns 201 with reservation details on success
- 409 when `RemainingCapacity < partySize`
- 409 when two simultaneous requests race — exactly one succeeds
- `RemainingCapacity` decremented atomically with reservation insert
- 401 for unauthenticated requests

## Acceptance Criteria

- [ ] Valid request returns 201 with reservationId, restaurantName, dateTime, partySize, status
- [ ] 409 when slot is full: "This time slot is no longer available."
- [ ] Two concurrent requests → exactly one 201 and one 409
- [ ] `RemainingCapacity` is decremented by partySize in a DB transaction
- [ ] `userId` read from JWT claims — never from request body

## Technical Constraints

- `TimeSlot.RowVersion` is the concurrency token (configured via `IsRowVersion()` in STORY-003)
- Catch `DbUpdateConcurrencyException`, re-read slot, return 409 if still insufficient
- Application handler reads `userId` from `IHttpContextAccessor`
- Endpoint in the `/api/reservations` group that has `.RequireAuthorization()`
