# Requirements: JWT Interceptor & Route Guard — Frontend

## Summary

The JWT obtained at login must be attached to every API request automatically. When the backend returns 401 (expired or invalid token), the interceptor clears the stored token and redirects to `/login`. A `canActivate` route guard protects routes like `/reservations` from unauthenticated access.

## Goals

- HTTP interceptor adds `Authorization: Bearer <token>` to every outbound API request
- Interceptor handles 401 response: clears token, navigates to /login
- Route guard reads `isAuthenticated` signal; redirects to /login if false
- Both registered in `app.config.ts`

## Non-Goals

- Token refresh logic
- Per-request opt-out of auth header (all requests get the header if token exists)

## Acceptance Criteria

- [ ] Every API request includes `Authorization: Bearer <token>` when token is present
- [ ] No token → no Authorization header (requests still go through)
- [ ] 401 response: token cleared, user redirected to /login
- [ ] Navigating to a guarded route without token redirects to /login
- [ ] Valid token lets navigation proceed normally

## Technical Constraints

- `HttpInterceptorFn` functional pattern (not class-based)
- `CanActivateFn` functional pattern (not class-based)
- Both registered in `app.config.ts` via `withInterceptors`
