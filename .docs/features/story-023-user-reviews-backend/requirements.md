# Requirements: User Reviews — Backend

## Summary

Diners can submit star ratings and text reviews for restaurants. Reviews are visible to all visitors on the restaurant detail page. Any registered user can submit a review (not limited to past diners). Ratings must be 1–5.

## Goals

- `POST /api/restaurants/{id}/reviews` accepts rating (1–5) and body text
- `GET /api/restaurants/{id}/reviews` returns paginated reviews, newest first
- `GET /api/restaurants/{id}` detail response includes recent reviews
- Rating outside 1–5 returns 400

## Acceptance Criteria

- [ ] Authenticated POST returns 201 with review data
- [ ] Rating < 1 or > 5 returns 400
- [ ] GET returns reviews sorted by timestamp descending
- [ ] Unauthenticated POST returns 401

## Technical Constraints

- `Review` entity in `Domain/Restaurants/`
- Any authenticated user can submit (not just diners with past reservations)
