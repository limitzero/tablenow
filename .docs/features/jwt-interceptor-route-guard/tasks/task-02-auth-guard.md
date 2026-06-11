# Task 02: Auth Route Guard

## Status

pending

## Wave

1

## Description

Creates a functional `CanActivateFn` guard that checks `AuthService.isAuthenticated()`. Returns `true` to allow navigation or a `UrlTree` to `/auth/login` to redirect unauthenticated users.

## Dependencies

**Depends on:** STORY-008 task-01-auth-service.md (AuthService with `isAuthenticated` signal)
**Blocks:** STORY-018 (reservations dashboard uses this guard on its routes)

**Context from dependencies:**
- STORY-008 task-01 created `AuthService` with `isAuthenticated` as a computed Signal
- `isAuthenticated()` returns `true` when a JWT is stored in localStorage
- The guard will be applied to the `/reservations` route in STORY-018

## Files to Create

- `client/src/app/core/guards/auth.guard.ts`

## Files to Modify

- `client/src/app/core/index.ts` — Export `authGuard`

## Technical Details

### Code Snippets

```typescript
// client/src/app/core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/auth/login']);
};
```

Update `client/src/app/core/index.ts`:
```typescript
export { AuthService } from './auth.service';
export { authGuard } from './guards/auth.guard';
export { authInterceptor } from './interceptors/auth.interceptor';
```

Apply to protected routes in `app.routes.ts`:
```typescript
{
  path: 'reservations',
  canActivate: [authGuard],
  loadChildren: () =>
    import('./features/reservations/routes').then(m => m.RESERVATION_ROUTES),
},
```

## Acceptance Criteria

- [ ] `authGuard` exists at `client/src/app/core/guards/auth.guard.ts`
- [ ] Returns `true` when `isAuthenticated()` is true
- [ ] Returns `UrlTree('/auth/login')` when `isAuthenticated()` is false
- [ ] Guard applied to `/reservations` route in `app.routes.ts`
- [ ] `npm run build` exits with code 0
