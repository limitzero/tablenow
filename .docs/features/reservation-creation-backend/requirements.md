# Requirements: Reservation Creation — Backend

## Summary

`POST /api/reservations` atomically creates a reservation and decrements `TimeSlot.RemainingCapacity`. Optimistic concurrency via `TimeSlot.RowVersion` prevents two simultaneous requests from both succeeding when only one seat remains. Auth required.

## Goals

- 201 on success with reservation details
- 409 when slot has insufficient remaining capacity
- Exactly one 201 + one 409 when two concurrent requests race for the last seat
- Auth required — 401 for unauthenticated requests

## Non-Goals

- No waitlist
- No group reservation split across slots

## Acceptance Criteria

- [ ] `POST /api/reservations` with valid slotId + partySize returns 201
- [ ] Insufficient capacity returns 409 with "This time slot is no longer available."
- [ ] Concurrent requests: exactly one 201, one 409
- [ ] `TimeSlot.RemainingCapacity` decremented in same transaction as reservation insert
- [ ] 401 for unauthenticated requests

## Technical Constraints

- EF Core optimistic concurrency: `[Timestamp]` on `TimeSlot.RowVersion`
- On `DbUpdateConcurrencyException`: re-read slot — if still insufficient → 409
- Command: `CreateReservationCommand` in `Data/Reservations/Commands/`
