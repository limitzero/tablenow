# Requirements: Popularity Rankings & Favorites Section

## Summary

Diners discover trending restaurants through a "Most Booked" section. The ranking aggregates confirmed reservation counts within a time window. Results are cached to avoid re-aggregating on every request.

## Goals

- `GET /api/restaurants/popular?period=week|month` returns top-N restaurants by booking count
- Results cached for 1 hour via IMemoryCache
- Frontend section on restaurant listing page with period toggle

## Acceptance Criteria

- [ ] Returns restaurants ranked by confirmed reservation count
- [ ] `period=week` counts last 7 days; `period=month` counts last 30 days
- [ ] Results cached for 1 hour
- [ ] No auth required

## Technical Constraints

- Aggregation query on Reservations table
- Index on Reservations(RestaurantId, CreatedAt) recommended
- Top 10 restaurants returned
