# Implementation Plan: Personal Bookmarks / Saved Restaurants

## Phase 1 — Entity and Store (parallel)

- [ ] **task-01-favorite-entity** — `UserFavorite` EF entity + migration (composite PK `UserId + RestaurantId`).
- [ ] **task-02-favorites-store** — `FavoritesStore` NgRx Signal Store with `favoriteIds: Set<string>`, `loadFavorites()`, and `toggle(restaurantId)` method (calls POST or DELETE based on current state).

## Phase 2 — Data handlers and UI (parallel)

- [ ] **task-03-favorite-data-handlers** — `AddFavoriteCommand`, `RemoveFavoriteCommand`, `GetFavoritesQuery` data handlers.
- [ ] **task-04-favorites-ui** — Heart icon on `RestaurantListComponent` cards + `MyFavoritesComponent` list on the user dashboard.

## Phase 3 — Endpoints

- [ ] **task-05-favorites-endpoints** — Application layer + `POST /api/favorites/{restaurantId}`, `DELETE /api/favorites/{restaurantId}`, `GET /api/favorites` endpoints (all require auth).
