# Implementation Plan: Reservation Cancellation — Backend

## Phase 1 — Data and Application layers (parallel)

- [ ] **task-01-cancel-reservation-command** — `CancelReservationCommand(ReservationId, RequestingUserId)` handler that loads reservation + slot, checks ownership (→ 403), checks already-cancelled (→ 409), sets `Status = Cancelled`, restores `RemainingCapacity`, saves once atomically.
- [ ] **task-02-cancel-reservation-application** — `CancelReservationRequest(ReservationId, RequestingUserId)` handler that dispatches the command and maps the result.

### Technical Details

Ownership check: `if (reservation.UserId != command.RequestingUserId) return Result.Failure(403, "Forbidden")`.
Already-cancelled check: `if (reservation.Status == ReservationStatus.Cancelled) return Result.Failure(409, "Reservation is already cancelled")`.
Atomic restore: `slot.RemainingCapacity += reservation.PartySize; reservation.Status = ReservationStatus.Cancelled; await db.SaveChangesAsync(ct)`.

## Phase 2 — Endpoint and BDD tests

- [ ] **task-03-cancel-endpoint** — `DELETE /api/reservations/{id}` endpoint with `userId` extraction from JWT, `ReservationsMapper` extension, and BDD tests: `describe_cancel_reservation` / `when_user_is_not_owner` / `it_should_return_forbidden`.
