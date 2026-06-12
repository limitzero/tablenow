# Task 04: Auth Routes

## Status

pending

## Wave

2

## Description

Wire the auth feature's routing and public surface. Define the `/register` and `/login` routes that mount the components built in tasks 02 and 03, expose them through the feature's barrel `index.ts`, and register the routes in the application's root route configuration so the pages are reachable. This is the integration step that makes the auth feature navigable.

## Dependencies

**Depends on:** task-02-register-page.md, task-03-login-page.md
**Blocks:** None

**Context from dependencies:** `task-02-register-page.md` creates `RegisterComponent` at `client/src/app/features/auth/components/register/register.component.ts` (selector `app-register`). `task-03-login-page.md` creates `LoginComponent` at `client/src/app/features/auth/components/login/login.component.ts` (selector `app-login`). Both are standalone `OnPush` components that redirect to `/restaurants` on success. This task references those components from `Routes` definitions and exposes them via the feature barrel.

## Files to Create

- `client/src/app/features/auth/routes/auth.routes.ts` — `Routes` array for the auth feature.
- `client/src/app/features/auth/index.ts` — Barrel export for the feature (required for every feature folder).

## Files to Modify

- `client/src/app/app.routes.ts` (or the project's root route config file) — Add the auth routes to the root routing table.

## Technical Details

### Implementation Steps

1. Create `auth.routes.ts` exporting a `Routes` constant. Prefer lazy `loadComponent` so each page is its own chunk.
2. Create the feature barrel `index.ts` re-exporting `AUTH_ROUTES` (and any types intended for external use). Per project convention every feature folder has a barrel `index.ts`.
3. Register the auth routes in the root route configuration — either spread `AUTH_ROUTES` into the root `Routes`, or lazy-load them via `loadChildren`.

### Code Snippets

```ts
// client/src/app/features/auth/routes/auth.routes.ts
import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  {
    path: 'register',
    loadComponent: () =>
      import('../components/register/register.component').then((m) => m.RegisterComponent),
    title: 'Create your account',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('../components/login/login.component').then((m) => m.LoginComponent),
    title: 'Sign in',
  },
];
```

```ts
// client/src/app/features/auth/index.ts
export { AUTH_ROUTES } from './routes/auth.routes';
```

```ts
// app.routes.ts (root) — option A: lazy children
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./features/auth').then((m) => m.AUTH_ROUTES),
  },
  // ...restaurant routes added by later stories
];
```

## Acceptance Criteria

- [ ] `/register` resolves to `RegisterComponent` and `/login` resolves to `LoginComponent`.
- [ ] The auth feature exposes a barrel `index.ts` that re-exports `AUTH_ROUTES`.
- [ ] The routes are registered in the application's root routing table and are navigable in a running app.
- [ ] Routes use lazy `loadComponent` (or `loadChildren`) so auth pages are code-split.
- [ ] `npm run build` succeeds with the new routes wired in.

## Notes

- The exact root route file name depends on the STORY-002 scaffolding (commonly `app.routes.ts`). Locate the array passed to `provideRouter(...)` in `app.config.ts` and register the auth routes there.
- Do not add a route guard here — guarding is delivered in STORY-009. These two routes (`/register`, `/login`) are intentionally public.
