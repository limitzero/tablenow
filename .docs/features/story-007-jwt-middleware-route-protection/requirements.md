# Requirements: JWT Middleware & Route Protection — Backend

## Summary

Before the reservation endpoints can be built (STORY-014+), the authentication middleware must be in place. This story configures JWT Bearer authentication using the secret from `JwtOptions`, adds CORS to allow the Angular dev server at `http://localhost:4200`, and applies `.RequireAuthorization()` to the reservations route group. The auth and restaurant routes remain open.

## Goals

- JWT Bearer authentication configured from `JwtOptions` (validates issuer, audience, lifetime, signature)
- CORS policy allowing `http://localhost:4200` (dev) with all methods and headers
- `/api/reservations` route group requires authorization
- `/api/auth` and `/api/restaurants` remain unauthenticated
- Expired/invalid JWT returns 401 (not a redirect)

## Non-Goals

- Role-based authorization policies (added per-endpoint in later stories)
- HTTPS redirect configuration
- Production CORS origins (added during deployment setup)

## Acceptance Criteria

- [ ] Unauthenticated request to `/api/reservations` returns 401
- [ ] Valid JWT on a protected endpoint returns 200 (request processed normally)
- [ ] Expired JWT returns 401
- [ ] CORS allows `http://localhost:4200` for all methods
- [ ] `app.UseAuthentication()` called before `app.UseAuthorization()`

## Technical Constraints

- Use `Microsoft.AspNetCore.Authentication.JwtBearer` (already in Api.csproj from STORY-001)
- `JwtBearerEvents.OnChallenge` must be overridden to suppress the default redirect and return 401
- `TokenValidationParameters.ClockSkew` should be minimal (e.g., `TimeSpan.Zero`) for predictable expiry
