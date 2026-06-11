# Task 03: Configure API Startup & Dependency Injection

## Status

pending

## Wave

2

## Description

Wires up `Program.cs`, `appsettings.json`, and the `ServiceCollectionExtensions` registration pattern so the API host starts cleanly. Each business context self-registers via an `Add[Context]Module()` extension method called from `RegisterServices()`. This task also creates the `appsettings.json` template with Jwt and ConnectionStrings sections, and establishes the Scalar OpenAPI documentation endpoint.

## Dependencies

**Depends on:** task-01-solution-projects.md
**Blocks:** Nothing directly — subsequent feature stories add their own `Add[Context]Module()` calls.

**Context from dependencies:** task-01 created `server/src/Api/CM.TableNow.Api.csproj` with all necessary NuGet packages. task-02 (running in parallel) creates the `Result<T>` and `TypedResultHelper` types in Shared.

## Files to Create

- `server/src/Api/Program.cs` — Application entry point, middleware pipeline
- `server/src/Api/Extensions/ServiceCollectionExtensions.cs` — `RegisterServices()` orchestrator
- `server/src/Api/appsettings.json` — Template with Jwt and ConnectionStrings placeholders
- `server/src/Api/appsettings.Development.json.example` — Example dev config (committed, no real secrets)

## Files to Modify

None.

## Technical Details

### Implementation Steps

1. Write `Program.cs` with the standard minimal API builder pattern.
2. Create `ServiceCollectionExtensions.cs` with `RegisterServices()` that calls placeholder `Add[Context]Module()` stubs (each returns immediately — real implementations come in feature stories).
3. Create `appsettings.json` with placeholder sections.
4. Create the example development settings file.

### Code Snippets

```csharp
// Program.cs
using CM.TableNow.Api.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Endpoint groups are mapped by feature stories via Map[Context]Endpoints()
// app.MapGroup("/api").MapAuthEndpoints();  <- added per feature story

app.Run();

public partial class Program { }  // needed for WebApplicationFactory in integration tests
```

```csharp
// Extensions/ServiceCollectionExtensions.cs
namespace CM.TableNow.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Each context module registers itself.
        // These stubs will be replaced with real implementations in feature stories.
        // services.AddAuthModule(configuration);
        // services.AddRestaurantsModule(configuration);
        // services.AddReservationsModule(configuration);

        return services;
    }
}
```

```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Secret": "REPLACE_WITH_ENV_OR_SECRET",
    "Issuer": "CM.TableNow",
    "Audience": "CM.TableNow.Client",
    "ExpiryHours": 24
  },
  "ConnectionStrings": {
    "DefaultConnection": "REPLACE_WITH_ENV_OR_SECRET"
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:4200" ]
  }
}
```

```json
// appsettings.Development.json.example
{
  "Jwt": {
    "Secret": "dev-secret-at-least-32-characters-long-for-hmac"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tablenow.db"
  }
}
```

### Environment Variables

- `Jwt__Secret` — JWT signing secret (min 32 chars for HS256). Overrides appsettings value.
- `ConnectionStrings__DefaultConnection` — Database connection string.

## Acceptance Criteria

- [ ] `dotnet run` from `server/src/Api/` starts the API without errors
- [ ] `appsettings.json` has `Jwt` and `ConnectionStrings` sections with no real secrets committed
- [ ] `appsettings.Development.json` is git-ignored; `.example` variant is committed
- [ ] `public partial class Program { }` exists at the bottom of `Program.cs` for integration test compatibility
- [ ] Scalar API docs available at `/scalar/v1` in Development mode
- [ ] `dotnet build` exits with code 0

## Notes

The commented-out `Add[Context]Module()` calls in `RegisterServices()` are intentional stubs. Each feature story will uncomment and implement them. Keep the comments so it's obvious where to add new modules.
