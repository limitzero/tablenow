# Requirements: Restaurant Detail & Availability — Frontend

## Summary

Once a diner clicks a restaurant card they need detailed information and the ability to check availability for their preferred date and party size. The `/restaurants/:id` route shows the full restaurant profile and a reactive slot-availability panel. Changing the date or party size triggers a fresh query to `GET /api/restaurants/{id}/slots?date=&partySize=` and re-renders the slot list. An empty state message is shown when no slots are available.

## Goals

- `/restaurants/:id` displays full restaurant info (name, description, cuisine, address).
- Date picker defaults to today + 1; changing it refreshes the slot list.
- Party size selector (1–20) defaults to 2; changing it refreshes the slot list.
- Slot list shows time and remaining seats for each available slot.
- Empty state message when no slots match.
- Each slot is tappable to proceed to booking (STORY-015 wires the confirmation step).

## Non-Goals

- No booking confirmation on this page — that is STORY-015.
- No reviews or photo gallery — those are Phase 3 (STORY-023/026).
- No map embed — only address text.

## Acceptance Criteria

- [ ] `/restaurants/:id` shows name, description, cuisine, and address.
- [ ] Date picker defaults to today + 1 and triggers slot refresh on change.
- [ ] Party size selector (1–20) triggers slot refresh on change.
- [ ] Each slot shows time and remaining seats.
- [ ] "No availability" message shown when slot list is empty.

## Assumptions

- STORY-012 added `restaurantRoutes` — this task adds `:id` to the same route array.
- `RestaurantsStore` from STORY-012 already holds the restaurant list; the detail component can look up by ID from the store or make a separate `GET /api/restaurants/{id}` call.
- STORY-011 (`GET /api/restaurants/{id}/slots`) is deployed and filtering correctly.

## Technical Constraints

- Slot query uses `httpResource()` keyed on `{ restaurantId, date, partySize }` — a new resource instance or a signal-driven URL that changes when inputs change.
- Reactive form with `date` and `partySize` FormControls; listen to `valueChanges` to trigger re-query.
- `OnPush` change detection; no `.subscribe()` in the component.
- Component lives in `client/src/app/features/restaurants/components/restaurant-detail/`.
