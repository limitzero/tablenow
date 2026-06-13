# Requirements: Reservation Creation — Backend

## Summary

Diners need to be able to book a table at a restaurant. This feature adds `POST /api/reservations` which creates a reservation for the authenticated user and atomically decrements `TimeSlot.RemainingCapacity`. To prevent double-booking, EF Core optimistic concurrency is used on the `TimeSlot.RowVersion` token: if two requests compete for the last seat, a `DbUpdateConcurrencyException` is caught, the slot is re-read, and if capacity is still insufficient a 409 Conflict is returned.

The endpoint requires a valid JWT (enforced by STORY-007). The `userId` is extracted from JWT claims — never from the request body. A 201 Created response is returned with reservation details on success.

## Goals

- `POST /api/reservations` with valid `slotId` and `partySize` returns 201 with reservation details.
- A slot with insufficient `RemainingCapacity` returns 409 Conflict.
- Two concurrent requests for the same last-capacity slot: exactly one 201, one 409.
- `TimeSlot.RemainingCapacity` is decremented atomically in the same transaction as reservation creation.
- Unauthenticated requests return 401.

## Non-Goals

- No reservation modification — only creation and cancellation (STORY-017).
- No waitlist functionality.
- No email notification on creation (Phase 2 — STORY-021).
- No admin override of capacity.

## Acceptance Criteria

- [ ] `POST /api/reservations` with valid `slotId` and `partySize` returns 201 with `reservationId`, `slotId`, `partySize`, `status`.
- [ ] A booking attempt when `RemainingCapacity < partySize` returns 409 "This time slot is no longer available."
- [ ] Two simultaneous requests for the same last-capacity slot produce exactly one 201 and one 409.
- [ ] `TimeSlot.RemainingCapacity` is decremented by `partySize` within the same DB transaction.
- [ ] An unauthenticated request returns 401.

## Assumptions

- STORY-003 created the `TimeSlot` EF model with `RowVersion` (`byte[]`) configured as a concurrency token.
- STORY-007 has applied `.RequireAuthorization()` to the reservations route group.
- The JWT `sub` claim holds the `userId` as a `Guid` string — the endpoint extracts it from `HttpContext.User`.

## Technical Constraints

- Handler: `CreateReservationRequest` / `CreateReservationRequestHandler` in `Application/Reservations/Features/CreateReservation/`.
- Data command: `CreateReservationCommand` / `CreateReservationCommandHandler` in `Data/Reservations/Commands/CreateReservation/`.
- On `DbUpdateConcurrencyException`: reload the slot, check capacity again, return 409 if still insufficient.
- `userId` extracted from `ClaimsPrincipal` at the endpoint — passed down as part of the command.
- BDD test: `describe_create_reservation` → `when_slot_is_fully_booked` → `it_should_return_conflict_status`.
- File-scoped namespaces, nullable enabled, `CancellationToken` on all async methods.
