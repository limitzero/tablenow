# Requirements: User Sign-In — Backend

## Summary

Returning users authenticate with email and password and receive a signed JWT for subsequent API calls. The JWT carries `userId`, `email`, and `role` claims and expires in 24 hours. The secret used to sign the JWT comes from environment configuration — it is never committed to source code.

Invalid credentials (wrong email or wrong password) return the same `401 Unauthorized` with the same message — leaking which field is wrong would aid brute-force attacks.

## Goals

- `POST /api/auth/login` returns 200 with `{token, expiresAt}` on valid credentials
- JWT contains `userId`, `email`, `role` claims; expires in 24 hours
- Invalid credentials return 401 (same message regardless of which field is wrong)
- JWT secret loaded from `appsettings.json:Jwt:Secret` (overridable via env var)

## Non-Goals

- Refresh token flow
- Remember-me / persistent sessions
- Frontend login form (STORY-008)

## Acceptance Criteria

- [ ] Valid credentials return 200 with `token` (JWT) and `expiresAt`
- [ ] Decoded JWT contains `userId`, `email`, `role` claims
- [ ] Invalid password returns 401 with "Invalid credentials"
- [ ] Non-existent email returns 401 with "Invalid credentials" (same message — no oracle)
- [ ] JWT expiry is 24 hours from issuance
- [ ] JWT secret is NOT committed in any source file

## Technical Constraints

- `JwtTokenGenerator` in `Infrastructure/Auth/`
- `JwtOptions` bound from `appsettings.json:Jwt` section via `IOptions<JwtOptions>`
- Same 401 message for wrong email AND wrong password
