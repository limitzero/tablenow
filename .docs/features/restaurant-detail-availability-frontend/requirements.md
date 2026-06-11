# Requirements: Restaurant Detail & Availability — Frontend

## Summary

The `/restaurants/:id` route shows full restaurant info and a dynamic slot list. Date and party-size controls trigger re-fetches via `httpResource()`. When no slots match, a "No availability" message is shown.

## Goals

- Restaurant info (name, description, cuisine, address) displayed
- Date defaults to today + 1
- Party size selector (1–20) with immediate slot refresh
- "No availability" message when list is empty

## Non-Goals

- No booking from this page (that's STORY-015)
- No reviews section (STORY-024)
- No photo gallery (STORY-026)

## Acceptance Criteria

- [ ] Restaurant info shown at `/restaurants/:id`
- [ ] Date picker defaults to tomorrow; changing it refreshes slots
- [ ] Party size change refreshes slots
- [ ] "No availability" message when slot list is empty

## Technical Constraints

- Slot list updates via `httpResource()` keyed on `{date, partySize}`
- Belongs to `features/restaurants/` detail component + route
- Reactive form with `date` and `partySize` controls
