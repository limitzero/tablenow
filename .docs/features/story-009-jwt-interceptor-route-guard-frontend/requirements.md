# Requirements: JWT Interceptor & Route Guard — Frontend

## Summary

With the auth pages and `AuthService` in place (STORY-008), the TableNow frontend needs authentication enforced automatically rather than per-component. Without an interceptor, every service would have to manually attach the bearer token; without a guard, protected routes would render before redirecting unauthenticated users.

This feature adds two cross-cutting pieces to `core/`: a functional HTTP interceptor that attaches `Authorization: Bearer <token>` to outgoing API requests using the token from `AuthService`, and that reacts to a `401` response by clearing the token and redirecting to `/login`; and a functional `canActivate` route guard that reads the `isAuthenticated` computed signal and redirects unauthenticated users to `/login`.

The expected outcome is transparent auth: authenticated users move through guarded routes with the token attached on every call, and any expired or invalid session is cleanly downgraded to a redirect to the login page.

## Goals

- Provide a functional HTTP interceptor in `core/interceptors/auth.interceptor.ts` that attaches the bearer header from `AuthService` to outgoing API requests.
- Have the interceptor handle `401` responses by clearing the token (`AuthService.logout()`) and redirecting to `/login`.
- Provide a functional route guard in `core/guards/auth.guard.ts` (`canActivate`) that reads the `isAuthenticated` signal and redirects to `/login` when false.
- Register the interceptor and guard in the application's root configuration so they apply globally.

## Non-Goals

- The `AuthService` itself, the login/register pages, and the auth routes (delivered in STORY-008).
- Refresh-token rotation or silent re-authentication.
- Role/permission-based authorization (only authenticated-vs-not is in scope here).
- Backend JWT validation (delivered in STORY-007).
- Attaching the token to non-API / cross-origin requests.

## Acceptance Criteria

- [ ] With a stored JWT, any API request has `Authorization: Bearer <token>` attached automatically.
- [ ] With no JWT in storage, navigating to a guarded route redirects the user to `/login`.
- [ ] A `401` response received by the interceptor clears the token and redirects the user to `/login`.
- [ ] With a valid token, navigation to guarded routes proceeds normally.

## Assumptions

- STORY-008 is complete: `AuthService` (`core/services/auth.service.ts`) exposes a read-only `token` signal, an `isAuthenticated` computed signal, `login(token)`, and `logout()`.
- The app uses `provideHttpClient(withInterceptors([...]))` (functional interceptors) and `provideRouter(...)` from the standalone bootstrap.
- `environment.apiBaseUrl` identifies API requests so the interceptor can scope the header to API calls only.
- The `/login` route exists (created in STORY-008).

## Technical Constraints

- Functional interceptor (`HttpInterceptorFn`) and functional guard (`CanActivateFn`) — no class-based `HttpInterceptor` or `CanActivate` interface implementations.
- Dependency resolution inside interceptor/guard via the `inject()` function.
- The interceptor must not attach the token to requests that already carry an explicit `Authorization` header, and should scope attachment to the configured API base URL.
- The interceptor must not introduce an infinite loop on `401` (it must not re-trigger itself when redirecting).
- No `.subscribe()` leaks — use RxJS operators (`catchError`, `throwError`) within the interceptor pipeline.
