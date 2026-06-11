# Task 01: JWT Infrastructure

## Status

pending

## Wave

1

## Description

Creates `JwtOptions` (bound from `appsettings.json`) and `JwtTokenGenerator` in the `Infrastructure/Auth` project. The token generator produces HS256 JWTs with `userId`, `email`, and `role` claims and a configurable expiry. This is a pure infrastructure service with no business logic dependency.

## Dependencies

**Depends on:** STORY-001 task-01-solution-projects.md
**Blocks:** task-02-login-handler.md

**Context from dependencies:** STORY-001 task-01 created `CM.TableNow.Auth.Infrastructure.csproj`. The `appsettings.json` from STORY-001 task-03 has a `Jwt` section: `{ Secret, Issuer, Audience, ExpiryHours }`.

## Files to Create

- `server/src/Infrastructure/CM.TableNow.Auth.Infrastructure/JwtOptions.cs`
- `server/src/Infrastructure/CM.TableNow.Auth.Infrastructure/JwtTokenGenerator.cs`
- `server/src/Infrastructure/CM.TableNow.Auth.Infrastructure/Extensions/ServiceCollectionExtensions.cs`

## Files to Modify

None.

## Technical Details

### Code Snippets

```csharp
// JwtOptions.cs
namespace CM.TableNow.Auth.Infrastructure;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "CM.TableNow";
    public string Audience { get; set; } = "CM.TableNow.Client";
    public int ExpiryHours { get; set; } = 24;
}
```

```csharp
// JwtTokenGenerator.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CM.TableNow.Auth.Infrastructure;

public class JwtTokenGenerator(IOptions<JwtOptions> jwtOptions)
{
    private readonly JwtOptions _options = jwtOptions.Value;

    public (string Token, DateTime ExpiresAt) GenerateToken(Guid userId, string email, string role)
    {
        var expiresAt = DateTime.UtcNow.AddHours(_options.ExpiryHours);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
```

```csharp
// Extensions/ServiceCollectionExtensions.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CM.TableNow.Auth.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<JwtTokenGenerator>();
        return services;
    }
}
```

### NuGet Packages Required

```powershell
dotnet add server/src/Infrastructure/CM.TableNow.Auth.Infrastructure package Microsoft.IdentityModel.Tokens
dotnet add server/src/Infrastructure/CM.TableNow.Auth.Infrastructure package System.IdentityModel.Tokens.Jwt
```

## Acceptance Criteria

- [ ] `JwtOptions` has `Secret`, `Issuer`, `Audience`, `ExpiryHours` properties
- [ ] `JwtTokenGenerator.GenerateToken()` returns a valid HS256 JWT with `sub`, `email`, `role` claims
- [ ] `AddAuthInfrastructure()` registers `JwtOptions` and `JwtTokenGenerator`
- [ ] `dotnet build` exits with code 0
