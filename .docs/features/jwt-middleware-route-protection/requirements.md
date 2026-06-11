# Requirements: JWT Middleware & Route Protection

## Summary

Configures ASP.NET Core's JWT bearer authentication pipeline and a CORS policy that allows the Angular frontend origin. Reservation endpoints are marked with `RequireAuthorization()`. Unauthenticated requests return 401. The JWT configuration mirrors what STORY-006 uses to generate tokens.

## Goals

- Unauthenticated requests to `/api/reservations` return 401
- Valid JWT requests to protected endpoints proceed normally
- Expired JWT returns 401
- CORS allows `http://localhost:4200` (dev) and production origin

## Non-Goals

- No role-based authorization beyond `RequireAuthorization()`
- No API key authentication
- No OAuth/OIDC

## Acceptance Criteria

- [ ] Unauthenticated `GET /api/reservations/my` returns 401
- [ ] Valid JWT passes through to the handler
- [ ] Expired JWT returns 401
- [ ] CORS preflight from `http://localhost:4200` returns 200 with correct headers

## Technical Constraints

- `Microsoft.AspNetCore.Authentication.JwtBearer` NuGet
- JWT validation parameters must match the `JwtOptions` used in STORY-006
- CORS origin list read from `appsettings.json` `Cors:AllowedOrigins` array
