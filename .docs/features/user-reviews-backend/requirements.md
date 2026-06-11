# Requirements: User Reviews — Backend

## Summary

Authenticated users can submit star ratings (1–5) and text reviews for restaurants. Reviews are listed newest-first. Any registered user can submit (not just past diners).

## Goals

- `POST /api/restaurants/{id}/reviews` creates a review (auth required)
- `GET /api/restaurants/{id}/reviews` returns paginated reviews newest-first
- Rating outside 1–5 returns 400

## Acceptance Criteria

- [ ] Valid review POST returns 201
- [ ] Rating outside 1–5 returns 400
- [ ] Reviews returned newest-first
- [ ] Auth required for POST; GET is public

## Technical Constraints

- `Review` entity: `Id, RestaurantId, UserId, Rating (int), Body, CreatedAt`
- New EF migration for `Reviews` table
