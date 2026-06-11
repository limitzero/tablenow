# Requirements: Restaurant Listing — Frontend

## Summary

The main browsing experience — a grid of restaurant cards with cuisine filter. The listing page is public (no login required to view). Clicking a card navigates to the detail page.

## Goals

- `/restaurants` shows grid of all restaurants as Material cards
- Cuisine filter (client-side) reduces visible cards
- Clicking a card navigates to `/restaurants/:id`
- NgRx Signal Store holds `restaurants`, `cuisineFilter`, computed `filteredRestaurants`

## Acceptance Criteria

- [ ] Grid of restaurant cards with name, cuisine, address
- [ ] Cuisine dropdown filters cards in real-time (client-side)
- [ ] Clicking a card navigates to `/restaurants/:id`
- [ ] Loading spinner shown while fetching

## Technical Constraints

- Feature folder: `client/src/app/features/restaurants/`
- NgRx Signal Store: `restaurants.store.ts`
- `httpResource()` for data fetching (no `.subscribe()` in component)
- OnPush on all components
