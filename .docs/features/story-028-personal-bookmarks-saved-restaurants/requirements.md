# Requirements: Personal Bookmarks / Saved Restaurants

## Summary

Diners bookmark restaurants they want to revisit. A toggle on restaurant cards and the detail page adds/removes the restaurant from their personal favorites list. The favorites list is visible in the dashboard. Unauthenticated users who click "Save" are redirected to login.

## Goals

- `POST /api/favorites/{restaurantId}` adds a bookmark (idempotent)
- `DELETE /api/favorites/{restaurantId}` removes it (idempotent)
- `GET /api/favorites` returns all saved restaurants for the authenticated user
- Heart/bookmark icon on restaurant cards and detail page
- "My Favorites" section in /reservations dashboard

## Acceptance Criteria

- [ ] Clicking save on unauthenticated → redirected to /login
- [ ] Toggle save/unsave changes icon state immediately
- [ ] Favorites persist across sessions
- [ ] "My Favorites" tab/section shows saved restaurants

## Technical Constraints

- `UserFavorite` entity: UserId, RestaurantId, SavedAt
- New EF migration required
- Favorites store in `features/restaurants/store/favorites.store.ts`
