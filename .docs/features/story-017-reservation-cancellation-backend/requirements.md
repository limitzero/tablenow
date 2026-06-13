# Requirements: Reservation Cancellation — Backend

## Summary

Diners need the ability to cancel reservations they no longer intend to keep so that the freed capacity is available for others. This feature adds `DELETE /api/reservations/{id}` which sets `Reservation.Status = Cancelled` and atomically restores `TimeSlot.RemainingCapacity` by `PartySize` within a single database transaction. Only the reservation owner (JWT `userId` == reservation `UserId`) may cancel; other users receive 403. Attempting to cancel an already-cancelled reservation returns 409.

## Goals

- `DELETE /api/reservations/{id}` returns 200 and sets status to `Cancelled`.
- `TimeSlot.RemainingCapacity` is restored by `PartySize` atomically.
- Ownership check: JWT `userId` must match the reservation's `UserId`; otherwise 403.
- Double cancellation returns 409 Conflict.
- BDD tests: `describe_cancel_reservation` → `when_user_is_not_owner` → `it_should_return_forbidden`.

## Non-Goals

- No partial cancellation (changing party size).
- No "undo" cancellation — once cancelled, permanently cancelled.
- No email notification on cancellation (out of MVP scope).
- No admin override.

## Acceptance Criteria

- [ ] `DELETE /api/reservations/{id}` by the owner returns 200 and status becomes `Cancelled`.
- [ ] `TimeSlot.RemainingCapacity` is restored within the same DB transaction.
- [ ] A different user's JWT returns 403 Forbidden.
- [ ] Cancelling an already-cancelled reservation returns 409 Conflict.
- [ ] BDD test `when_user_is_not_owner` → `it_should_return_forbidden` passes.

## Assumptions

- STORY-016 created `GetMyReservationsQuery` and the `Reservation` EF model is accessible in `ReservationsDbContext`.
- STORY-007 applied `.RequireAuthorization()` to the reservations route group.
- The JWT `sub` claim holds the `userId` Guid string.

## Technical Constraints

- Command: `CancelReservationCommand` / `CancelReservationCommandHandler` in `Data/Reservations/Commands/CancelReservation/`.
- Application handler: `CancelReservationRequest` / `CancelReservationRequestHandler` in `Application/Reservations/Features/CancelReservation/`.
- Authorization check at the Application layer (not at the endpoint): compare `request.RequestingUserId` with `reservation.UserId`.
- Atomic: load reservation + slot, update both, `SaveChangesAsync` once — no double-save.
