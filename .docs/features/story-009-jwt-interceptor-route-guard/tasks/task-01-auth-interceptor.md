# Task 01: Auth Interceptor

## Status

pending

## Wave

1

## Description

Creates the `HttpInterceptorFn` that adds the JWT Bearer token to every API request and handles 401 responses by clearing the token and redirecting to login. Registered in `app.config.ts`.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-008 task-01 AuthService)
**Blocks:** STORY-012 (restaurant listing needs auth header on API calls), all protected endpoint stories

**Context from dependencies:** STORY-008 task-01 created `AuthService` in `core/services/auth.service.ts` with `getToken()` and `clearToken()` methods. This task creates the interceptor that consumes those methods. Does not overlap with task-02 (different file).

## Files to Create

- `client/src/app/core/interceptors/auth.interceptor.ts`

## Files to Modify

- `client/src/app/app.config.ts` — register interceptor with `withInterceptors`
- `client/src/app/core/index.ts` — add interceptor export

## Technical Details

### Code Snippets

```typescript
// client/src/app/core/interceptors/auth.interceptor.ts
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { EMPTY, catchError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getToken();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401) {
        authService.clearToken();
        router.navigate(['/login']);
        return EMPTY;
      }
      throw err;
    })
  );
};
```

```typescript
// app.config.ts update
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
  ]
};
```

## Acceptance Criteria

- [ ] Every HTTP request with a stored token includes `Authorization: Bearer <token>` header
- [ ] Requests without a token are forwarded without an Authorization header
- [ ] 401 response: `authService.clearToken()` called, user navigated to /login, request stream completes (EMPTY)
- [ ] Interceptor registered in `app.config.ts` via `withInterceptors`
