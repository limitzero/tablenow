# Task 03: Restaurant Routes

## Status

pending

## Wave

3

## Description

Define the Angular route configuration for the restaurants feature and register it as a lazy-loaded route in the app router. The `/restaurants` path loads `RestaurantListComponent`; the `/restaurants/:id` path will load the detail component (placeholder for STORY-013). This task wires the feature into the application navigation so the list page is reachable from the root.

## Dependencies

**Depends on:** task-02-restaurant-list-component.md
**Blocks:** None

**Context from dependencies:** task-02 created `RestaurantListComponent` in `client/src/app/features/restaurants/components/restaurant-list/`. task-01 established `index.ts` exports for the feature. The app-level routes are configured in `client/src/app/app.routes.ts`.

## Files to Create

- `client/src/app/features/restaurants/routes/restaurant.routes.ts` — Feature route definitions.

## Files to Modify

- `client/src/app/app.routes.ts` — Add lazy-loaded `restaurants` route.
- `client/src/app/features/restaurants/index.ts` — Export routes.

## Technical Details

### Implementation Steps

1. Create `restaurant.routes.ts`:
   ```typescript
   import { Routes } from '@angular/router';
   import { RestaurantListComponent } from '../components/restaurant-list/restaurant-list.component';

   export const restaurantRoutes: Routes = [
     { path: '', component: RestaurantListComponent },
   ];
   ```
2. In `app.routes.ts`, add:
   ```typescript
   {
     path: 'restaurants',
     loadChildren: () =>
       import('./features/restaurants/routes/restaurant.routes').then(m => m.restaurantRoutes),
   }
   ```
3. Ensure a redirect from `''` to `'restaurants'` exists or the root route navigates there.

## Acceptance Criteria

- [ ] Navigating to `/restaurants` renders `RestaurantListComponent`.
- [ ] The restaurants feature is lazy-loaded (not in the main bundle).
- [ ] `app.routes.ts` references the feature via `loadChildren` (not `component` directly).

## Notes

- The `:id` detail route will be added by STORY-013. For now, only the list route is needed.
