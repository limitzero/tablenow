# Task 03: Module Registration & Configuration

## Status

pending

## Wave

2

## Description

Wire the modular monolith together: give each business context an `Add[Context]Module()` extension method, aggregate them in a single `ServiceCollectionExtensions.RegisterServices()` called from `Program.cs`, and add the `appsettings.json` configuration files with shaped-but-secret-free `Jwt` and `ConnectionStrings` sections. Update `.gitignore` so real secrets never get committed. After this task the host starts and `dotnet build` still passes, giving later stories a single place to register their services.

## Dependencies

**Depends on:** task-01-solution-structure, task-02-shared-result-type
**Blocks:** None

**Context from dependencies:** task-01 created the solution, the `CM.TableNow.Api` host project (with `Extensions/` and a minimal `Program.cs` containing a `// RegisterServices wired in task-03` marker and `public partial class Program;`), and the per-context `Infrastructure`/`Application` projects. task-02 created the `Result<T>` type in `CM.TableNow.Shared`. This task adds the DI wiring and configuration on top of that structure — no business logic yet.

## Files to Create

- `server/src/Infrastructure/Auth/Extensions/AuthModuleExtensions.cs` — `public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)`.
- `server/src/Infrastructure/Restaurants/Extensions/RestaurantsModuleExtensions.cs` — `AddRestaurantsModule(...)`.
- `server/src/Infrastructure/Reservations/Extensions/ReservationsModuleExtensions.cs` — `AddReservationsModule(...)`.
- `server/src/Api/Extensions/ServiceCollectionExtensions.cs` — `RegisterServices(this IServiceCollection, IConfiguration)` aggregating the three module methods and adding Mediator.
- `server/src/Api/appsettings.json` — `Jwt` and `ConnectionStrings` sections, no secrets.
- `server/src/Api/appsettings.Development.json` — dev overrides (SQLite connection string for local dev).

## Files to Modify

- `server/src/Api/Program.cs` — replace the task-01 marker with `builder.Services.RegisterServices(builder.Configuration);`.
- `.gitignore` (repo root) — add entries for secrets / `.env` / user-secrets / local settings.

## Technical Details

### Implementation Steps

1. In each context's `Infrastructure` project, add an `Extensions/` folder with an `Add[Context]Module(IServiceCollection, IConfiguration)` method. For now each method registers nothing context-specific (later stories add handlers, DbContexts, options) and simply returns `services`. Leave a `// TODO(STORY-xxx): register <context> services` comment so later stories know where to extend.
2. In the Api project's `Extensions/ServiceCollectionExtensions.cs`, implement `RegisterServices` that:
   - calls `services.AddMediator(...)` (source-generated Mediator configuration),
   - calls `AddAuthModule`, `AddRestaurantsModule`, `AddReservationsModule`,
   - returns `services`.
3. Update `Program.cs` to call `builder.Services.RegisterServices(builder.Configuration);` before `builder.Build()`.
4. Create `appsettings.json` with the shaped sections below (empty/placeholder values, no real secrets).
5. Create `appsettings.Development.json` with a local SQLite connection string for development (the stories require SQLite for dev, SQL Server for prod).
6. Update the root `.gitignore`.
7. Run `dotnet build server/TableNow.sln` and confirm zero errors.

### Code Snippets

`ServiceCollectionExtensions.cs`:

```csharp
namespace CM.TableNow.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);

        services
            .AddAuthModule(configuration)
            .AddRestaurantsModule(configuration)
            .AddReservationsModule(configuration);

        return services;
    }
}
```

Example module extension:

```csharp
namespace CM.TableNow.Auth.Infrastructure.Extensions;

public static class AuthModuleExtensions
{
    public static IServiceCollection AddAuthModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO(STORY-005/006): register Auth DbContext, handlers, password hasher, JWT service.
        return services;
    }
}
```

`appsettings.json` (no secrets — placeholders only):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Default": ""
  },
  "Jwt": {
    "Issuer": "https://tablenow.local",
    "Audience": "tablenow-client",
    "Secret": "",
    "ExpiryHours": 24
  }
}
```

`appsettings.Development.json` (local SQLite for dev):

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=tablenow.dev.db"
  }
}
```

`.gitignore` additions:

```gitignore
# Secrets & local config
*.env
.env
.env.*
secrets.json
appsettings.*.local.json
**/*.user
# User Secrets are stored outside the repo by dotnet; ensure no local copies are committed
```

### Environment Variables

The JWT secret and production connection string are supplied at runtime (not in this task), via:
- `Jwt__Secret` — the HMAC signing key (configured in STORY-006).
- `ConnectionStrings__Default` — the SQL Server connection string for prod.

These override the empty `appsettings.json` placeholders through standard .NET configuration. They are intentionally left blank in committed files.

## Acceptance Criteria

- [ ] Each context's Infrastructure project exposes an `Add[Context]Module(IServiceCollection, IConfiguration)` method.
- [ ] `Api/Extensions/ServiceCollectionExtensions.cs` defines `RegisterServices` that adds Mediator and calls all three module methods.
- [ ] `Program.cs` calls `builder.Services.RegisterServices(builder.Configuration);`.
- [ ] `appsettings.json` contains a `Jwt` section (Issuer, Audience, Secret, ExpiryHours) and a `ConnectionStrings` section, with the secret/connection values empty (no committed secrets).
- [ ] `appsettings.Development.json` supplies a local SQLite connection string for dev.
- [ ] Root `.gitignore` excludes `.env`, `secrets.json`, `*.user`, and local settings files.
- [ ] `dotnet build server/TableNow.sln` completes with zero errors and the host starts.

## Notes

- Module methods are intentionally near-empty here; the point is to establish the extension-point so later stories register into the correct context without restructuring `Program.cs`.
- Do not configure JWT bearer authentication or CORS in this task — that is STORY-007. Only the configuration *section* shape is created here.
- Verify the `Jwt:Secret` and `ConnectionStrings:Default` committed values are empty strings so no secret leaks into git history.
