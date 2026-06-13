# Action Required: User Sign-In — Backend

## Before Implementation

- [ ] **Ensure JWT secret is configured** — `appsettings.Development.json` (gitignored) or `dotnet user-secrets` must have `Jwt:Secret` set to a value ≥ 32 characters. The same secret must be used here (token generation) and in STORY-007 (token validation). Run from `./server/src/Api`: `dotnet user-secrets set "Jwt:Secret" "your-256-bit-secret-here"`.

---

> The code binds `JwtOptions` from config but will throw at startup if `Jwt:Secret` is missing or less than 32 characters.
