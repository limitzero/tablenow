# Task 02: JWT Token Generator

## Status

pending

## Wave

1

## Description

Implement the JWT token generation infrastructure helper used by the Login Application handler. `JwtTokenGenerator` takes the user's `Id`, `email`, and `role`, signs a HS256 JWT with the configured secret and expiry, and returns the serialized token string plus the `expiresAt` timestamp. It reads configuration from `IOptions<JwtOptions>`. This class lives in `Infrastructure/Auth/` and is registered in the DI container during module setup.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-03-login-application.md

**Context from dependencies:** This is a Wave 1 task. `JwtOptions` (`Secret`, `Issuer`, `Audience`, `ExpiryHours`) will be bound from `appsettings.json` by STORY-007's middleware setup — the same values are used here for generation and in STORY-007 for validation. Both must be consistent. The `System.IdentityModel.Tokens.Jwt` package provides `JwtSecurityTokenHandler`.

## Files to Create

- `server/src/Infrastructure/Auth/JwtOptions.cs` — `JwtOptions` record bound from config.
- `server/src/Infrastructure/Auth/JwtTokenGenerator.cs` — Token generation class with `GenerateToken(...)`.

## Files to Modify

- `server/src/Api/Extensions/ServiceCollectionExtensions.cs` — Register `JwtTokenGenerator` as a singleton and bind `JwtOptions`.

## Technical Details

### Implementation Steps

1. Install JWT package in the Infrastructure/Auth project: `dotnet add server/src/Infrastructure/Auth package System.IdentityModel.Tokens.Jwt`.
2. Create `JwtOptions` with properties `Secret`, `Issuer`, `Audience`, `ExpiryHours`.
3. Create `JwtTokenGenerator` with primary constructor `(IOptions<JwtOptions> options)`.
4. Implement `GenerateToken(Guid userId, string email, string role)` returning a `(string Token, DateTimeOffset ExpiresAt)` tuple.
5. Build claims: `sub = userId.ToString()`, `email = email`, `role = role`, `jti = Guid.NewGuid().ToString()`.
6. Sign with `HmacSha256Signature` using `new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.Secret))`.
7. Set `notBefore = DateTime.UtcNow`, `expires = DateTime.UtcNow.AddHours(options.Value.ExpiryHours)`.
8. Register `builder.Services.AddSingleton<JwtTokenGenerator>()` in `ServiceCollectionExtensions`.

### Code Snippets

```csharp
namespace CM.TableNow.Infrastructure.Auth;

public sealed record JwtOptions
{
    public required string Secret { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int ExpiryHours { get; init; } = 24;
}
```

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CM.TableNow.Infrastructure.Auth;

public sealed class JwtTokenGenerator(IOptions<JwtOptions> options)
{
    public (string Token, DateTimeOffset ExpiresAt) GenerateToken(
        Guid userId, string email, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(options.Value.ExpiryHours);

        var token = new JwtSecurityToken(
            issuer: options.Value.Issuer,
            audience: options.Value.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            ],
            notBefore: DateTime.UtcNow,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
```

## Acceptance Criteria

- [ ] `GenerateToken` returns a non-empty JWT string and a future `ExpiresAt`.
- [ ] The decoded JWT contains `sub` (userId), `email`, and `role` claims.
- [ ] The JWT is signed with HS256 using the configured secret.
- [ ] The `ExpiresAt` value is approximately `now + JwtOptions.ExpiryHours`.
- [ ] `JwtTokenGenerator` is registered in the DI container.

## Notes

- The `JwtOptions` record is shared between this class (generation) and STORY-007 (validation middleware). Defining it here and referencing it there keeps a single source of truth for the options shape.
