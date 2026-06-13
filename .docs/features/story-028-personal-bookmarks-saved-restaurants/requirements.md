# Requirements: Personal Bookmarks / Saved Restaurants

## Summary

Diners want to quickly return to restaurants they've enjoyed or are considering. This feature adds a bookmark/heart toggle on restaurant cards and the detail page. Saved restaurants appear in a "My Favorites" section on the user dashboard. Unauthenticated users clicking "Save" are redirected to `/login`. The saved state is managed in a `FavoritesStore` NgRx Signal Store slice.

## Goals

- Bookmark toggle on restaurant cards and detail page.
- "My Favorites" tab/section on the reservations dashboard.
- Toggle removes a restaurant from favorites when already saved.
- Unauthenticated save redirects to `/login`.

## Non-Goals

- No favoriting other content types (time slots, reviews).
- No sharing favorites with other users.
- No favorites-based recommendations.

## Acceptance Criteria

- [ ] Clicking the bookmark icon on a card saves/unsaves the restaurant.
- [ ] "My Favorites" shows all saved restaurants for the authenticated user.
- [ ] Unauthenticated users are redirected to `/login` when clicking save.
- [ ] Toggle is reflected immediately in the UI (optimistic update).

## Technical Constraints

- `UserFavorite` entity: `UserId`, `RestaurantId`, `SavedAt`.
- Backend: `POST /api/favorites/{restaurantId}` (save), `DELETE /api/favorites/{restaurantId}` (unsave), `GET /api/favorites` (list).
- Frontend: `FavoritesStore` with `favoriteIds: Set<string>`, `toggle(restaurantId)`, `loadFavorites()`.
- Favorite state shown via a `<mat-icon>` heart filled/outlined based on store signal.
