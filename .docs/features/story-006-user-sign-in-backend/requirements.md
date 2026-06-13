# Requirements: User Sign-In — Backend

## Summary

TableNow needs a login endpoint so that registered users can authenticate and receive a JWT token for accessing protected reservation endpoints. The endpoint accepts email and password, verifies the BCrypt hash, and — if valid — issues a JWT containing `userId`, `email`, and `role` claims with a 24-hour expiry.

Following the project's CQRS architecture, the request flows from the Minimal API → `LoginRequest` (Application) → `GetUserByEmailQuery` (Data) for user lookup, then to `JwtTokenGenerator` (Infrastructure) for token creation. Invalid credentials return a generic 401 without disclosing which field is wrong (timing-safe: BCrypt comparison always runs). The JWT secret comes from `IOptions<JwtOptions>` bound from environment config.

## Goals

- `POST /api/auth/login` returns 200 with `{ token, expiresAt }` for valid credentials.
- Invalid credentials return 401 Unauthorized without disclosing which field is wrong.
- JWT contains `userId`, `email`, and `role` claims; expires in 24 hours.
- JWT secret comes from environment configuration, never from source code.
- BDD tests: `describe_login` with `when_credentials_are_valid` and `when_credentials_are_invalid`.

## Non-Goals

- No refresh token.
- No account lockout after N failures.
- No multi-factor authentication.
- No remember-me / extended session.

## Acceptance Criteria

- [ ] `POST /api/auth/login` with valid credentials returns 200 with `token` and `expiresAt`.
- [ ] Invalid credentials (wrong password or unknown email) return 401 without detail about which field is wrong.
- [ ] The JWT contains `userId`, `email`, and `role` claims.
- [ ] JWT expiry is 24 hours from issuance.
- [ ] JWT secret is not present in any committed `appsettings.json`.

## Assumptions

- STORY-003 created the `User` EF model with `Id`, `Email`, `PasswordHash`, `Role` in `AuthDbContext`.
- STORY-005 has seeded or created at least one user whose BCrypt hash can be verified.
- `JwtOptions` (`Secret`, `Issuer`, `Audience`, `ExpiryHours`) will be reused by STORY-007 for validation — they must match.

## Technical Constraints

- Handler: `LoginRequest` / `LoginResponse` / `LoginRequestHandler` in `Application/Auth/Features/Login/`.
- Data query: `GetUserByEmailQuery` / `UserData` / `GetUserByEmailQueryHandler` in `Data/Auth/Queries/GetUserByEmail/`.
- JWT helper: `JwtTokenGenerator` in `Infrastructure/Auth/` — `GenerateToken(Guid userId, string email, string role)`.
- BCrypt comparison always runs even for unknown emails (use a dummy hash) to prevent timing attacks.
- File-scoped namespaces, nullable enabled, records for DTOs, `CancellationToken` on all async methods.
