# Task 13: Restaurant Listing Frontend

## Status

pending

## Phase

7

## Description

Build the restaurant listing page at `/restaurants`. Diners see a grid of Angular Material cards showing each restaurant's name, cuisine, and address. A cuisine-type filter dropdown narrows the list client-side. Clicking a card navigates to `/restaurants/:id`. State is managed in a NgRx Signal Store slice. Data is fetched with `httpResource()` — no `.subscribe()` in components.

## Dependencies

**Depends on:** task-10-jwt-interceptor-guard-frontend, task-07-restaurant-slot-api  
**Blocks:** task-14-restaurant-detail-frontend

**Context from dependencies:** task-10 added `authInterceptor` to `provideHttpClient` — the interceptor attaches a JWT header if a token is present (restaurant endpoints are public and work without a token). task-07 implemented `GET /api/v1/restaurants` returning `[{id, name, cuisine, address, thumbnailUrl}]`. The `environment.apiBaseUrl` is `http://localhost:5000/api`. `app.routes.ts` exists and accepts new route additions.

## Files to Create

- `client/src/app/features/restaurants/models/restaurant.model.ts`
- `client/src/app/features/restaurants/services/restaurants.service.ts`
- `client/src/app/features/restaurants/store/restaurants.store.ts`
- `client/src/app/features/restaurants/components/restaurant-list/restaurant-list.component.ts`
- `client/src/app/features/restaurants/components/restaurant-list/restaurant-list.component.html`
- `client/src/app/features/restaurants/components/restaurant-list/restaurant-list.component.scss`
- `client/src/app/features/restaurants/routes/restaurants.routes.ts`
- `client/src/app/features/restaurants/index.ts`

## Files to Modify

- `client/src/app/app.routes.ts` — add lazy-loaded restaurants routes

## Technical Details

### Implementation Steps

1. **Define `Restaurant` model interface.**
2. **Write `RestaurantsService`** — wraps `HttpClient` calls; used by the store.
3. **Write `restaurants.store.ts`** — NgRx Signal Store slice with `restaurants`, `cuisineFilter`, and `filteredRestaurants` state/computed.
4. **Write `RestaurantListComponent`** — reads from store, renders grid, handles filter.
5. **Write `restaurants.routes.ts`** with the listing route (detail route placeholder — filled in task-14).
6. **Add to `app.routes.ts`** as a lazy-loaded child route.

### Code Snippets

**`restaurant.model.ts`:**
```typescript
export interface Restaurant {
  id: string;
  name: string;
  cuisine: string;
  address: string;
  thumbnailUrl: string | null;
}
```

**`restaurants.service.ts`:**
```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Restaurant } from '../models/restaurant.model';

@Injectable({ providedIn: 'root' })
export class RestaurantsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/v1`;

  getRestaurants() {
    return this.http.get<Restaurant[]>(`${this.base}/restaurants`);
  }

  getRestaurantById(id: string) {
    return this.http.get<RestaurantDetail>(`${this.base}/restaurants/${id}`);
  }
}
```

**`restaurants.store.ts` (NgRx Signal Store):**
```typescript
import { signalStore, withState, withComputed, withMethods } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { computed, inject } from '@angular/core';
import { pipe, switchMap, tap } from 'rxjs';
import { RestaurantsService } from '../services/restaurants.service';
import { Restaurant } from '../models/restaurant.model';

interface RestaurantsState {
  restaurants: Restaurant[];
  cuisineFilter: string | null;
  loading: boolean;
}

