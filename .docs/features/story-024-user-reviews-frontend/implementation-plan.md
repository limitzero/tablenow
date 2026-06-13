# Implementation Plan: User Reviews — Frontend

## Phase 1 — Service and Models

- [ ] **task-01-review-service-models** — `Review` TypeScript interface + `ReviewsService` (GET reviews via `httpResource`, POST via `HttpClient`).

## Phase 2 — List and Form (parallel — different template sections)

- [ ] **task-02-review-list-component** — Add reviews list section to `RestaurantDetailComponent` using `httpResource` for reviews.
- [ ] **task-03-review-form-component** — Add submission form (star rating + text area) shown only when `authService.isAuthenticated()`.
