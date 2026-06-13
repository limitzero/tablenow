# Requirements: My Reservations Dashboard — Frontend

## Summary

Authenticated diners need a single place to review all their past and upcoming reservations and to cancel confirmed bookings they no longer need. The `/reservations` route is guarded by `AuthGuard`; unauthenticated users are redirected to `/login`. Reservations are fetched from `GET /api/reservations/my`. A cancel action shows a confirmation prompt before issuing the `DELETE` request. On successful cancellation the reservation's status updates in the store without a full page reload.

## Goals

- `/reservations` shows all user reservations with restaurant name, date, time, party size, and status badge.
- "Cancel" button on confirmed reservations shows a confirmation prompt.
- Successful cancellation updates the reservation status to "Cancelled" in-place.
- Route requires authentication (AuthGuard redirect to `/login`).

## Non-Goals

- No booking modification (change date/party size).
- No reservation creation from this page — only listing and cancellation.
- No search or filter — shows all reservations.

## Acceptance Criteria

- [ ] `/reservations` shows all reservations with correct fields and status badge.
- [ ] Cancel button triggers confirmation prompt before sending DELETE.
- [ ] Successful cancellation updates status badge to "Cancelled" without reload.
- [ ] Unauthenticated access redirects to `/login`.

## Assumptions

- STORY-009 `AuthGuard` is available in `core/guards/`.
- STORY-016 `GET /api/reservations/my` endpoint returns `[{ reservationId, restaurantName, date, time, partySize, status }]`.
- STORY-017 `DELETE /api/reservations/{id}` is available.

## Technical Constraints

- Feature folder: `client/src/app/features/reservations/`.
- NgRx Signal Store slice: `reservations.store.ts` holds `reservations: Reservation[]`.
- Angular Material table or list; status shown as `<mat-chip>` with color based on status.
- `OnPush` change detection; no `.subscribe()` in components; `inject()` for DI.
