# Requirements: User Reviews — Backend

## Summary

Diners want to share and read reviews to make informed dining decisions. This feature adds a `Review` entity, a `POST /api/restaurants/{id}/reviews` endpoint for authenticated users to submit a star rating (1–5) and text review, and a `GET /api/restaurants/{id}/reviews` endpoint returning paginated reviews sorted newest-first. Any registered user (not just past diners) may submit a review per the PRD.

## Goals

- `POST /api/restaurants/{id}/reviews` returns 201 for valid authenticated submission.
- Rating outside 1–5 returns 400.
- `GET /api/restaurants/{id}/reviews` returns reviews sorted by timestamp, newest first.
- Each review includes author name, rating, body, and timestamp.

## Non-Goals

- No review editing or deletion.
- No "helpful" voting.
- No photo attachments to reviews.
- No moderation queue.

## Acceptance Criteria

- [ ] `POST /api/restaurants/{id}/reviews` with valid rating and body returns 201.
- [ ] Rating outside 1–5 returns 400.
- [ ] `GET /api/restaurants/{id}/reviews` returns reviews sorted newest-first.
- [ ] Each review includes `authorName`, `rating`, `body`, `createdAt`.

## Technical Constraints

- `Review` entity: `Id`, `RestaurantId`, `UserId`, `Rating` (1–5), `Body`, `CreatedAt`.
- `GET /api/restaurants/{id}/reviews` supports `?page=&pageSize=` query params (default page 1, size 20).
- `POST` requires JWT auth; `GET` is public.
