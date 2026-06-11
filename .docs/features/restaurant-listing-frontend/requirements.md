# Requirements: Restaurant Listing — Frontend

## Summary

The `/restaurants` route displays a grid of restaurant cards with name, cuisine, and address. A cuisine filter control lets users narrow the list client-side. Clicking a card navigates to the detail page. Data is fetched with `httpResource()`.

## Goals

- Grid of restaurant cards at `/restaurants`
- Cuisine filter (client-side, no re-fetch)
- Click navigates to `/restaurants/:id`
- NgRx Signal Store holds restaurant list and active filter

## Non-Goals

- No server-side search or pagination
- No map view

## Acceptance Criteria

- [ ] `/restaurants` shows all restaurants as cards
- [ ] Cuisine filter hides non-matching cards instantly
- [ ] Clicking a card navigates to `/restaurants/:id`
- [ ] `npm run build` exits with code 0

## Technical Constraints

- Feature folder: `client/src/app/features/restaurants/`
- NgRx Signal Store slice: `restaurants.store.ts`
- `httpResource()` for data fetching — no `.subscribe()`
- Angular Material cards; `@for` to iterate
