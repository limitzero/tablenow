# Action Required: JWT Middleware & Route Protection — Backend

## Before Implementation

- [ ] **Set the JWT secret in local environment config** — `appsettings.Development.json` (gitignored) or a `dotnet user-secrets` entry for `Jwt:Secret`. The secret must be at least 32 characters. Example: `dotnet user-secrets set "Jwt:Secret" "your-256-bit-secret-here-at-least-32-chars"` run from `./server/src/Api`.

---

> This task is also referenced in `task-01-jwt-bearer-cors-setup.md`. The code configures the binding but will fail at startup if no secret is provided.
