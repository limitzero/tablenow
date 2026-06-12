# Task 02: Auth Route Guard

## Status

pending

## Wave

1

## Description

Create a functional `canActivate` route guard that protects authenticated-only routes in TableNow. It reads the `isAuthenticated` computed signal from `AuthService`; when the user is authenticated it allows navigation, and when not it redirects to `/login` by returning a `UrlTree`. This guard is applied to protected routes (e.g. the reservations dashboard) in later stories; this task delivers and exports the reusable guard and demonstrates how to attach it.

## Dependencies

**Depends on:** None (Wave 1) — consumes the STORY-008 `AuthService` contract.
**Blocks:** None

**Context from dependencies:** STORY-008 created `AuthService` at `client/src/app/core/services/auth.service.ts` (`providedIn: 'root'`) exposing `isAuthenticated`, a computed Signal that is `true` only when a non-expired JWT is stored. This guard reads `auth.isAuthenticated()` to decide whether to allow activation. The `/login` route was created in STORY-008. This task is independent of task-01 (the interceptor) — they touch different files.

## Files to Create

- `client/src/app/core/guards/auth.guard.ts` — The functional `CanActivateFn` guard.
- `client/src/app/core/guards/auth.guard.spec.ts` — Unit tests (Vitest) for the authenticated and unauthenticated branches.

## Files to Modify

- None required for this story. (Later stories attach `canActivate: [authGuard]` to protected routes such as `/reservations`.)

## Technical Details

### Implementation Steps

1. Create `authGuard` as a `CanActivateFn`.
2. `inject(AuthService)` and `inject(Router)` inside the guard (guards run in an injection context).
3. If `auth.isAuthenticated()` is `true`, return `true`.
4. Otherwise return a `UrlTree` to `/login` via `router.createUrlTree(['/login'])`. Returning a `UrlTree` is preferred over imperative `router.navigate` inside a guard because the router treats it as a redirect cleanly.
5. Optionally capture the attempted URL as a `returnUrl` query param so the user can be sent back after logging in (nice-to-have; keep it simple if unsure).

### Code Snippets

```ts
// client/src/app/core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  // Redirect to login, preserving where the user was heading.
  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url },
  });
};
```

```ts
// Example usage in a later story's routes (not created here):
// { path: 'reservations', canActivate: [authGuard], loadComponent: () => ... }
```

## Acceptance Criteria

- [ ] When `AuthService.isAuthenticated()` is `true`, the guard returns `true` and navigation proceeds.
- [ ] When `isAuthenticated()` is `false`, the guard returns a `UrlTree` redirecting to `/login`.
- [ ] The guard is a functional `CanActivateFn` (no class implementing `CanActivate`) and resolves dependencies via `inject()`.
- [ ] The guard is exported so later stories can attach it to protected routes with `canActivate: [authGuard]`.

## Notes

- Returning a `UrlTree` (rather than calling `router.navigate` and returning `false`) avoids a brief flash of the protected route and is the idiomatic Angular functional-guard pattern.
- The `returnUrl` query param is optional; only wire the post-login return if the login component is set up to read it (otherwise omit to avoid dead config).
