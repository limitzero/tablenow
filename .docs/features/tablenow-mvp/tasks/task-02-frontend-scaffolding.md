# Task 02: Frontend Scaffolding

## Status

pending

## Phase

1

## Description

Create the Angular 21 SPA for TableNow using standalone components and `bootstrapApplication` (no NgModules). Establish the feature-based folder structure under `client/src/app/` with `core/`, `shared/`, and `features/` directories. Configure Angular Material with a custom theme, NgRx Signal Store for state management, and environment files pointing at the .NET API. This scaffold is the prerequisite for every other frontend task.

## Dependencies

**Depends on:** None (Phase 1)  
**Blocks:** task-08-auth-frontend

**Context from dependencies:** N/A — this is a root task.

## Files to Create

- `client/` — root of the Angular project
- `client/src/app/app.config.ts` — `ApplicationConfig` with router, HTTP, and Material providers
- `client/src/app/app.routes.ts` — top-level lazy-loaded routes (stubs; filled by feature tasks)
- `client/src/app/app.component.ts` — root component (`<router-outlet>`)
- `client/src/app/core/` — directory for auth service, interceptors, guards (files added by later tasks)
- `client/src/app/shared/` — directory for reusable components (initially empty)
- `client/src/app/features/` — directory for feature modules (initially empty)
- `client/src/environments/environment.ts` — dev environment config
- `client/src/environments/environment.prod.ts` — prod environment config
- `client/src/styles.scss` — global styles + Angular Material theme import
- `client/src/theme.scss` — Angular Material custom theme definition

## Files to Modify

- (none — greenfield)

## Technical Details

### Implementation Steps

1. **Scaffold the Angular project:**
   ```bash
   cd client  # or: ng new tablenow-client --standalone --routing --style=scss --directory=client
   ```
   Choose: standalone components ✓, SCSS ✓, routing ✓.

2. **Install NgRx Signal Store and Angular Material:**
   ```bash
   npm install @ngrx/signals
   ng add @angular/material
   ```
   When prompted for theme, choose "Custom" — we'll define our own in `theme.scss`.

3. **Install date utility (for date picker formatting):**
   ```bash
   npm install date-fns
   ```

4. **Create the folder structure:**
   ```bash
   mkdir -p src/app/core/interceptors
   mkdir -p src/app/core/guards
   mkdir -p src/app/core/services
   mkdir -p src/app/shared/components
   mkdir -p src/app/features
   mkdir -p src/environments
   ```

5. **Write `app.config.ts`** (see snippet below).

6. **Write `app.routes.ts`** as a stub — feature routes are added in later tasks.

7. **Write `environment.ts` and `environment.prod.ts`** (see snippet below).

8. **Configure `angular.json`** to use environment file replacements for prod builds.

### Code Snippets

**`app.config.ts`:**
```typescript
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      // withInterceptors([authInterceptor])  ← added in task-10
    ),
    provideAnimationsAsync(),
  ],
};
```

**`app.routes.ts` (stub):**
```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [
  // Routes added by: task-08 (auth), task-13 (restaurants), task-15 (reservations)
  { path: '', redirectTo: '/restaurants', pathMatch: 'full' },
];
```

**`app.component.ts`:**
```typescript
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `<router-outlet />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {}
```

**`environment.ts`:**
```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000/api',
};
```

**`environment.prod.ts`:**
```typescript
export const environment = {
  production: true,
  apiBaseUrl: '/api',
};
```

**`theme.scss` (Angular Material custom theme):**
```scss
@use '@angular/material' as mat;

$primary: mat.define-palette(mat.$indigo-palette);
$accent:  mat.define-palette(mat.$amber-palette, A200, A100, A400);
$warn:    mat.define-palette(mat.$red-palette);

$theme: mat.define-light-theme((
  color: (primary: $primary, accent: $accent, warn: $warn),
  typography: mat.define-typography-config(),
  density: 0,
));

@include mat.all-component-themes($theme);
```

**`styles.scss`:**
```scss
@use './theme';

html, body {
  height: 100%;
  margin: 0;
  font-family: Roboto, "Helvetica Neue", sans-serif;
}
```

### Conventions (all frontend tasks must follow these)

- **Change detection**: `ChangeDetectionStrategy.OnPush` on every component — no exceptions.
- **DI**: Use `inject()` function — never constructor injection.
- **Templates**: `@if` / `@for` — never `*ngIf` / `*ngFor`.
- **API calls**: through services only, never directly in components.
- **No `.subscribe()`** in components — use `httpResource()` or `async` pipe.
- **Barrel exports**: every feature folder must have an `index.ts`.
- **State**: NgRx Signal Store — one store slice per feature.

## Acceptance Criteria

- [ ] `npm run build` from `client/` succeeds with zero errors
- [ ] `bootstrapApplication` is used in `main.ts` — no `AppModule`
- [ ] `src/app/core/`, `src/app/shared/`, `src/app/features/` directories exist
- [ ] `environment.ts` defines `apiBaseUrl` as `http://localhost:5000/api`
- [ ] Angular Material is installed and the custom theme is applied (app renders without style errors)
- [ ] `@ngrx/signals` is listed in `package.json` dependencies
- [ ] `ng serve` starts and the browser shows the default app without console errors

## Notes

- Use `ChangeDetectionStrategy.OnPush` immediately when generating components — it is easier to set upfront than retrofit later.
- The `withInterceptors([])` call in `app.config.ts` is a placeholder — `authInterceptor` is added in task-10.
- Do not install `@ngrx/store` or `@ngrx/effects` — only `@ngrx/signals` is used in this project.
