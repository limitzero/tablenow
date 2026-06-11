# Task 01: JWT Bearer Authentication Middleware

## Status

pending

## Wave

1

## Description

Configures `AddAuthentication().AddJwtBearer()` in the API startup so the ASP.NET Core pipeline can validate incoming JWTs. The validation parameters must exactly match the JWT settings used in STORY-006 (`JwtOptions`: Issuer, Audience, Secret).

## Dependencies

**Depends on:** STORY-006 task-01-jwt-infrastructure.md (JwtOptions must exist)
**Blocks:** task-02-cors-authorization.md

**Context from dependencies:**
- STORY-006 task-01 created `JwtOptions` with `Secret`, `Issuer`, `Audience`, `ExpiryHours` bound from `appsettings.json` `Jwt` section
- STORY-001 task-03 created `ServiceCollectionExtensions.RegisterServices()` — this task adds JWT auth registration there
- STORY-001 task-03 `Program.cs` already calls `app.UseAuthentication()` and `app.UseAuthorization()` — this task just needs the service registration

## Files to Modify

- `server/src/Api/Extensions/ServiceCollectionExtensions.cs` — Add `AddAuthentication().AddJwtBearer()`

## Technical Details

### Code Snippets

Add to `RegisterServices()` in `ServiceCollectionExtensions.cs`:
```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// Inside RegisterServices():
var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, // Strict expiry
        };
    });

services.AddAuthorization();
```

### NuGet Packages Required

```powershell
dotnet add server/src/Api package Microsoft.AspNetCore.Authentication.JwtBearer
```
(This was already added in STORY-001 task-01 — verify it's present in the .csproj before adding again.)

## Acceptance Criteria

- [ ] `services.AddAuthentication()` and `services.AddAuthorization()` are called in `RegisterServices()`
- [ ] JWT validation parameters use the same `Issuer`, `Audience`, and signing key as `JwtTokenGenerator`
- [ ] `ClockSkew` is set to `TimeSpan.Zero` for strict expiry
- [ ] `dotnet build` exits with code 0
