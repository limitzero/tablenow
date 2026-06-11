# Requirements: Restaurant Listing — Backend

## Summary

Public endpoints for browsing restaurants. `GET /api/restaurants` returns all restaurants; `GET /api/restaurants/{id}` returns one. Both are unauthenticated. Data matches what the seed loaded in STORY-004.

## Goals

- List endpoint returns all restaurants with id, name, cuisine, address, description, thumbnailUrl
- Detail endpoint returns a single restaurant by id; 404 if not found
- Integration test verifies expected restaurant count

## Non-Goals

- No pagination (all restaurants in one response for MVP)
- No full-text search
- No auth required

## Acceptance Criteria

- [ ] `GET /api/restaurants` returns 200 with array of restaurants
- [ ] `GET /api/restaurants/{id}` returns 200 with single restaurant, 404 if not found
- [ ] Integration test in `describe_get_restaurants` verifies seeded count
- [ ] No authentication required on either endpoint

## Technical Constraints

- Query handler in `Data/Restaurants/Queries/`
- Application handler in `Application/Restaurants/Features/`
- No auth on restaurant endpoint group
