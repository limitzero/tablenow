# Requirements: Reservation Cancellation — Backend

## Summary

Diners can cancel their own confirmed reservations. Cancelling restores the time slot capacity so other diners can book it. Cancellation by a different user returns 403; cancelling an already-cancelled reservation returns 409.

## Goals

- `DELETE /api/reservations/{id}` cancels reservation and restores slot capacity
- Ownership check: JWT userId must match reservation userId
- Already-cancelled returns 409
- Not owner returns 403

## Acceptance Criteria

- [ ] Valid DELETE by owner returns 200
- [ ] `TimeSlot.RemainingCapacity` increased by partySize in same transaction
- [ ] Different user's JWT returns 403
- [ ] Already-cancelled reservation returns 409
- [ ] Unauthenticated returns 401

## Technical Constraints

- Status update + capacity restore in one DB transaction
- `userId` from JWT claims only
