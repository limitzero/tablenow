# Task 02: Folder Structure & Theme

## Status

pending

## Wave

2

## Description

Creates the feature-based folder structure under `src/app/`, sets up the Angular Material custom theme in `styles.scss`, and creates `environment.ts` pointing to the backend API. Every subsequent frontend story drops its feature folder into `features/` — this task defines and creates those top-level directories with their barrel exports.

## Dependencies

**Depends on:** task-01-angular-project-init.md
**Blocks:** All frontend feature stories (STORY-008+)

**Context from dependencies:** task-01 created the Angular project with `app.component.ts`, `app.config.ts`, and `app.routes.ts` but no feature folders or theme. This task adds the structural directories and design system setup.

## Files to Create

- `client/src/environments/environment.ts` — `apiBaseUrl` pointing to local backend
- `client/src/environments/environment.prod.ts` — production placeholder
- `client/src/app/core/index.ts` — barrel export (empty initially)
- `client/src/app/core/services/.gitkeep`
- `client/src/app/core/interceptors/.gitkeep`
- `client/src/app/core/guards/.gitkeep`
- `client/src/app/shared/index.ts` — barrel export (empty initially)
- `client/src/app/shared/components/.gitkeep`
- `client/src/app/features/.gitkeep`

## Files to Modify

- `client/src/styles.scss` — Angular Material theme + global styles

## Technical Details

### Implementation Steps

1. Create `client/src/environments/environment.ts`:
   ```typescript
   export const environment = {
     production: false,
     apiBaseUrl: 'http://localhost:5000/api',
   };
   ```

2. Create `client/src/environments/environment.prod.ts`:
   ```typescript
   export const environment = {
     production: true,
     apiBaseUrl: '/api',
   };
   ```

3. Add `fileReplacements` to `angular.json` under the production build configuration so `environment.ts` is replaced with `environment.prod.ts` on prod builds.

4. Configure Angular Material theme in `styles.scss`:
   ```scss
   @use '@angular/material' as mat;

   @include mat.core();

   $primary: mat.define-palette(mat.$indigo-palette);
   $accent: mat.define-palette(mat.$pink-palette, A200, A100, A400);
   $warn: mat.define-palette(mat.$red-palette);

   $theme: mat.define-light-theme((
     color: (primary: $primary, accent: $accent, warn: $warn),
     typography: mat.define-typography-config(),
     density: 0,
   ));

   @include mat.all-component-themes($theme);

   html, body { height: 100%; }
   body { margin: 0; font-family: Roboto, "Helvetica Neue", sans-serif; }
   ```

5. Create `core/index.ts` and `shared/index.ts` as empty barrel files (they'll be populated by later stories).

6. Add `MatToolbarModule` import to `AppComponent` and a simple `<mat-toolbar>TableNow</mat-toolbar>` to confirm Material is working.

### Environment Variables

- `apiBaseUrl` — the base URL for all API calls; injected via the environment object (not hardcoded in services)

## Acceptance Criteria

- [ ] `environment.ts` exists with `apiBaseUrl: 'http://localhost:5000/api'`
- [ ] `styles.scss` includes Angular Material theme setup and `@include mat.all-component-themes`
- [ ] `core/`, `shared/`, and `features/` directories exist under `src/app/`
- [ ] `core/index.ts` and `shared/index.ts` barrel files exist
- [ ] `npm run build` still passes with zero errors after these changes
