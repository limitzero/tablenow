# Task 01: Auth HTTP Interceptor

## Status

pending

## Wave

1

## Description

Create a functional HTTP interceptor that attaches the JWT to every outgoing TableNow API request and reacts to authentication failures. It reads the current token from `AuthService` and, when present, clones the request to add an `Authorization: Bearer <token>` header (scoped to the configured API base URL, and only when no `Authorization` header is already set). When the backend returns `401 Unauthorized`, the interceptor clears the stored token via `AuthService.logout()` and redirects the user to `/login`. This removes the need for every feature service to manage the token manually.

## Dependencies

**Depends on:** None (Wave 1) — consumes the STORY-008 `AuthService` contract.
**Blocks:** None

**Context from dependencies:** STORY-008 created `AuthService` at `client/src/app/core/services/auth.service.ts` (`providedIn: 'root'`). It exposes a read-only `token` signal (`token(): string | null`), an `isAuthenticated` computed signal, and a `logout(): void` method that clears `localStorage` and the token signal. This interceptor reads `auth.token()` to attach the header and calls `auth.logout()` on a 401. The `/login` route exists (STORY-008). `environment.apiBaseUrl` is `http://localhost:5000/api`.

## Files to Create

- `client/src/app/core/interceptors/auth.interceptor.ts` — The functional interceptor.
- `client/src/app/core/interceptors/auth.interceptor.spec.ts` — Unit tests (Vitest) for header attachment and 401 handling.

## Files to Modify

- `client/src/app/app.config.ts` — Register the interceptor via `provideHttpClient(withInterceptors([authInterceptor]))`.

## Technical Details

### Implementation Steps

1. Create `authInterceptor` as an `HttpInterceptorFn`.
2. `inject(AuthService)` and `inject(Router)` inside the function body (functional interceptors run in an injection context).
3. Read `const token = auth.token();`.
4. Determine if the request targets the API: `req.url.startsWith(environment.apiBaseUrl)`.
5. If a token exists, the request targets the API, and `!req.headers.has('Authorization')`, clone the request adding the header. Otherwise pass the original request through.
6. Pipe `next(authReq)` through `catchError`. On an `HttpErrorResponse` with `status === 401`, call `auth.logout()` and `router.navigate(['/login'])`, then rethrow with `throwError(() => error)` so callers still observe the failure.
7. Register the interceptor in `app.config.ts`: ensure `provideHttpClient(withInterceptors([authInterceptor]))` is present (add `withInterceptors` if the project currently uses bare `provideHttpClient()`).

### Code Snippets

```ts
// client/src/app/core/interceptors/auth.interceptor.ts
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.token();

  const isApiRequest = req.url.startsWith(environment.apiBaseUrl);
  const shouldAttach = token !== null && isApiRequest && !req.headers.has('Authorization');

  const authReq = shouldAttach
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        auth.logout();
        void router.navigate(['/login']);
      }
      return throwError(() => error);
    }),
  );
};
```

```ts
// app.config.ts (registration excerpt)
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor])),
    // ...other providers
  ],
};
```

## Acceptance Criteria

- [ ] When a token is stored, API requests carry `Authorization: Bearer <token>`.
- [ ] When no token is stored, no `Authorization` header is added.
- [ ] Requests that already include an `Authorization` header are left unchanged.
- [ ] Requests to URLs outside `environment.apiBaseUrl` are not given the header.
- [ ] A `401` response triggers `AuthService.logout()` and a redirect to `/login`, and the error is still propagated to the caller.
- [ ] The interceptor is registered through `provideHttpClient(withInterceptors([...]))` and is a functional `HttpInterceptorFn` (no class).

## Notes

- Use `void router.navigate(...)` to acknowledge the returned promise without awaiting it inside the synchronous error handler.
- Do not retry the request inside the interceptor on 401 — that would risk a loop. Simply downgrade to logged-out and redirect.
