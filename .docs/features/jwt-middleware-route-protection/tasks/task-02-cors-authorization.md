# Task 02: CORS Policy & Route Authorization

## Status

pending

## Wave

2

## Description

Configures a named CORS policy allowing the Angular dev origin and applies `RequireAuthorization()` to the reservations endpoint group. Also applies the CORS middleware to the pipeline. After this task, unauthenticated reservation requests return 401 and the Angular frontend's CORS preflight succeeds.

## Dependencies

**Depends on:** task-01-jwt-bearer.md
**Blocks:** STORY-014 (reservation endpoints rely on this protection)

**Context from dependencies:**
- task-01 registered authentication and authorization services
- STORY-001 task-03 `Program.cs` calls `app.UseAuthentication()` and `app.UseAuthorization()` — `UseCors()` must be added before these
- `appsettings.json` has `Cors: { AllowedOrigins: ["http://localhost:4200"] }`

## Files to Modify

- `server/src/Api/Extensions/ServiceCollectionExtensions.cs` — Add `AddCors()` with named policy
- `server/src/Api/Program.cs` — Add `app.UseCors()`, apply `RequireAuthorization()` to reservation routes

## Technical Details

### Code Snippets

Add to `RegisterServices()`:
```csharp
var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

services.AddCors(options =>
{
    options.AddPolicy("TableNowCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

Update `Program.cs` to use CORS (before UseAuthentication):
```csharp
app.UseCors("TableNowCors");
app.UseAuthentication();
app.UseAuthorization();
```

When reservation endpoints are mapped (STORY-014), apply authorization:
```csharp
// In Program.cs — added by STORY-014 but protected by this story's middleware:
app.MapGroup("/api")
   .MapAuthEndpoints()       // Public
   .MapRestaurantEndpoints()  // Public
   // MapReservationEndpoints().RequireAuthorization() — added in STORY-014
   ;
```

Add a guard comment in `Program.cs` near the endpoint group:
```csharp
// Reservation endpoints are mapped by STORY-014 and require authorization.
// Ensure app.UseAuthentication() and app.UseAuthorization() are called above.
```

## Acceptance Criteria

- [ ] CORS preflight from `http://localhost:4200` to any `/api` route returns 200 with `Access-Control-Allow-Origin`
- [ ] `app.UseCors()` is called before `app.UseAuthentication()` in `Program.cs`
- [ ] `AllowedOrigins` is read from `appsettings.json` (not hardcoded)
- [ ] Unauthenticated `GET /api/reservations/my` (once STORY-016 adds that endpoint) returns 401
- [ ] `dotnet build` exits with code 0
