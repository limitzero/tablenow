# Requirements: User Sign-In — Backend

## Summary

Registered users authenticate with email and password. On success, the API returns a JWT containing `userId`, `email`, and `role` claims with a 24-hour expiry. Invalid credentials always return 401 with no indication of which field is wrong (security best practice).

## Goals

- Valid credentials return 200 with `{ token, expiresAt }`
- JWT contains `userId`, `email`, `role` claims
- Invalid credentials return 401 (no field-level detail)
- JWT secret comes from configuration, never hardcoded

## Non-Goals

- No refresh tokens (Phase 2+ enhancement)
- No MFA
- No rate limiting (separate infrastructure concern)

## Acceptance Criteria

- [ ] `POST /api/auth/login` with valid credentials returns 200 with `{ token, expiresAt }`
- [ ] JWT decoded contains `userId`, `email`, `role` claims
- [ ] Invalid credentials return 401
- [ ] JWT expiry is 24 hours
- [ ] JWT secret read from `IOptions<JwtOptions>` bound from `appsettings.json`

## Assumptions

- JWT signing algorithm: HS256
- `JwtOptions` bound from `appsettings.json` `Jwt` section

## Technical Constraints

- JWT helper in `Infrastructure/Auth/`
- Application handler in `Application/Auth/Features/Login/`
- Never log or return plaintext password
