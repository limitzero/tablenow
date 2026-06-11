# Task 03: Module Registration & Configuration

## Status

pending

## Wave

2

## Description

Creates the module self-registration pattern and baseline configuration. Each business context (Auth, Restaurants, Reservations) self-registers via its own `Add[Context]Module()` extension method. The Api project's `ServiceCollectionExtensions.RegisterServices()` calls all three. Also creates `appsettings.json` with placeholder `Jwt` and `ConnectionStrings` sections and a `.gitignore` that prevents committing secrets.

## Dependencies

**Depends on:** task-01-solution-structure.md
**Blocks:** All stories that add services (STORY-005+)

**Context from dependencies:** task-01 created all project files including `Infrastructure/Auth`, `Infrastructure/Notifications`, and the `Api` project. This task wires them together via extension methods and creates the startup configuration files. Does not overlap with task-02 (different files).

## Files to Create

- `server/src/Api/Extensions/ServiceCollectionExtensions.cs` — `RegisterServices()` calling all module registrations
- `server/src/Infrastructure/Auth/AuthModuleRegistration.cs` — `AddAuthModule()` stub
- `server/src/Infrastructure/Notifications/NotificationsModuleRegistration.cs` — `AddNotificationsModule()` stub
- `server/src/Data/Restaurants/RestaurantsDataRegistration.cs` — `AddRestaurantsModule()` stub
- `server/src/Data/Reservations/ReservationsDataRegistration.cs` — `AddReservationsModule()` stub
- `server/src/Api/appsettings.json` — Jwt + ConnectionStrings sections
- `server/src/Api/appsettings.Development.json` — SQLite dev connection string
- `server/.gitignore` — exclude bin/, obj/, secrets

## Technical Details

### Implementation Steps

1. Create `ServiceCollectionExtensions.cs` in `server/src/Api/Extensions/` with a single `RegisterServices(IServiceCollection services, IConfiguration configuration)` method that calls all module registrations and Mediator registration.

2. Create stub `Add[Context]Module()` extension methods in each Infrastructure/Data project. These return `IServiceCollection` for chaining. Add a TODO comment — each story will fill them in.

3. Update `Program.cs` to call `builder.Services.RegisterServices(builder.Configuration)`.

4. Create `appsettings.json` with placeholder sections — never commit real values.

5. Create `appsettings.Development.json` with SQLite connection string for local dev.

6. Create `.gitignore` at `server/` root.

### Code Snippets

```csharp
// server/src/Api/Extensions/ServiceCollectionExtensions.cs
namespace TableNow.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
        services.AddHttpContextAccessor();

        services.AddAuthModule(configuration);
        services.AddRestaurantsModule(configuration);
        services.AddReservationsModule(configuration);
        services.AddNotificationsModule(configuration);

        return services;
    }
}
```

```csharp
// server/src/Infrastructure/Auth/AuthModuleRegistration.cs
namespace TableNow.Infrastructure.Auth;

public static class AuthModuleRegistration
{
    public static IServiceCollection AddAuthModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: STORY-006 adds JWT token generator and options binding
        return services;
    }
}
```

```json
// server/src/Api/appsettings.json
{
  "Logging": {
    "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Secret": "SET_VIA_ENVIRONMENT_VARIABLE",
    "Issuer": "tablenow",
    "Audience": "tablenow-users",
    "ExpiryHours": 24
  },
  "ConnectionStrings": {
    "Default": "SET_VIA_ENVIRONMENT_VARIABLE"
  },
  "Email": {
    "ApiKey": "SET_VIA_ENVIRONMENT_VARIABLE",
    "FromAddress": "noreply@tablenow.com",
    "FromName": "TableNow"
  }
}
```

```json
// server/src/Api/appsettings.Development.json
{
  "ConnectionStrings": {
    "Default": "Data Source=tablenow-dev.db"
  }
}
```

```gitignore
# server/.gitignore
bin/
obj/
*.user
appsettings.Production.json
appsettings.Staging.json
*.pfx
*.p12
tablenow-dev.db
tablenow-dev.db-shm
tablenow-dev.db-wal
```

## Acceptance Criteria

- [ ] `RegisterServices()` calls `AddAuthModule()`, `AddRestaurantsModule()`, `AddReservationsModule()`, `AddNotificationsModule()`, and `AddMediator()`
- [ ] Each module has its own `Add[Context]Module()` extension method
- [ ] `Program.cs` calls `builder.Services.RegisterServices(builder.Configuration)`
- [ ] `appsettings.json` has `Jwt` and `ConnectionStrings` sections with no real secrets
- [ ] `appsettings.Development.json` has SQLite connection string
- [ ] `.gitignore` excludes `bin/`, `obj/`, `appsettings.Production.json`
