# Requirements: My Reservations Dashboard — Frontend

## Summary

Authenticated diners can see all their reservations in one place and cancel upcoming ones. The route must redirect unauthenticated users to `/login`. A status chip distinguishes Confirmed from Cancelled reservations. The cancel flow uses a confirmation dialog to prevent accidental cancellations.

## Goals

- `/reservations` route guarded by `authGuard`
- List shows restaurantName, date, time, partySize, status chip
- Cancel button on Confirmed rows → confirmation prompt → DELETE → status update
- NgRx Signal Store holds reservations state

## Acceptance Criteria

- [ ] Authenticated user sees their reservations
- [ ] Unauthenticated access redirects to /login
- [ ] Status chips: Confirmed=green, Cancelled=gray
- [ ] Cancel button only visible on Confirmed reservations
- [ ] Cancel prompts confirmation before calling DELETE
- [ ] After successful cancel, reservation status updates in the list

## Technical Constraints

- Feature folder: `client/src/app/features/reservations/`
- NgRx Signal Store: `reservations.store.ts`
- `authGuard` from STORY-009 applied to the route
