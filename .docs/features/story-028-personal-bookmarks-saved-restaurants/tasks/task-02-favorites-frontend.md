# Task 02: Favorites Frontend

## Status

pending

## Wave

2

## Description

Creates `FavoritesStore` (NgRx Signal Store), adds a bookmark icon to restaurant cards and detail page, and adds a "My Favorites" section to the reservations dashboard. Unauthenticated users clicking Save are redirected to `/login`.

## Dependencies

**Depends on:** task-01-favorites-backend.md
**Blocks:** Nothing

**Context from dependencies:** task-01 created `POST/DELETE /api/favorites/{restaurantId}` and `GET /api/favorites`. `AuthService.isAuthenticated` from STORY-008 determines whether to redirect. `RestaurantListComponent` from STORY-012 shows cards that need the bookmark icon. `RestaurantDetailComponent` from STORY-013 also needs the icon.

## Files to Create

- `client/src/app/features/restaurants/store/favorites.store.ts`

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-list.component.ts` — add bookmark icon to cards
- `client/src/app/features/restaurants/components/restaurant-detail.component.ts` — add bookmark button
- `client/src/app/features/reservations/components/reservations-dashboard.component.ts` — add favorites tab/section

## Technical Details

### Code Snippets

```typescript
// store/favorites.store.ts
export const FavoritesStore = signalStore(
  { providedIn: 'root' },
  withState({ favoriteIds: new Set<string>(), loaded: false }),
  withComputed((store) => ({
    isFavorite: (id: string) => computed(() => store.favoriteIds().has(id)),
  })),
  withMethods((store, http = inject(HttpClient)) => ({
    loadFavorites() {
      http.get<{ id: string }[]>(`${environment.apiBaseUrl}/favorites`).subscribe({
        next: (favs) => patchState(store, { favoriteIds: new Set(favs.map(f => f.id)), loaded: true }),
      });
    },
    toggleFavorite(restaurantId: string, isAuthenticated: boolean, router: Router) {
      if (!isAuthenticated) { router.navigate(['/login']); return; }
      const isFav = store.favoriteIds().has(restaurantId);
      const method = isFav ? 'delete' : 'post';
      http.request(method, `${environment.apiBaseUrl}/favorites/${restaurantId}`).subscribe({
        next: () => {
          const ids = new Set(store.favoriteIds());
          isFav ? ids.delete(restaurantId) : ids.add(restaurantId);
          patchState(store, { favoriteIds: ids });
        },
      });
    },
  }))
);
```

```html
<!-- Add to restaurant card template: -->
<button mat-icon-button (click)="toggleFavorite(restaurant.id, $event)">
  <mat-icon>{{ favoritesStore.isFavorite(restaurant.id)() ? 'bookmark' : 'bookmark_border' }}</mat-icon>
</button>
```

## Acceptance Criteria

- [ ] `FavoritesStore` tracks favoriteIds as a Set
- [ ] Clicking bookmark when authenticated toggles the favorite
- [ ] Clicking bookmark when unauthenticated redirects to /login
- [ ] Bookmark icon shows filled/unfilled based on favorite state
- [ ] Favorites section visible in reservations dashboard
