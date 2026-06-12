# Implementation Plan: JWT Interceptor & Route Guard — Frontend

Two independent tasks, both in Phase 1, both consuming the STORY-008 `AuthService`.

## Phase 1 — Interceptor + Guard (parallel)

- [ ] **task-01-auth-interceptor** — Create `client/src/app/core/interceptors/auth.interceptor.ts`.
  - Functional `HttpInterceptorFn`.
  - `inject(AuthService)` to read `auth.token()`; if a token exists and the request targets `environment.apiBaseUrl` and has no existing `Authorization` header, clone the request adding `Authorization: Bearer <token>`.
  - Pipe the response through `catchError`: on `HttpErrorResponse` with `status === 401`, call `auth.logout()` and `inject(Router).navigate(['/login'])`, then `return throwError(() => error)`.
  - Register in `app.config.ts` via `provideHttpClient(withInterceptors([authInterceptor]))`.

- [ ] **task-02-auth-guard** — Create `client/src/app/core/guards/auth.guard.ts`.
  - Functional `CanActivateFn`.
  - `inject(AuthService)` and `inject(Router)`; if `auth.isAuthenticated()` return `true`, else return a `UrlTree` to `/login` (preferred over imperative navigate inside a guard).
  - Apply to protected routes (e.g. `/reservations`) via `canActivate: [authGuard]` in later stories; this story only creates and exports the guard and demonstrates registration.

## Verification

- `npm run lint` — zero errors.
- `npm run build` — succeeds.
- `npm run test` — unit tests: interceptor attaches header when token present, skips when absent or header already set, and on 401 calls `logout()` + redirects; guard returns `true` when authenticated and a `/login` `UrlTree` when not.
