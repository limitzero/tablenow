# Requirements: Personal Bookmarks

## Summary

Authenticated users can save/unsave restaurants using `POST/DELETE /api/favorites/{restaurantId}`. A heart/bookmark icon on restaurant cards shows saved state. My Favorites dashboard tab lists saved restaurants.

## Goals

- Toggle save/unsave on restaurant cards
- My Favorites tab in user dashboard
- Unauthenticated user redirected to login on save attempt
- `UserFavorite` entity persisted

## Acceptance Criteria

- [ ] POST /api/favorites/{id} saves; DELETE removes (auth required)
- [ ] Heart icon shows filled/unfilled based on saved state
- [ ] My Favorites tab shows all saved restaurants
- [ ] 401 for unauthenticated requests

## Technical Constraints

- `UserFavorite`: UserId, RestaurantId, SavedAt
- Frontend: `favorites.store.ts` NgRx Signal Store
