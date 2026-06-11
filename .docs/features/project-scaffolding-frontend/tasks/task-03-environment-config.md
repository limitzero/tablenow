# Task 03: Configure Environments, Material Theme & NgRx

## Status

pending

## Wave

2

## Description

Configures the three cross-cutting setup concerns needed before any feature work begins: (1) environment files with `apiBaseUrl`, (2) a custom Angular Material theme using the app's brand palette, and (3) NgRx Signal Store and HTTP client providers wired into `app.config.ts`. Also replaces the default Karma test runner with Vitest.

## Dependencies

**Depends on:** task-01-angular-project.md
**Blocks:** All feature stories that use `environment.ts`, the Material theme, or NgRx store

**Context from dependencies:** task-01 created the Angular project with a baseline `app.config.ts`, `angular.json`, and SCSS support. task-02 (parallel) adds the folder structure and routing — these two tasks touch different files so they can run concurrently.

## Files to Create

- `client/src/environments/environment.ts` — Development environment config
- `client/src/environments/environment.prod.ts` — Production environment config
- `client/src/app/app.theme.scss` — Custom Angular Material theme

## Files to Modify

- `client/src/app/app.config.ts` — Add `provideHttpClient`, `provideStore`, `withInterceptors` providers
- `client/src/styles.scss` — Import the custom theme
- `client/package.json` — Add Vitest devDependency
- `client/vite.config.ts` (create if not present) — Vitest configuration

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

3. Configure Angular's file replacements in `angular.json` under `configurations.production.fileReplacements`:
   ```json
   {
     "replace": "src/environments/environment.ts",
     "with": "src/environments/environment.prod.ts"
   }
   ```

4. Create `client/src/app/app.theme.scss` with a custom Material 3 theme:
   ```scss
   @use '@angular/material' as mat;

   $primary-palette: mat.define-palette(mat.$indigo-palette, 700, 300, 900);
   $accent-palette: mat.define-palette(mat.$amber-palette, A200);
   $warn-palette: mat.define-palette(mat.$red-palette);

   $light-theme: mat.define-light-theme((
     color: (
       primary: $primary-palette,
       accent: $accent-palette,
       warn: $warn-palette,
     ),
     typography: mat.define-typography-config(),
     density: 0,
   ));

   @include mat.all-component-themes($light-theme);
   ```

5. Update `client/src/styles.scss`:
   ```scss
   @use './app/app.theme';
   @import '@angular/material/prebuilt-themes/deeppurple-amber.css'; // fallback for components

   html, body { height: 100%; }
   body { margin: 0; font-family: Roboto, "Helvetica Neue", sans-serif; }
   ```

6. Update `client/src/app/app.config.ts`:
   ```typescript
   import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
   import { provideRouter, withComponentInputBinding } from '@angular/router';
   import { provideHttpClient, withInterceptors } from '@angular/common/http';
   import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
   import { routes } from './app.routes';

   export const appConfig: ApplicationConfig = {
     providers: [
       provideZoneChangeDetection({ eventCoalescing: true }),
       provideRouter(routes, withComponentInputBinding()),
       provideHttpClient(withInterceptors([])), // interceptors added by STORY-009
       provideAnimationsAsync(),
     ],
   };
   ```

7. Install and configure Vitest:
   ```bash
   npm install -D vitest @vitest/coverage-v8 @angular/core vitest-angular
   ```

   Create `client/vite.config.ts`:
   ```typescript
   import { defineConfig } from 'vitest/config';
   import angular from '@analogjs/vite-plugin-angular';

   export default defineConfig({
     plugins: [angular()],
     test: {
       globals: true,
       environment: 'jsdom',
       setupFiles: ['src/test-setup.ts'],
     },
   });
   ```

   Update `package.json` scripts:
   ```json
   "test": "vitest run",
   "test:watch": "vitest"
   ```

### Environment Variables

None — environment config is file-based in Angular (build-time substitution).

## Acceptance Criteria

- [ ] `client/src/environments/environment.ts` defines `apiBaseUrl: 'http://localhost:5000/api'`
- [ ] `client/src/environments/environment.prod.ts` defines `apiBaseUrl: '/api'`
- [ ] `app.config.ts` includes `provideHttpClient`, `provideAnimationsAsync`, and `provideRouter`
- [ ] `npm run build` exits with code 0
- [ ] `npm run test` runs (even if zero tests) without errors

## Notes

The `withInterceptors([])` call in `provideHttpClient` starts with an empty array. STORY-009 will add the auth interceptor to this array. The empty array is intentional — do not remove it.
