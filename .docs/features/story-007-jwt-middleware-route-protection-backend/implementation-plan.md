# Implementation Plan: JWT Middleware & Route Protection — Backend

## Phase 1 — Middleware and Route Protection

Single phase — all changes live in the API startup project.

- [ ] **task-01-jwt-bearer-cors-setup** — Register `JwtOptions` from `appsettings.json`, add JWT bearer middleware with full token validation parameters, configure a named CORS policy for the Angular origin, and apply `.RequireAuthorization()` to the reservations endpoint group.

### Technical Details

```powershell
dotnet add server/src/Api package Microsoft.AspNetCore.Authentication.JwtBearer
```

**`appsettings.json` Jwt section:**
```json
"Jwt": {
  "Issuer": "tablenow",
  "Audience": "tablenow-clients",
  "ExpiryHours": 24
}
```
Secret is supplied via `appsettings.Development.json` or environment variable `Jwt__Secret`.

**`JwtOptions`** record: `Secret`, `Issuer`, `Audience`, `ExpiryHours`.

**Token validation parameters:**
- `ValidateIssuer = true`, `ValidIssuer = options.Issuer`
- `ValidateAudience = true`, `ValidAudience = options.Audience`
- `ValidateLifetime = true`
- `ValidateIssuerSigningKey = true`, key from `options.Secret`
- `ClockSkew = TimeSpan.Zero` (no grace period on expiry)

**CORS** named policy `"angular-client"`:
- `WithOrigins("http://localhost:4200")`
- `AllowAnyHeader()`, `AllowAnyMethod()`

Apply `.RequireAuthorization()` on the reservations route group in `ReservationsEndpoints`.
