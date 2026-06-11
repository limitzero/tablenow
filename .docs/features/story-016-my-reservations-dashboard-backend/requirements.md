# Requirements: My Reservations Dashboard — Backend

## Summary

Authenticated users need to see all their reservations. The endpoint joins Reservation, TimeSlot, and Restaurant tables to produce the display payload. The `userId` is always taken from JWT claims — never from the request URL or body to prevent access control bypass.

## Goals

- `GET /api/reservations/my` returns all reservations for JWT user
- Empty array `[]` returned when user has no reservations (not 404)
- Response includes reservationId, restaurantName, date, time, partySize, status

## Acceptance Criteria

- [ ] Returns all reservations for the authenticated user
- [ ] Returns 200 with `[]` when no reservations (not 404)
- [ ] Returns 401 for unauthenticated requests
- [ ] `userId` from JWT claims only

## Technical Constraints

- Query joins Reservation → TimeSlot → Restaurant
- Status values: "Confirmed" or "Cancelled"
- `MyReservationDto` in `Contracts/Reservations/`
