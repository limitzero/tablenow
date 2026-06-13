# Task 02: Frontend Favorites Store

## Status

pending

## Wave

1

## Description

Create `FavoritesStore` NgRx Signal Store with state `favoriteIds: Set<string>`, a `loadFavorites()` method that fetches `GET /api/favorites`, and a `toggle(restaurantId)` method that optimistically updates the store and calls `POST` or `DELETE /api/favorites/{restaurantId}` based on current state. Create `FavoritesService` for the HTTP calls.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-04-favorites-ui.md

## Files to Create

- `client/src/app/features/reservations/models/favorite.model.ts` — `Favorite` interface.
- `client/src/app/features/reservations/services/favorites.service.ts` — `FavoritesService`.
- `client/src/app/features/reservations/store/favorites.store.ts` — `FavoritesStore`.

## Technical Details

```typescript
export const FavoritesStore = signalStore(
  { providedIn: 'root' },
  withState<{ favoriteIds: string[] }>({ favoriteIds: [] }),
  withComputed(({ favoriteIds }) => ({
    isFavorite: (id: string) => computed(() => favoriteIds().includes(id)),
  })),
  withMethods((store, service = inject(FavoritesService)) => ({
    async loadFavorites() {
      const ids = await firstValueFrom(service.getFavoriteIds());
      patchState(store, { favoriteIds: ids });
    },
    async toggle(restaurantId: string) {
      const isSaved = store.favoriteIds().includes(restaurantId);
      // Optimistic update
      patchState(store, {
        favoriteIds: isSaved
          ? store.favoriteIds().filter(id => id !== restaurantId)
          : [...store.favoriteIds(), restaurantId],
      });
      await firstValueFrom(isSaved
        ? service.remove(restaurantId)
        : service.add(restaurantId));
    },
  })),
);
```

## Acceptance Criteria

- [ ] `FavoritesStore.isFavorite(id)` returns a computed signal for a given restaurant ID.
- [ ] `toggle(id)` adds to favorites if not present, removes if already present.
- [ ] Optimistic UI update happens before the API call completes.
