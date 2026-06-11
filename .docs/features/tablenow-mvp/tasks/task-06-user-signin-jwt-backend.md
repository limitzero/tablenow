# Task 06: User Sign-In & JWT Backend

## Status

pending

## Phase

4

## Description

Implement `POST /api/auth/login` and configure JWT bearer authentication for the entire API. When a user submits valid credentials, a signed JWT is returned containing `userId`, `email`, and `role` claims. The JWT expires in 24 hours. JWT middleware is configured in the API project so reservation endpoints added in later tasks can simply call `.RequireAuthorization()`. CORS is also configured here to allow the Angular dev client.

## Dependencies

**Depends on:** task-05-user-registration-backend  
**Blocks:** task-08-auth-frontend, task-09-reservation-creation-backend

**Context from dependencies:** task-05 created `GetUserByEmailQueryHandler` (returns a `UserModel?` given an email), `AuthEndpoints.cs` with `MapAuthEndpoints()`, and `AuthMapper.cs`. The `UserModel` has `Id`, `Email`, `PasswordHash`, `Role` fields. `AppDbContext` and the Mediator pattern are in place.

## Files to Create

- `server/src/Infrastructure/Auth/JwtOptions.cs` — strongly-typed JWT config options
- `server/src/Infrastructure/Auth/JwtTokenGenerator.cs` — generates signed JWTs from a `UserModel`
- `server/src/Application/Auth/Features/Login/LoginRequest.cs`
- `server/src/Application/Auth/Features/Login/LoginResponse.cs`
- `server/src/Application/Auth/Features/Login/LoginRequestHandler.cs`

## Files to Modify

- `server/src/Api/Auth/AuthEndpoints.cs` — add `POST /auth/login` route
- `server/src/Api/Auth/AuthMapper.cs` — add `ToLoginRequest` and `ToLoginResponse` mappers
- `server/src/Contracts/Auth/` — add `LoginRequest.cs` and `LoginResponse.cs` API DTOs
- `server/src/Api/ServiceCollectionExtensions.cs` — add JWT auth + CORS + `AddAuthModule` (if not already done)
- `server/src/Application/Auth/AuthModule.cs` — register `JwtTokenGenerator` in DI

## Technical Details

### Implementation Steps

1. **Add JWT package:**
   ```powershell
   dotnet add src/Api package Microsoft.AspNetCore.Authentication.JwtBearer
   dotnet add src/Infrastructure/Auth package Microsoft.IdentityModel.Tokens
   dotnet add src/Infrastructure/Auth package System.IdentityModel.Tokens.Jwt
   ```

2. **Define `JwtOptions`** bound from `appsettings.json` `Jwt` section.

3. **Write `JwtTokenGenerator`** that builds a signed JWT.

4. **Write `LoginRequestHandler`** — verifies BCrypt hash, calls `JwtTokenGenerator`.

5. **Add `POST /auth/login`** to `AuthEndpoints`.

6. **Register JWT authentication** in `ServiceCollectionExtensions`:
   - `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)`
   - `AddCors(...)` allowing `http://localhost:4200`

7. **Add `UseAuthentication()` and `UseAuthorization()`** to `Program.cs` (confirm already present from task-01 scaffold).

### Code Snippets

**`JwtOptions.cs`:**
```csharp
namespace TableNow.Auth.Infrastructure;

public sealed class JwtOptions
{
    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = "tablenow-api";
    public string Audience { get; set; } = "tablenow-client";
    public int ExpiryHours { get; set; } = 24;
}
```

**`JwtTokenGenerator.cs`:**
```csharp
namespace TableNow.Auth.Infrastructure;

public sealed class JwtTokenGenerator(IOptions<JwtOptions> options)
{
    public (string Token, DateTime ExpiresAt) Generate(Guid userId, string email, string role)
    {
        var opts = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(opts.ExpiryHours);

        var claims = new[]
        {
            new Claim("userId", userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
        };

        var token = new JwtSecurityToken(
            issuer: opts.Issuer,
            audience: opts.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
```

**`LoginRequestHandler.cs`:**
```csharp
namespace TableNow.Auth.Application.Features.Login;

public sealed class LoginRequestHandler(IMediator mediator, JwtTokenGenerator jwt)
    : IRequestHandler<LoginRequest, Result<LoginResponse>>
{
    public async ValueTask<Result<LoginResponse>> Handle(
        LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await mediator.Send(new GetUserByEmailQuery(request.Email), cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure(401, "Invalid credentials.");

        var (token, expiresAt) = jwt.Generate(user.Id, user.Email, user.Role);
        return Result<LoginResponse>.Success(new LoginResponse(token, expiresAt));
    }
}
```

**JWT authentication registration in `ServiceCollectionExtensions`:**
```csharp
var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>()!;
services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
services.AddSingleton<JwtTokenGenerator>();

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Secret)),
        };
    });

services.AddAuthorization();

services.AddCors(opts =>
    opts.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()));
```

**`Program.cs` additions (after `Build()`, before endpoint mapping):**
```csharp
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
```

**API response shapes:**
```json
// POST /api/v1/auth/login — 200
{ "token": "eyJ...", "expiresAt": "2026-06-11T10:00:00Z" }

// 401
{ "errors": ["Invalid credentials."] }
```

**JWT payload (decoded):**
```json
{
  "userId": "uuid",
  "email": "alice@tablenow.dev",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role": "diner",
  "exp": 1749636000
}
```

### Environment Variables

- `JWT__Secret` — HMAC-SHA256 signing secret, minimum 32 characters. Must be set in environment or `launchSettings.json`. Never committed.

## Acceptance Criteria

- [ ] `POST /api/v1/auth/login` with valid credentials returns 200 with `token` and `expiresAt`
- [ ] Invalid credentials return 401 (no hint about which field is wrong)
- [ ] Decoded JWT contains `userId`, `email`, and `role` claims
- [ ] JWT expiry is 24 hours from issue time
- [ ] A request to any protected endpoint without a JWT returns 401
- [ ] CORS allows requests from `http://localhost:4200`

## Notes

- Never return a different 401 message for "user not found" vs "wrong password" — this prevents email enumeration.
- `JwtTokenGenerator` is registered as a singleton — it holds no per-request state.
- The `userId` claim uses a custom claim name `"userId"` (not `ClaimTypes.NameIdentifier`) to make it easy to extract in later handlers via `context.User.FindFirstValue("userId")`.
- The JWT secret is validated at startup — throw if `JwtOptions.Secret` is null or < 32 chars in non-development environments.
