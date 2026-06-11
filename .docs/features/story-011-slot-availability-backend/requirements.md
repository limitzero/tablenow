# Requirements: Slot Availability — Backend

## Summary

Diners need to see which time slots are available for a given restaurant, date, and party size. The query must filter at the database level (not load all slots then filter in memory) and must exclude slots where `RemainingCapacity < partySize`. Response time target is under 300ms.

## Goals

- `GET /api/restaurants/{id}/slots?date=&partySize=` returns available slots
- Filters `RemainingCapacity >= partySize` in EF LINQ (not in-memory)
- Returns 400 if `date` or `partySize` is missing or invalid
- Returns `[{slotId, time, remainingCapacity}]`

## Acceptance Criteria

- [ ] Slots with `RemainingCapacity = 0` excluded from results when any `partySize` is queried
- [ ] Slots with `RemainingCapacity < partySize` excluded
- [ ] Missing date or partySize returns 400
- [ ] partySize outside 1–20 returns 400

## Technical Constraints

- Filter in EF LINQ expression — no `.AsEnumerable()` before the where clause
- `SlotDto` in `Contracts/Restaurants/`
- Query in `Data/Restaurants/Queries/`, Application handler in `Application/Restaurants/Features/GetAvailableSlots/`
