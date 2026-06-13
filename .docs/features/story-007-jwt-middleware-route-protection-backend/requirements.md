# Requirements: JWT Middleware & Route Protection — Backend

## Summary

The TableNow API must enforce authentication on reservation endpoints so that only users with a valid JWT can create, list, or cancel reservations. This story adds the JWT Bearer authentication middleware to the ASP.NET Core pipeline, configures a CORS policy that permits requests only from the known Angular origin, and marks reservation endpoint groups as requiring authorization.

JWT configuration (secret, issuer, audience, expiry) is loaded from `appsettings.json` and overridable via environment variables — the secret is never committed to source code. An expired or malformed token returns 401. A valid token allows the request through so downstream handlers can read the `userId` claim.

The expected outcome is that unauthenticated requests to `/api/reservations` return 401, valid-token requests succeed, and the Angular dev client at `http://localhost:4200` can make cross-origin requests without CORS errors.

## Goals

- JWT bearer middleware registered and validating tokens on every request.
- `JwtOptions` bound from `appsettings.json` (secret via environment override only, never in source).
- CORS policy allows `http://localhost:4200` (dev) and the production Angular origin.
- All `/api/reservations` endpoint groups marked `.RequireAuthorization()`.
- Expired or invalid tokens return 401 Unauthorized.

## Non-Goals

- No role-based authorization (RBAC) — that comes later when operator features are added.
- No refresh-token endpoint — only the Bearer validation middleware is in scope here.
- No changes to the auth endpoints (`/api/auth/*`) — they remain public.
- No restaurant endpoints authorization (they stay public per STORY-010).

## Acceptance Criteria

- [ ] An unauthenticated request to `/api/reservations` returns 401 Unauthorized.
- [ ] A request with a valid JWT to a protected endpoint is processed normally.
- [ ] An expired JWT returns 401 Unauthorized.
- [ ] CORS policy allows `http://localhost:4200` in development.
- [ ] JWT secret is loaded from environment configuration, not source code.

## Assumptions

- STORY-006 has implemented the Login endpoint that issues JWTs with the same secret, issuer, and audience configured here — both must be consistent.
- `JwtOptions` class (with `Secret`, `Issuer`, `Audience`, `ExpiryHours`) is defined or will be defined in `Infrastructure/Auth/`.
- The reservation endpoint group exists (or a placeholder exists) so `.RequireAuthorization()` can be applied without a runtime error.

## Technical Constraints

- Use `Microsoft.AspNetCore.Authentication.JwtBearer` NuGet package.
- JWT validation parameters: validate issuer, audience, lifetime, and signing key.
- `JwtOptions.Secret` must have a minimum length of 32 characters (256-bit key for HS256).
- File-scoped namespaces, nullable enabled, primary constructors where applicable.
- Do not store secrets in `appsettings.json` — use `appsettings.Development.json` (gitignored) or environment variables.
