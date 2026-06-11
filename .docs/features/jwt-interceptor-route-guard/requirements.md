# Requirements: JWT Interceptor & Route Guard

## Summary

The HTTP interceptor attaches `Authorization: Bearer <token>` to every API request, handling 401 responses by clearing the token and redirecting to `/login`. The route guard prevents navigation to protected routes without a valid token.

## Goals

- All API requests automatically include the JWT header if a token is stored
- 401 responses trigger token clear + redirect to `/login`
- Guarded routes redirect unauthenticated users to `/login`
- Valid token allows navigation to proceed

## Non-Goals

- No token refresh logic
- No per-route role checks

## Acceptance Criteria

- [ ] Stored JWT is attached as `Authorization: Bearer <token>` header on every API request
- [ ] No JWT: no header added, no error thrown
- [ ] 401 response: token cleared from localStorage, redirect to `/login`
- [ ] `canActivate` guard returns `UrlTree('/login')` when unauthenticated

## Technical Constraints

- Interceptor: `client/src/app/core/interceptors/auth.interceptor.ts`
- Guard: `client/src/app/core/guards/auth.guard.ts`
- Use functional interceptor API (Angular 17+): `HttpInterceptorFn`
- Use functional guard API: `CanActivateFn`
- Interceptor must be added to `withInterceptors([])` in `app.config.ts`
