# Task 01: Restaurants NgRx Signal Store

## Status

pending

## Wave

1

## Description

Creates the NgRx Signal Store slice for the restaurants feature. Holds `restaurants` state, `cuisineFilter` state, and a `filteredRestaurants` computed signal. The store drives the listing page's reactive data flow.

## Dependencies

**Depends on:** STORY-002 task-03-environment-config.md (NgRx Signal Store configured in app.config.ts)
**Blocks:** task-03-listing-page.md

**Context from dependencies:** STORY-002 task-03 added `@ngrx/signals` and set up `provideStore()`. `environment.ts` has `apiBaseUrl: 'http://localhost:5000/api'`.

## Files to Create

- `client/src/app/features/restaurants/store/restaurants.store.ts`
- `client/src/app/features/restaurants/models/restaurant.model.ts`

## Technical Details

### Code Snippets

```typescript
// restaurant.model.ts
export interface Restaurant {
  id: string;
  name: string;
  cuisine: string;
  address: string;
  description: string;
  thumbnailUrl: string | null;
}
```

```typescript
// restaurants.store.ts
import { computed, inject } from '@angular/core';
import { signalStore, withState, withComputed, withMethods } from '@ngrx/signals';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Restaurant } from '../models/restaurant.model';

type RestaurantsState = {
  restaurants: Restaurant[];
  cuisineFilter: string | null;
  loading: boolean;
  error: string | null;
};

const initialState: RestaurantsState = {
  restaurants: [],
  cuisineFilter: null,
  loading: false,
  error: null,
};

export const RestaurantsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    filteredRestaurants: computed(() => {
      const filter = store.cuisineFilter();
      const all = store.restaurants();
      return filter ? all.filter(r => r.cuisine === filter) : all;
    }),
    availableCuisines: computed(() =>
      [...new Set(store.restaurants().map(r => r.cuisine))].sort()
    ),
  }))
);
```

## Acceptance Criteria

- [ ] `RestaurantsStore` exists with `restaurants`, `cuisineFilter`, `loading`, `error` state
- [ ] `filteredRestaurants` computed signal returns filtered subset when `cuisineFilter` is set
- [ ] `availableCuisines` computed signal returns unique sorted cuisine list
- [ ] `npm run build` exits with code 0
