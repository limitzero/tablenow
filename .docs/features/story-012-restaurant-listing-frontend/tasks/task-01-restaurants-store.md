# Task 01: Restaurants Store & Models

## Status

pending

## Wave

1

## Description

Create the foundation for the restaurants feature: TypeScript model interfaces, a `RestaurantsService` that fetches from `GET /api/restaurants` using `httpResource()`, and an NgRx Signal Store slice (`RestaurantsStore`) that holds the restaurant list, the active cuisine filter, and a derived computed signal for the filtered list. All subsequent components in this feature import from this store.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-02-restaurant-list-component.md

**Context from dependencies:** This is a Wave 1 task. It assumes STORY-002 created the Angular project with NgRx Signal Store and Angular Material installed, `environment.ts` defines `apiBaseUrl = 'http://localhost:5000/api'`, and the `core/` HTTP interceptor from STORY-009 attaches the JWT when present (not required here since restaurants are public).

## Files to Create

- `client/src/app/features/restaurants/models/restaurant.model.ts` — `Restaurant` interface.
- `client/src/app/features/restaurants/services/restaurants.service.ts` — `RestaurantsService` using `httpResource()`.
- `client/src/app/features/restaurants/store/restaurants.store.ts` — NgRx Signal Store slice.
- `client/src/app/features/restaurants/index.ts` — Barrel export.

## Files to Modify

- None (new feature folder).

## Technical Details

### Implementation Steps

1. Define `Restaurant` interface with `id`, `name`, `cuisine`, `address`, `description`, `thumbnailUrl` (all strings).
2. Create `RestaurantsService` using `inject(HttpClient)` and expose a `getAll()` method returning `httpResource<Restaurant[]>(() => \`\${environment.apiBaseUrl}/restaurants\`)`.
3. Create `RestaurantsStore` using `signalStore(...)`:
   - State: `restaurants: Restaurant[]` (initial `[]`), `cuisineFilter: string` (initial `''`).
   - Computed: `filteredRestaurants = computed(() => filter === '' ? all : all.filter(r => r.cuisine === filter))`.
   - Method: `setCuisineFilter(filter: string)` and `setRestaurants(restaurants: Restaurant[])`.
4. Export everything from `index.ts`.

### Code Snippets

```typescript
// restaurant.model.ts
export interface Restaurant {
  id: string;
  name: string;
  cuisine: string;
  address: string;
  description: string;
  thumbnailUrl: string;
}
```

```typescript
// restaurants.store.ts
import { signalStore, withState, withComputed, withMethods } from '@ngrx/signals';
import { computed } from '@angular/core';
import { Restaurant } from '../models/restaurant.model';

type RestaurantsState = { restaurants: Restaurant[]; cuisineFilter: string };

export const RestaurantsStore = signalStore(
  { providedIn: 'root' },
  withState<RestaurantsState>({ restaurants: [], cuisineFilter: '' }),
  withComputed(({ restaurants, cuisineFilter }) => ({
    filteredRestaurants: computed(() => {
      const filter = cuisineFilter();
      return filter === '' ? restaurants() : restaurants().filter(r => r.cuisine === filter);
    }),
    availableCuisines: computed(() => [...new Set(restaurants().map(r => r.cuisine))].sort()),
  })),
  withMethods((store) => ({
    setRestaurants: (list: Restaurant[]) => patchState(store, { restaurants: list }),
    setCuisineFilter: (filter: string) => patchState(store, { cuisineFilter: filter }),
  })),
);
```

## Acceptance Criteria

- [ ] `Restaurant` interface has `id`, `name`, `cuisine`, `address`, `description`, `thumbnailUrl`.
- [ ] `RestaurantsStore.filteredRestaurants` returns all restaurants when `cuisineFilter` is empty.
- [ ] `RestaurantsStore.filteredRestaurants` returns only matching restaurants when a filter is set.
- [ ] `RestaurantsStore.availableCuisines` returns a sorted, deduplicated list of cuisine values.
- [ ] `index.ts` exports the store, service, and model.

## Notes

- `providedIn: 'root'` on the store makes it a singleton — the same instance is shared between the list and detail components, avoiding redundant API calls when navigating back.
