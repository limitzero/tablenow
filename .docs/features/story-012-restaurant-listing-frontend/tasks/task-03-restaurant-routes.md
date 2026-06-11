# Task 03: Restaurant Routes

## Status

pending

## Wave

2

## Description

Creates the `restaurants.routes.ts` route config for `/restaurants` (list) and `/restaurants/:id` (detail stub — filled in STORY-013) and wires them into `app.routes.ts`. Creates `features/restaurants/index.ts` barrel.

## Dependencies

**Depends on:** task-01-restaurants-store-service.md
**Blocks:** STORY-013 (detail component route must exist)

**Context from dependencies:** task-01 created the service and store. task-02 (parallel) creates `RestaurantListComponent`. The `/restaurants/:id` detail component (created in STORY-013) is referenced here as a lazy import. This file does not overlap with task-02.

## Files to Create

- `client/src/app/features/restaurants/routes/restaurants.routes.ts`
- `client/src/app/features/restaurants/index.ts`

## Files to Modify

- `client/src/app/app.routes.ts`

## Technical Details

### Code Snippets

```typescript
// features/restaurants/routes/restaurants.routes.ts
import { Routes } from '@angular/router';

export const restaurantRoutes: Routes = [
  {
    path: 'restaurants',
    loadComponent: () =>
      import('../components/restaurant-list.component').then(m => m.RestaurantListComponent),
  },
  {
    path: 'restaurants/:id',
    loadComponent: () =>
      import('../components/restaurant-detail.component').then(m => m.RestaurantDetailComponent),
  },
];
```

```typescript
// app.routes.ts — add restaurantRoutes
import { restaurantRoutes } from './features/restaurants/routes/restaurants.routes';

export const routes: Routes = [
  { path: '', redirectTo: 'restaurants', pathMatch: 'full' },
  ...authRoutes,
  ...restaurantRoutes,
  // reservationRoutes added in STORY-018
];
```

```typescript
// features/restaurants/index.ts
export { RestaurantListComponent } from './components/restaurant-list.component';
export { restaurantRoutes } from './routes/restaurants.routes';
export { RestaurantsStore } from './store/restaurants.store';
export { RestaurantsService } from './services/restaurants.service';
```

## Acceptance Criteria

- [ ] `/restaurants` route loads `RestaurantListComponent`
- [ ] `/restaurants/:id` route loads `RestaurantDetailComponent` (lazy)
- [ ] Routes added to `app.routes.ts`
- [ ] `features/restaurants/index.ts` barrel exports key items
