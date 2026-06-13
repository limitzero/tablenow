# Implementation Plan: Restaurant Listing — Frontend

## Phase 1 — Store and Models

Foundation layer — all other tasks depend on the types and store defined here.

- [ ] **task-01-restaurants-store** — Define `Restaurant` TypeScript model, create `RestaurantsService` (wraps `httpResource()`), create `RestaurantsStore` NgRx Signal Store slice with `restaurants`, `cuisineFilter`, and derived `filteredRestaurants` computed signal.

### Technical Details

`Restaurant` model:
```typescript
export interface Restaurant {
  id: string;
  name: string;
  cuisine: string;
  address: string;
  description: string;
  thumbnailUrl: string;
}
```

`RestaurantsStore` state: `restaurants: Restaurant[]`, `cuisineFilter: string` (empty = all).
Computed: `filteredRestaurants = computed(() => ...)`.

## Phase 2 — List Component

Renders the restaurant grid using the store.

- [ ] **task-02-restaurant-list-component** — `RestaurantListComponent` with Material card grid, cuisine filter `<mat-select>`, and navigation to detail route on card click.

### Technical Details

- `@for (r of store.filteredRestaurants(); track r.id)` iterates cards.
- `@if (store.filteredRestaurants().length === 0)` shows empty state.
- `OnPush` change detection.
- Uses `Router.navigate(['/restaurants', id])` on card click.

## Phase 3 — Routes

Wires the feature into the Angular router.

- [ ] **task-03-restaurant-routes** — Define `restaurantRoutes` and add a lazy-loaded route reference in the app routes configuration.

### Technical Details

```typescript
export const restaurantRoutes: Routes = [
  { path: '', component: RestaurantListComponent },
  { path: ':id', component: RestaurantDetailComponent }, // placeholder for STORY-013
];
```
