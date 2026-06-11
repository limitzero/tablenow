# Task 02: Auth Guard

## Status

pending

## Wave

1

## Description

Creates the `CanActivateFn` route guard that redirects unauthenticated users to `/login`. Applied to the `/reservations` route in STORY-018. Reads `AuthService.isAuthenticated` signal.

## Dependencies

**Depends on:** None (Wave 1 — parallel with task-01, different files)
**Blocks:** STORY-018 (reservations route uses this guard)

**Context from dependencies:** STORY-008 task-01 created `AuthService.isAuthenticated` as a computed Signal. This task creates the guard that reads it. The guard is exported from `core/index.ts` for use in any feature route file.

## Files to Create

- `client/src/app/core/guards/auth.guard.ts`

## Files to Modify

- `client/src/app/core/index.ts` — add guard export

## Technical Details

### Code Snippets

```typescript
// client/src/app/core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }
  return router.createUrlTree(['/login']);
};
```

```typescript
// core/index.ts additions:
export { authGuard } from './guards/auth.guard';
export { authInterceptor } from './interceptors/auth.interceptor';
```

## Acceptance Criteria

- [ ] `authGuard` exists as a `CanActivateFn` at `core/guards/auth.guard.ts`
- [ ] Returns `true` when `authService.isAuthenticated()` is true
- [ ] Returns `router.createUrlTree(['/login'])` when not authenticated (preserves URL)
- [ ] Exported from `core/index.ts`
