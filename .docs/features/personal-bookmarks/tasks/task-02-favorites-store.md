# Task 02: Favorites NgRx Signal Store

## Status

pending

## Wave

2

## Description

Creates `favorites.store.ts` NgRx Signal Store and `FavoritesService`. The store holds a Set of saved restaurantIds. Loads on first access. `isFavorite(id)` computed Signal used by restaurant cards.

## Dependencies

**Depends on:** task-01-favorites-backend.md
**Blocks:** task-03-favorites-ui.md

**Context from dependencies:** `GET /api/favorites` returns `Guid[]` (restaurantIds). `POST/DELETE /api/favorites/{restaurantId}` toggles saved state.

## Files to Create

- `client/src/app/features/restaurants/store/favorites.store.ts`
- `client/src/app/features/restaurants/services/favorites.service.ts`

## Technical Details

```typescript
// favorites.service.ts
@Injectable({ providedIn: 'root' })
export class FavoritesService {
  private readonly http = inject(HttpClient);
  getAll() { return this.http.get<string[]>(`${environment.apiBaseUrl}/favorites`); }
  add(id: string) { return this.http.post(`${environment.apiBaseUrl}/favorites/${id}`, {}); }
  remove(id: string) { return this.http.delete(`${environment.apiBaseUrl}/favorites/${id}`); }
}

// favorites.store.ts
export const FavoritesStore = signalStore(
  { providedIn: 'root' },
  withState({ favoriteIds: new Set<string>(), loaded: false }),
  withComputed((store) => ({
    isFavorite: computed(() => (id: string) => store.favoriteIds().has(id)),
  }))
);
```

## Acceptance Criteria

- [ ] `FavoritesStore` with `favoriteIds` Set and `isFavorite` computed exists
- [ ] `FavoritesService` wraps GET/POST/DELETE calls
- [ ] `npm run build` exits with code 0
