# Task 01: JWT Bearer & CORS Setup

## Status

pending

## Wave

1

## Description

Configure JWT Bearer authentication middleware and CORS policy in the TableNow API so that reservation endpoints are protected and Angular can make cross-origin requests. This task adds the `JwtOptions` settings class, binds it from `appsettings.json`, registers `AddAuthentication().AddJwtBearer(...)` with full token validation parameters (issuer, audience, lifetime, signing key), adds a named CORS policy for the Angular dev origin, and applies `.RequireAuthorization()` to the reservations route group. After this task, unauthenticated requests to `/api/reservations` return 401 and a request with a valid JWT proceeds normally.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** None

**Context from dependencies:** This task assumes STORY-006 already wired up `JwtOptions` and a JWT generation helper in `Infrastructure/Auth/` — the same secret, issuer, and audience values must be used here for token validation to succeed. If STORY-006 is not yet complete, the validation parameters can be configured consistently and the middleware will work once Login is implemented.

## Files to Create

- `server/src/Infrastructure/Auth/JwtOptions.cs` — Options record (if not already created by STORY-006).

## Files to Modify

- `server/src/Api/Program.cs` — Add authentication, authorization, and CORS service registrations; call `app.UseAuthentication()`, `app.UseAuthorization()`, `app.UseCors(...)`.
- `server/src/Api/appsettings.json` — Add `Jwt` section with `Issuer`, `Audience`, `ExpiryHours` (secret via env override only).
- `server/src/Api/Endpoints/ReservationsEndpoints.cs` — Add `.RequireAuthorization()` to the reservations route group.

## Technical Details

### Implementation Steps

1. Install the JWT bearer package: `dotnet add server/src/Api package Microsoft.AspNetCore.Authentication.JwtBearer`.
2. Define `JwtOptions` in `Infrastructure/Auth/` (skip if STORY-006 already created it):
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
3. In `appsettings.json`, add the `Jwt` section (no secret here):
   ```json
   "Jwt": {
     "Issuer": "tablenow",
     "Audience": "tablenow-clients",
     "ExpiryHours": 24
   }
   ```
4. In `Program.cs`, bind options and register authentication:
   ```csharp
   builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
   var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
   var key = Encoding.UTF8.GetBytes(jwtOptions.Secret);

   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidIssuer = jwtOptions.Issuer,
               ValidateAudience = true,
               ValidAudience = jwtOptions.Audience,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               IssuerSigningKey = new SymmetricSecurityKey(key),
               ClockSkew = TimeSpan.Zero,
           };
       });
   builder.Services.AddAuthorization();
   ```
5. Add CORS policy:
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("angular-client", policy =>
           policy.WithOrigins("http://localhost:4200")
                 .AllowAnyHeader()
                 .AllowAnyMethod());
   });
   ```
6. In the middleware pipeline (order matters):
   ```csharp
   app.UseCors("angular-client");
   app.UseAuthentication();
   app.UseAuthorization();
   ```
7. In `ReservationsEndpoints.cs`, apply auth to the route group:
   ```csharp
   var group = app.MapGroup("/api/reservations").RequireAuthorization();
   ```

### Environment Variables

- `Jwt__Secret` — The JWT signing secret; minimum 32 characters. Set in `appsettings.Development.json` (gitignored) or via `dotnet user-secrets`.

## Acceptance Criteria

- [ ] `GET /api/reservations/my` without a token returns 401 Unauthorized.
- [ ] `GET /api/reservations/my` with a valid JWT returns a non-401 response.
- [ ] An expired JWT returns 401 Unauthorized.
- [ ] CORS preflight from `http://localhost:4200` receives a 204 with the correct headers.
- [ ] `Jwt:Secret` is not present in any committed `appsettings.json` file.

## Notes

- `app.UseAuthentication()` must come before `app.UseAuthorization()` in the middleware pipeline.
- `app.UseCors(...)` must come before `UseAuthentication()` so CORS headers are added even on 401 responses — otherwise the Angular client will see a network error rather than an auth error.
- `ClockSkew = TimeSpan.Zero` means tokens expire exactly at their `exp` claim. This is intentional per the story's acceptance criteria.
- For the production Angular origin, add the URL to a `Cors:AllowedOrigins` config array and read it dynamically — do not hard-code prod URLs in source.