export const RestaurantsStore = signalStore(
  { providedIn: 'root' },
  withState<RestaurantsState>({
    restaurants: [],
    cuisineFilter: null,
    loading: false,
  }),
  withComputed(({ restaurants, cuisineFilter }) => ({
    filteredRestaurants: computed(() => {
      const filter = cuisineFilter();
      return filter
        ? restaurants().filter(r => r.cuisine === filter)
        : restaurants();
    }),
    availableCuisines: computed(() =>
      [...new Set(restaurants().map(r => r.cuisine))].sort()
    ),
  })),
  withMethods((store, service = inject(RestaurantsService)) => ({
    loadRestaurants: rxMethod<void>(
      pipe(
        tap(() => patchState(store, { loading: true })),
        switchMap(() => service.getRestaurants()),
        tap(restaurants => patchState(store, { restaurants, loading: false }))
      )
    ),
    setCuisineFilter(cuisine: string | null): void {
      patchState(store, { cuisineFilter: cuisine });
    },
  }))
);
```

**`restaurant-list.component.ts`:**
```typescript
import { Component, ChangeDetectionStrategy, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RestaurantsStore } from '../../store/restaurants.store';

@Component({
  selector: 'app-restaurant-list',
  standalone: true,
  imports: [MatCardModule, MatSelectModule, MatProgressSpinnerModule],
  templateUrl: './restaurant-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RestaurantListComponent implements OnInit {
  private readonly router = inject(Router);
  protected readonly store = inject(RestaurantsStore);

  ngOnInit(): void {
    this.store.loadRestaurants();
  }

  navigateToDetail(id: string): void {
    this.router.navigate(['/restaurants', id]);
  }

  onFilterChange(cuisine: string | null): void {
    this.store.setCuisineFilter(cuisine);
  }
}
```

**`restaurant-list.component.html`:**
```html
<div class="page-container">
  <h1>Restaurants</h1>

  <mat-form-field appearance="outline">
    <mat-label>Cuisine</mat-label>
    <mat-select
      [value]="store.cuisineFilter()"
      (selectionChange)="onFilterChange($event.value)">
      <mat-option [value]="null">All cuisines</mat-option>
      @for (cuisine of store.availableCuisines(); track cuisine) {
        <mat-option [value]="cuisine">{{ cuisine }}</mat-option>
      }
    </mat-select>
  </mat-form-field>

  @if (store.loading()) {
    <mat-spinner />
  } @else {
    <div class="restaurant-grid">
      @for (r of store.filteredRestaurants(); track r.id) {
        <mat-card (click)="navigateToDetail(r.id)" class="restaurant-card">
          <mat-card-header>
            <mat-card-title>{{ r.name }}</mat-card-title>
            <mat-card-subtitle>{{ r.cuisine }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <p>{{ r.address }}</p>
          </mat-card-content>
        </mat-card>
      } @empty {
        <p>No restaurants match your filter.</p>
      }
    </div>
  }
</div>
```

**`restaurants.routes.ts`:**
```typescript
import { Routes } from '@angular/router';

export const RESTAURANT_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../components/restaurant-list/restaurant-list.component')
        .then(m => m.RestaurantListComponent),
  },
  // Detail route added in task-14:
  // { path: ':id', loadComponent: () => ... }
];
```

**`app.routes.ts` addition:**
```typescript
{
  path: 'restaurants',
  loadChildren: () =>
    import('./features/restaurants/routes/restaurants.routes')
      .then(m => m.RESTAURANT_ROUTES),
},
```

**`index.ts`:**
```typescript
export { RestaurantsStore } from './store/restaurants.store';
export { RestaurantsService } from './services/restaurants.service';
```

## Acceptance Criteria

- [ ] `/restaurants` route renders a grid of restaurant cards with name, cuisine, and address
- [ ] Cuisine filter dropdown updates the visible list in real time (client-side, no API call)
- [ ] Clicking a card navigates to `/restaurants/:id`
- [ ] `store.loading()` shows a spinner while data is fetching
- [ ] No `*ngIf`, `*ngFor`, `.subscribe()`, or constructor injection in any component
- [ ] `ChangeDetectionStrategy.OnPush` on `RestaurantListComponent`

## Notes

- The restaurants list endpoint requires no authentication — the interceptor is a no-op here if no token is stored, which is correct.
- `signalStore` from `@ngrx/signals` and `patchState` from `@ngrx/signals` must be imported properly — check the version's exact API.
- Do not add detail route to `restaurants.routes.ts` yet — task-14 adds it to avoid file modification conflicts.
