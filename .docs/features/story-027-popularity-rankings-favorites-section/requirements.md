# Requirements: Popularity Rankings & Favorites Section

## Summary

Diners want to discover trending restaurants without having to browse the full list. This feature adds a "Most Booked" home page section backed by a `GET /api/restaurants/popular?period=week|month&locale=` endpoint that counts confirmed reservations within the requested period. The frontend renders the top-N results in a visually distinct section on the home/landing page.

## Goals

- `GET /api/restaurants/popular` returns top-N restaurants ranked by booking count.
- Supports `period=week|month` and optional `locale` filter.
- Results are cached or refreshed at least daily.
- Frontend shows a "Trending" section with the top restaurants.

## Non-Goals

- No real-time ranking updates.
- No personalized recommendations.
- No admin UI for featured restaurants.

## Acceptance Criteria

- [ ] `GET /api/restaurants/popular?period=week` returns restaurants ranked by booking count.
- [ ] `GET /api/restaurants/popular?period=month` works correctly.
- [ ] Frontend "Trending" section renders the ranked list.
- [ ] Results are cached (in-memory or via a background job) to avoid expensive repeated aggregations.

## Technical Constraints

- Aggregation query: group `Reservations` by `RestaurantId` where `Status = Confirmed` and `CreatedAt` within the period.
- Add index on `(RestaurantId, CreatedAt)` in the Reservations table.
- Frontend: new section on the `RestaurantListComponent` or a dedicated home page component.
- `OnPush` change detection; `httpResource()` for data.
