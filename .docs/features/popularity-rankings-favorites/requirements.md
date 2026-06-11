# Requirements: Popularity Rankings & Favorites Section

## Summary

Backend: `GET /api/restaurants/popular?period=week|month&locale=` returns top-N restaurants ranked by confirmed booking count. Frontend: "Most Booked" section on the home/landing page. Rankings refreshed at least daily via cache or scheduled job.

## Goals

- Popular restaurants endpoint with period and locale filters
- Rankings cached or refreshed daily
- Home page shows "Most Booked" section

## Acceptance Criteria

- [ ] `GET /api/restaurants/popular?period=week` returns ranked list
- [ ] Rankings computed from confirmed reservations count
- [ ] Frontend home page shows the section
- [ ] Rankings refreshed at least daily

## Technical Constraints

- Index on `Reservations(RestaurantId, CreatedAt)` for aggregation performance
- Cache or scheduled job refresh
