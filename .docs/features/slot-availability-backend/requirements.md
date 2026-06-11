# Requirements: Slot Availability — Backend

## Summary

`GET /api/restaurants/{id}/slots?date=YYYY-MM-DD&partySize=N` returns time slots for a restaurant on a given date with sufficient remaining capacity. Filtering is done in EF LINQ — no in-memory filtering allowed.

## Goals

- Returns slots where `RemainingCapacity >= partySize` for the given date
- Returns 400 for missing/invalid date or partySize
- Fully booked slots excluded from results
- Response time < 300ms

## Non-Goals

- No multi-day range queries
- No wait-list functionality

## Acceptance Criteria

- [ ] Valid query returns slots with sufficient capacity
- [ ] Fully booked slots excluded
- [ ] Missing date or partySize returns 400
- [ ] Filter applied in EF LINQ (not in-memory)
- [ ] BDD test: `describe_get_available_slots` / `when_party_size_exceeds_remaining_capacity`

## Technical Constraints

- Query: `GetAvailableSlotsQuery(restaurantId, date, partySize)` in `Data/Restaurants/Queries/`
- Filter in LINQ: `Where(s => s.RestaurantId == id && s.SlotDateTime.Date == date && s.RemainingCapacity >= partySize)`
