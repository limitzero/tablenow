# Requirements: My Reservations Dashboard — Backend

## Summary

`GET /api/reservations/my` returns all reservations for the authenticated user. `userId` is read exclusively from JWT claims — never from request body. Empty array on no reservations. Each item includes restaurantName, date, time, partySize, status.

## Goals

- Returns array of user's reservations
- Empty array (not 404) when no reservations
- 401 for unauthenticated requests
- `userId` from JWT only

## Acceptance Criteria

- [ ] Valid JWT returns 200 with reservations array
- [ ] Empty array when user has no reservations
- [ ] 401 for unauthenticated requests
- [ ] Each item includes reservationId, restaurantName, date, time, partySize, status

## Technical Constraints

- `userId` from `ClaimTypes.NameIdentifier` (JWT `sub` claim)
- Query joins across Reservations + TimeSlots + Restaurants contexts (via separate queries in app handler, not EF cross-context join)
