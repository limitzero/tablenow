# Task 04: Frontend Favorites UI

## Status

pending

## Wave

2

## Description

Add a heart/bookmark toggle icon to each restaurant card in `RestaurantListComponent` and to the `RestaurantDetailComponent`. For authenticated users, clicking the icon calls `store.toggle(restaurantId)`. Unauthenticated users are redirected to `/login`. Also add a `MyFavoritesComponent` that shows all saved restaurants, accessible from the reservations dashboard.

## Dependencies

**Depends on:** task-02-favorites-store.md
**Blocks:** None

**Context from dependencies:** task-02 created `FavoritesStore` with `isFavorite(id)` computed signal and `toggle(id)` method. `AuthService.isAuthenticated` is available from STORY-008.

## Files to Create

- `client/src/app/features/reservations/components/my-favorites/my-favorites.component.ts` — Component listing saved restaurants.
- `client/src/app/features/reservations/components/my-favorites/my-favorites.component.html`

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-list/restaurant-list.component.html` — Add heart icon to each card.
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.html` — Add heart icon near the restaurant name.

## Technical Details

Heart icon snippet for restaurant card:
```html
<button mat-icon-button (click)="onToggleFavorite($event, restaurant.id)">
  <mat-icon>{{ favStore.isFavorite(restaurant.id)() ? 'favorite' : 'favorite_border' }}</mat-icon>
</button>
```

`onToggleFavorite(event, id)`:
```typescript
onToggleFavorite(event: Event, id: string): void {
  event.stopPropagation(); // prevent card navigation
  if (!this.authService.isAuthenticated()) {
    this.router.navigate(['/login']);
    return;
  }
  this.favStore.toggle(id);
}
```

## Acceptance Criteria

- [ ] Heart icon on restaurant cards reflects saved state.
- [ ] Clicking heart for authenticated user calls `favStore.toggle(id)`.
- [ ] Unauthenticated click redirects to `/login`.
- [ ] `MyFavoritesComponent` renders the user's saved restaurants.
