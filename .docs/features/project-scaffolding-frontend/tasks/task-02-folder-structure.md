# Task 02: Create Feature Folder Structure & App Routing

## Status

pending

## Wave

2

## Description

Establishes the feature-based folder structure under `src/app/` — creating `core/`, `shared/`, and `features/` directories with their required `index.ts` barrel exports. Sets up the top-level app routing with placeholder routes. This gives every subsequent feature story a consistent home and import path.

## Dependencies

**Depends on:** task-01-angular-project.md
**Blocks:** All feature stories (they all add folders under `features/`)

**Context from dependencies:** task-01 created the Angular project with `bootstrapApplication` in `main.ts` and an `app.routes.ts` file. This task populates the folder structure and connects routing.

## Files to Create

- `client/src/app/core/index.ts` — Barrel export for core module
- `client/src/app/core/README.md` — Brief description of what belongs in core
- `client/src/app/shared/index.ts` — Barrel export for shared components/pipes
- `client/src/app/features/index.ts` — Barrel export placeholder

## Files to Modify

- `client/src/app/app.routes.ts` — Add a placeholder home route

## Technical Details

### Implementation Steps

1. Create the required directories:
   ```bash
   mkdir -p client/src/app/core
   mkdir -p client/src/app/shared
   mkdir -p client/src/app/features
   ```

2. Create `core/index.ts` (will grow as auth service and interceptors are added):
   ```typescript
   // Core exports — auth service, interceptors, and guards are exported from here
   export {};
   ```

3. Create `shared/index.ts`:
   ```typescript
   // Shared component exports — reusable components, pipes, and directives
   export {};
   ```

4. Create `features/index.ts`:
   ```typescript
   // Feature module exports
   export {};
   ```

5. Update `app.routes.ts` with a placeholder home route that will be replaced by the restaurant listing feature (STORY-012):
   ```typescript
   import { Routes } from '@angular/router';

   export const routes: Routes = [
     {
       path: '',
       redirectTo: '/restaurants',
       pathMatch: 'full',
     },
     {
       path: 'restaurants',
       loadChildren: () =>
         import('./features/restaurants/routes').then(m => m.RESTAURANT_ROUTES),
     },
     {
       path: 'auth',
       loadChildren: () =>
         import('./features/auth/routes').then(m => m.AUTH_ROUTES),
     },
     {
       path: 'reservations',
       loadChildren: () =>
         import('./features/reservations/routes').then(m => m.RESERVATION_ROUTES),
     },
   ];
   ```
   Note: The lazy-loaded route targets will fail until the feature stories create those files — that is expected. Use `loadChildren` with `() => import(...)` for all feature routes.

6. Create placeholder route files for each feature so the build doesn't fail:
   - `client/src/app/features/restaurants/routes.ts` — exports `RESTAURANT_ROUTES = []`
   - `client/src/app/features/auth/routes.ts` — exports `AUTH_ROUTES = []`
   - `client/src/app/features/reservations/routes.ts` — exports `RESERVATION_ROUTES = []`
   - Each feature folder also gets an `index.ts` barrel export

### Code Snippets

Placeholder route file pattern (repeat for auth and reservations):
```typescript
// client/src/app/features/restaurants/routes.ts
import { Routes } from '@angular/router';

export const RESTAURANT_ROUTES: Routes = [
  // Routes added by STORY-012
];
```

## Acceptance Criteria

- [ ] `client/src/app/core/`, `client/src/app/shared/`, `client/src/app/features/` directories exist with `index.ts` barrels
- [ ] `app.routes.ts` has lazy-loaded route stubs for `restaurants`, `auth`, and `reservations`
- [ ] Placeholder route files exist so `npm run build` still passes
- [ ] `npm run build` exits with code 0

## Notes

The route paths (`/restaurants`, `/auth`, `/reservations`) are the canonical URL structure for the entire application. Feature stories must use these paths. Do not rename them later without updating this file.
