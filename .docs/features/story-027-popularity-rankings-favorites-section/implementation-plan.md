# Implementation Plan: Popularity Rankings & Favorites Section

## Phase 1 — Backend and Frontend (parallel)

- [ ] **task-01-popular-backend** — `GetPopularRestaurantsQuery(period, locale)` data handler with aggregation query + Application handler + `GET /api/restaurants/popular` endpoint + in-memory response caching (`IMemoryCache`).
- [ ] **task-02-popular-frontend** — `TrendingService` using `httpResource` + "Trending This Week/Month" section added to the restaurant listing page, with a period toggle.
