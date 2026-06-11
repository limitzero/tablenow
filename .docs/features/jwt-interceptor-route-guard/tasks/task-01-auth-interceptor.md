# Task 01: Auth HTTP Interceptor

## Status

pending

## Wave

1

## Description

Creates a functional `HttpInterceptorFn` that attaches `Authorization: Bearer <token>` to every outgoing API request when a token is stored. Also handles 401 responses by clearing the token and redirecting to `/login`.

## Dependencies

**Depends on:** STORY-008 task-01-auth-service.md (AuthService with `token()` signal and `clearToken()`)
**Blocks:** Nothing directly — other stories simply use the `withInterceptors([authInterceptor])` registration

**Context from dependencies:**
- STORY-008 task-01 created `AuthService` at `client/src/app/core/auth.service.ts`
- `authService.token()` returns the current JWT string or `null`
- `authService.clearToken()` removes the token from localStorage and updates the signal
- STORY-002 task-03 set up `provideHttpClient(withInterceptors([]))` in `app.config.ts` — the interceptor is added to that array

## Files to Create

- `client/src/app/core/interceptors/auth.interceptor.ts`

## Files to Modify

- `client/src/app/core/index.ts` — Export `authInterceptor`
- `client/src/app/app.config.ts` — Add `authInterceptor` to `withInterceptors([])`

## Technical Details

### Code Snippets

```typescript
// client/src/app/core/interceptors/auth.interceptor.ts
import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth.service';

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.token();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error) => {
      if (error.status === 401) {
        authService.clearToken();
        router.navigateByUrl('/auth/login');
      }
      return throwError(() => error);
    })
  );
};
```

Update `app.config.ts`:
```typescript
import { authInterceptor } from './core/interceptors/auth.interceptor';

// Change:
provideHttpClient(withInterceptors([authInterceptor])),
```

## Acceptance Criteria

- [ ] `authInterceptor` exists at specified path
- [ ] Token present → `Authorization: Bearer <token>` header added to request
- [ ] No token → request passes through unmodified
- [ ] 401 response → `clearToken()` called, redirect to `/auth/login`
- [ ] Interceptor registered in `app.config.ts` via `withInterceptors([authInterceptor])`
- [ ] `npm run build` exits with code 0
