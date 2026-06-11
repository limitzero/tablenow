# Requirements: Reservation Cancellation — Backend

## Summary

`DELETE /api/reservations/{id}` sets status to `Cancelled` and restores `TimeSlot.RemainingCapacity` atomically. Ownership check: 403 if requester is not the reservation owner. Already-cancelled returns 409.

## Goals

- 200 on successful cancellation
- Capacity restored atomically in same transaction
- 403 if JWT user is not the reservation owner
- 409 if already cancelled

## Acceptance Criteria

- [ ] Owner can cancel → 200, status = Cancelled, capacity restored
- [ ] Non-owner → 403
- [ ] Already cancelled → 409
- [ ] Capacity restored by `partySize` in same transaction

## Technical Constraints

- Command: `CancelReservationCommand` in `Data/Reservations/Commands/`
- BDD test: `describe_cancel_reservation` / `when_user_is_not_owner` / `it_should_return_forbidden`
