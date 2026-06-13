# Implementation Plan: User Reviews — Backend

## Phase 1 — Entity and Migration

- [ ] **task-01-review-entity-migration** — `Review` domain entity in `Domain/Restaurants/Entities/Review.cs`, EF model in `Data/Restaurants/Models/`, Fluent config, and EF migration.

## Phase 2 — Data and Application layers (parallel)

- [ ] **task-02-review-data-handlers** — `CreateReviewCommand` + handler (inserts review) and `GetReviewsQuery(restaurantId, page, pageSize)` + handler (returns paged, newest-first).
- [ ] **task-03-review-application-layer** — `SubmitReviewRequest` handler (validates rating 1–5, dispatches command) and `GetReviewsRequest` handler.

## Phase 3 — Endpoints

- [ ] **task-04-review-endpoints** — `POST /api/restaurants/{id}/reviews` (requires auth) and `GET /api/restaurants/{id}/reviews` (public) with `ReviewsMapper`.
