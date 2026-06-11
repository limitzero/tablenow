# Task 01: JWT Helper

## Status

pending

## Wave

1

## Description

Creates the JWT infrastructure in `Infrastructure/Auth/`: a strongly-typed options class (`JwtOptions`) bound from config, and a `JwtTokenGenerator` service that produces signed JWTs. This is the foundation that the login handler calls to issue tokens.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-001 Infrastructure/Auth project)
**Blocks:** task-02-login-handler.md

**Context from dependencies:** STORY-001 created `server/src/Infrastructure/Auth/Infrastructure.Auth.csproj` with `BCrypt.Net-Next` and `System.IdentityModel.Tokens.Jwt` packages. The `AuthModuleRegistration.cs` stub is where options binding and service registration will be added. This task creates the two new files.

## Files to Create

- `server/src/Infrastructure/Auth/JwtOptions.cs`
- `server/src/Infrastructure/Auth/JwtTokenGenerator.cs`

## Files to Modify

- `server/src/Infrastructure/Auth/AuthModuleRegistration.cs` — add JwtOptions binding and register JwtTokenGenerator

## Technical Details

### Implementation Steps

1. Create `JwtOptions` as a record bound from the `Jwt` config section.
2. Create `JwtTokenGenerator` that generates a HS256-signed JWT.
3. Update `AuthModuleRegistration.AddAuthModule()` to bind options and register the generator.

### Code Snippets

```csharp
// server/src/Infrastructure/Auth/JwtOptions.cs
namespace TableNow.Infrastructure.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = "tablenow";
    public string Audience { get; init; } = "tablenow-users";
    public int ExpiryHours { get; init; } = 24;
}
```

```csharp
// server/src/Infrastructure/Auth/JwtTokenGenerator.cs
namespace TableNow.Infrastructure.Auth;

public class JwtTokenGenerator(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public (string Token, DateTimeOffset ExpiresAt) Generate(Guid userId, string email, string role)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddHours(_options.ExpiryHours);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
```

```csharp
// Update AuthModuleRegistration.cs
public static IServiceCollection AddAuthModule(
    this IServiceCollection services, IConfiguration configuration)
{
    services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
    services.AddSingleton<JwtTokenGenerator>();
    return services;
}
```

### Environment Variables

- `Jwt__Secret` — override via environment variable (double-underscore = nested section separator in .NET)

## Acceptance Criteria

- [ ] `JwtOptions` exists and binds from the `Jwt` config section
- [ ] `JwtTokenGenerator.Generate` returns a valid signed JWT and `DateTimeOffset expiresAt`
- [ ] Generated JWT contains `userId`, email, and role claims
- [ ] `JwtTokenGenerator` is registered as singleton in `AddAuthModule`
