# Task 01: Restaurants Store & Service

## Status

pending

## Wave

1

## Description

Creates the `RestaurantsService` for HTTP calls and the NgRx Signal Store slice that holds restaurant state and the cuisine filter with a computed `filteredRestaurants` signal.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-002 Angular scaffold, STORY-010 backend exists)
**Blocks:** task-02-restaurant-list-component.md, task-03-restaurant-routes.md

**Context from dependencies:** STORY-002 created the `features/` directory. STORY-010 provides `GET /api/restaurants` endpoint. `environment.ts` has `apiBaseUrl`. This task creates the service and store that the list component uses.

## Files to Create

- `client/src/app/features/restaurants/models/restaurant.model.ts`
- `client/src/app/features/restaurants/services/restaurants.service.ts`
- `client/src/app/features/restaurants/store/restaurants.store.ts`

## Technical Details

### Code Snippets

```typescript
// models/restaurant.model.ts
export interface RestaurantDto {
  id: string;
  name: string;
  cuisine: string;
  address: string;
  description: string;
  thumbnailUrl: string;
}
```

```typescript
// services/restaurants.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { RestaurantDto } from '../models/restaurant.model';

@Injectable({ providedIn: 'root' })
export class RestaurantsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/restaurants`;

  getAll() {
    return this.http.get<RestaurantDto[]>(this.base);
  }

  getById(id: string) {
    return this.http.get<RestaurantDto>(`${this.base}/${id}`);
  }
}
```

```typescript
// store/restaurants.store.ts
import { signalStore, withState, withComputed, withMethods } from '@ngrx/signals';
import { inject } from '@angular/core';
import { computed } from '@angular/core';
import { RestaurantsService } from '../services/restaurants.service';
import { RestaurantDto } from '../models/restaurant.model';

type RestaurantsState = {
  restaurants: RestaurantDto[];
  cuisineFilter: string | null;
  loading: boolean;
};

const initialState: RestaurantsState = {
  restaurants: [],
  cuisineFilter: null,
  loading: false,
};

export const RestaurantsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    filteredRestaurants: computed(() => {
      const filter = store.cuisineFilter();
      return filter
        ? store.restaurants().filter(r => r.cuisine === filter)
        : store.restaurants();
    }),
    availableCuisines: computed(() =>
      [...new Set(store.restaurants().map(r => r.cuisine))].sort()
    ),
  })),
  withMethods((store, service = inject(RestaurantsService)) => ({
    loadRestaurants() {
      patchState(store, { loading: true });
      service.getAll().subscribe({
        next: (restaurants) => patchState(store, { restaurants, loading: false }),
        error: () => patchState(store, { loading: false }),
      });
    },
    setCuisineFilter(cuisine: string | null) {
      patchState(store, { cuisineFilter: cuisine });
    },
  }))
);
```

## Acceptance Criteria

- [ ] `RestaurantDto` interface matches backend contract (id, name, cuisine, address, description, thumbnailUrl)
- [ ] `RestaurantsService.getAll()` calls `GET /api/restaurants`
- [ ] `RestaurantsStore` has `restaurants`, `cuisineFilter`, `loading` state
- [ ] `filteredRestaurants` is a computed signal filtered by `cuisineFilter`
- [ ] `loadRestaurants()` sets loading true/false around the HTTP call
