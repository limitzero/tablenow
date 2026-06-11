# Requirements: Restaurant Listing — Backend

## Summary

The frontend needs a public API to fetch all restaurants and individual restaurant details. Both endpoints are unauthenticated. The list endpoint returns all restaurants. The detail endpoint returns one restaurant by ID.

## Goals

- `GET /api/restaurants` returns 200 with array of all restaurants
- `GET /api/restaurants/{id}` returns 200 with single restaurant detail
- Both endpoints public (no auth required)
- Each restaurant includes id, name, cuisine, address, description, thumbnailUrl

## Acceptance Criteria

- [ ] `GET /api/restaurants` returns all seeded restaurants
- [ ] `GET /api/restaurants/{id}` with valid id returns single restaurant
- [ ] `GET /api/restaurants/{id}` with unknown id returns 404
- [ ] No auth required on either endpoint

## Technical Constraints

- Query handler in `Data/Restaurants/Queries/`
- Application handler in `Application/Restaurants/Features/`
- `RestaurantDto` in `Contracts/Restaurants/`
