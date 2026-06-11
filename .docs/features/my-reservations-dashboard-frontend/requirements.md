# Requirements: My Reservations Dashboard — Frontend

## Summary

The `/reservations` route shows all reservations for the signed-in user. Each row shows restaurant name, date, time, party size, and a status badge. A Cancel button prompts confirmation before issuing the DELETE. Route is guarded — unauthenticated users are redirected to `/login`.

## Goals

- List of user's reservations at `/reservations`
- Status badge (Confirmed green, Cancelled grey)
- Cancel button on Confirmed reservations with prompt
- Auth guard redirects unauthenticated users

## Non-Goals

- No re-booking from cancelled reservation
- No pagination

## Acceptance Criteria

- [ ] `/reservations` shows reservations with status badges
- [ ] Cancel button prompts before calling DELETE
- [ ] Successful cancel updates status in UI
- [ ] Unauthenticated → redirect to `/login`

## Technical Constraints

- Feature folder: `client/src/app/features/reservations/`
- NgRx Signal Store slice: `reservations.store.ts`
- Route guarded by `authGuard` from STORY-009
