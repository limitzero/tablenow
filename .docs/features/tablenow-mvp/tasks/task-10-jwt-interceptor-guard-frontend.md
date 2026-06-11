# Task 10: JWT Interceptor & Route Guard — Frontend

## Status

pending

## Phase

6

## Description

Add two pieces of Angular cross-cutting infrastructure: (1) an HTTP interceptor that automatically attaches the stored JWT as a `Bearer` token to every outgoing API request, and (2) a `canActivate` route guard that redirects unauthenticated users to `/login`. The interceptor also handles 401 responses globally by clearing the stored token and redirecting to `/login`. These must be wired into `app.config.ts` before any protected routes can be accessed.

## Dependencies

**Depends on:** task-08-auth-frontend  
**Blocks:** task-13-restaurant-listing-frontend, task-15-my-reservations-dashboard-frontend

**Context from dependencies:** task-08 created `AuthService` in `client/src/app/core/services/auth.service.ts`. `AuthService` exposes `token()` (a read-only Signal with the stored JWT string or `null`), `isAuthenticated` (a computed boolean Signal), and `logout()` (clears token and navigates to `/login`). `app.config.ts` has a `provideHttpClient(withInterceptors([]))` call with an empty interceptors array — add the auth interceptor there.

## Files to Create

- `client/src/app/core/interceptors/auth.interceptor.ts` — functional HTTP interceptor
- `client/src/app/core/guards/auth.guard.ts` — functional `canActivate` guard

## Files to Modify

- `client/src/app/app.config.ts` — add `authInterceptor` to `withInterceptors([...])`
- `client/src/app/app.routes.ts` — wrap protected routes in an `authGuard` (applied in task-13 and task-15, but guard must exist here)

## Technical Details

### Implementation Steps

1. **Create `auth.interceptor.ts`** (functional interceptor — not a class).

2. **Create `auth.guard.ts`** (functional guard using `inject()`).

3. **Register interceptor in `app.config.ts`.**

4. **Export both from `core/` barrel** (`client/src/app/core/index.ts`).

### Code Snippets

**`auth.interceptor.ts`:**
```typescript
import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const auth = inject(AuthService);
  const token = auth.token();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError(err => {
      if (err.status === 401) {
        auth.logout(); // clears token + navigates to /login
      }
      return throwError(() => err);
    })
  );
};
```

**`auth.guard.ts`:**
```typescript
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }
  return router.createUrlTree(['/login']);
};
```

**Updated `app.config.ts`:**
```typescript
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([authInterceptor])  // ← added here
    ),
    provideAnimationsAsync(),
  ],
};
```

**`core/index.ts` (barrel):**
```typescript
export { AuthService } from './services/auth.service';
export { authInterceptor } from './interceptors/auth.interceptor';
export { authGuard } from './guards/auth.guard';
```

**How the guard is applied to routes (added in task-13 and task-15):**
```typescript
// Example — do NOT add these routes here; just make the guard available
{
  path: 'reservations',
  canActivate: [authGuard],
  loadChildren: () => import('./features/reservations/routes/reservations.routes')
    .then(m => m.RESERVATION_ROUTES),
}
```

## Acceptance Criteria

- [ ] Every HTTP request made while a token is stored includes `Authorization: Bearer <token>` header
- [ ] HTTP requests made without a stored token do not include an `Authorization` header
- [ ] A 401 response from any API call clears `localStorage` and redirects to `/login`
- [ ] Navigating to a guarded route without a stored token redirects to `/login`
- [ ] Navigating to a guarded route with a valid stored token allows access
- [ ] `app.config.ts` registers `authInterceptor` via `withInterceptors`

## Notes

- Use the functional interceptor pattern (`HttpInterceptorFn`) — not a class-based interceptor (`HttpInterceptor`). Angular 17+ prefers the functional form.
- Use the functional guard pattern (`CanActivateFn`) — not a class implementing `CanActivate`.
- `auth.logout()` (from task-08) already handles both token clearance and navigation — the interceptor just calls it.
- The interceptor should pass through all requests unmodified if no token is stored — public endpoints (restaurant listing, register, login) must work for unauthenticated users.
