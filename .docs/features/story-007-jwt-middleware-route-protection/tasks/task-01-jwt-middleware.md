# Task 01: JWT Middleware & CORS

## Status

pending

## Wave

1

## Description

Adds JWT Bearer authentication and CORS to the `Program.cs` / startup pipeline. Protects the `/api/reservations` route group with `.RequireAuthorization()`. Sets `OnChallenge` to return 401 JSON instead of the default browser redirect. All auth and restaurant routes remain open.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-006 task-01 JwtOptions to exist)
**Blocks:** STORY-014 (reservation endpoint needs this middleware), STORY-008 (Angular login needs CORS to work)

**Context from dependencies:** STORY-006 task-01 created `JwtOptions` in `Infrastructure/Auth/` and registered it in `AddAuthModule`. `AuthEndpoints` and `RestaurantsEndpoints` from STORY-005/010 are registered without auth. This task adds the middleware configuration that enforces those auth rules.

## Files to Modify

- `server/src/Api/Program.cs` — add auth middleware and CORS
- `server/src/Api/Extensions/AuthExtensions.cs` — (create) extracts auth + CORS setup

## Technical Details

### Implementation Steps

1. Create `AuthExtensions.cs` helper to keep `Program.cs` clean:

```csharp
// server/src/Api/Extensions/AuthExtensions.cs
namespace TableNow.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuth(
        this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"error\":\"Unauthorized\"}");
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularDev", policy =>
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials());
        });
        return services;
    }
}
```

2. Update `Program.cs` to call these extensions and apply `.RequireAuthorization()` to the reservations group:

```csharp
// server/src/Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .RegisterServices(builder.Configuration)
    .AddJwtAuth(builder.Configuration)
    .AddCorsPolicy();

var app = builder.Build();

app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api/auth").MapAuthEndpoints();
app.MapGroup("/api/restaurants").MapRestaurantsEndpoints();
app.MapGroup("/api/reservations").MapReservationsEndpoints().RequireAuthorization();

app.Run();
```

### Environment Variables

- `Jwt__Secret` — required at runtime for JWT validation (set via env var, not `appsettings.json`)

## Acceptance Criteria

- [ ] `app.UseAuthentication()` is called before `app.UseAuthorization()`
- [ ] JWT Bearer configured with `ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `ValidateIssuerSigningKey`
- [ ] `ClockSkew = TimeSpan.Zero` (predictable expiry)
- [ ] `OnChallenge` returns 401 JSON (not a redirect)
- [ ] CORS allows `http://localhost:4200` with any method and headers
- [ ] `/api/reservations` group has `.RequireAuthorization()`
- [ ] `/api/auth` and `/api/restaurants` groups do NOT have `.RequireAuthorization()`
