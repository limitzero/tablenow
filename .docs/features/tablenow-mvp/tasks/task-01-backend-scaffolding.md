# Task 01: Backend Scaffolding

## Status

pending

## Phase

1

## Description

Create the .NET 10 solution for the TableNow backend as a modular monolith. The solution contains one entry-point API project and separate class library projects for each business context (`Auth`, `Restaurants`, `Reservations`) with layers for Application, Domain, Data, Infrastructure, and Contracts. A shared `TableNow.Shared` project provides the `Result<T>` cross-cutting type used by all handlers. This scaffold is the prerequisite for every other backend task.

## Dependencies

**Depends on:** None (Phase 1)  
**Blocks:** task-03-database-schema, task-05-user-registration-backend

**Context from dependencies:** N/A — this is a root task.

## Files to Create

- `server/TableNow.sln` — Solution file referencing all projects
- `server/src/Api/TableNow.Api.csproj` — Entry-point Web API project (references all context modules)
- `server/src/Api/Program.cs` — Minimal API bootstrap: builder setup, middleware, endpoint registration
- `server/src/Api/ServiceCollectionExtensions.cs` — `RegisterServices()` calling `Add[Context]Module()` for each context
- `server/src/Shared/TableNow.Shared.csproj` — Shared library (no external dependencies)
- `server/src/Shared/Result.cs` — Generic `Result<T>` type in `CM.OpenTable.Common` namespace
- `server/src/Shared/TypedResultHelper.cs` — Maps `Result<T>` → `IResult` for Minimal API endpoints
- `server/src/Application/Auth/TableNow.Auth.Application.csproj`
- `server/src/Domain/Auth/TableNow.Auth.Domain.csproj`
- `server/src/Data/Auth/TableNow.Auth.Data.csproj`
- `server/src/Infrastructure/Auth/TableNow.Auth.Infrastructure.csproj`
- `server/src/Contracts/TableNow.Contracts.csproj` — Shared API-facing request/response models
- `server/src/Application/Restaurants/TableNow.Restaurants.Application.csproj`
- `server/src/Domain/Restaurants/TableNow.Restaurants.Domain.csproj`
- `server/src/Data/Restaurants/TableNow.Restaurants.Data.csproj`
- `server/src/Application/Reservations/TableNow.Reservations.Application.csproj`
- `server/src/Domain/Reservations/TableNow.Reservations.Domain.csproj`
- `server/src/Data/Reservations/TableNow.Reservations.Data.csproj`
- `server/src/Migrations/TableNow.Migrations.csproj` — EF Core migrations project
- `server/tests/UnitTests/TableNow.UnitTests.csproj`
- `server/tests/IntegrationTests/TableNow.IntegrationTests.csproj`
- `server/appsettings.json` (placed at Api level)
- `server/appsettings.Development.json`
- `.gitignore` — covers `*.user`, `bin/`, `obj/`, `.env`, `appsettings.*.Local.json`

## Files to Modify

- (none — greenfield)

## Technical Details

### Implementation Steps

1. **Create the solution and directory tree:**
   ```powershell
   mkdir server
   cd server
   dotnet new sln -n TableNow
   ```

2. **Create all projects using their respective templates:**
   ```powershell
   dotnet new webapi -n TableNow.Api -o src/Api --use-minimal-apis
   dotnet new classlib -n TableNow.Shared -o src/Shared
   dotnet new classlib -n TableNow.Auth.Application -o src/Application/Auth
   dotnet new classlib -n TableNow.Auth.Domain -o src/Domain/Auth
   dotnet new classlib -n TableNow.Auth.Data -o src/Data/Auth
   dotnet new classlib -n TableNow.Auth.Infrastructure -o src/Infrastructure/Auth
   dotnet new classlib -n TableNow.Contracts -o src/Contracts
   dotnet new classlib -n TableNow.Restaurants.Application -o src/Application/Restaurants
   dotnet new classlib -n TableNow.Restaurants.Domain -o src/Domain/Restaurants
   dotnet new classlib -n TableNow.Restaurants.Data -o src/Data/Restaurants
   dotnet new classlib -n TableNow.Reservations.Application -o src/Application/Reservations
   dotnet new classlib -n TableNow.Reservations.Domain -o src/Domain/Reservations
   dotnet new classlib -n TableNow.Reservations.Data -o src/Data/Reservations
   dotnet new classlib -n TableNow.Migrations -o src/Migrations
   dotnet new xunit -n TableNow.UnitTests -o tests/UnitTests
   dotnet new xunit -n TableNow.IntegrationTests -o tests/IntegrationTests
   dotnet sln add (ls -r **/*.csproj)
   ```

3. **Add NuGet packages to `Api`:**
   ```powershell
   dotnet add src/Api package Mediator --version 3.*
   dotnet add src/Api package Scalar.AspNetCore
   dotnet add src/Api package Microsoft.AspNetCore.Authentication.JwtBearer
   ```

4. **Add NuGet packages to `Shared`:**
   ```powershell
   dotnet add src/Shared package FluentValidation
   ```

5. **Add NuGet packages to `UnitTests` and `IntegrationTests`:**
   ```powershell
   dotnet add tests/UnitTests package FluentAssertions
   dotnet add tests/UnitTests package NSubstitute
   dotnet add tests/IntegrationTests package FluentAssertions
   dotnet add tests/IntegrationTests package Microsoft.AspNetCore.Mvc.Testing
   dotnet add tests/IntegrationTests package Testcontainers.MsSql
   ```

6. **Wire project references** (Api → Application, Data, Infrastructure, Shared; Application → Domain, Shared; Data → Domain, Shared):
   ```powershell
   dotnet add src/Api reference src/Shared src/Application/Auth src/Data/Auth src/Infrastructure/Auth src/Application/Restaurants src/Data/Restaurants src/Application/Reservations src/Data/Reservations src/Contracts
   dotnet add src/Application/Auth reference src/Domain/Auth src/Shared
   dotnet add src/Data/Auth reference src/Domain/Auth src/Shared
   dotnet add src/Application/Restaurants reference src/Domain/Restaurants src/Shared
   dotnet add src/Data/Restaurants reference src/Domain/Restaurants src/Shared
   dotnet add src/Application/Reservations reference src/Domain/Reservations src/Shared
   dotnet add src/Data/Reservations reference src/Domain/Reservations src/Shared
   dotnet add src/Migrations reference src/Data/Auth src/Data/Restaurants src/Data/Reservations
   dotnet add tests/IntegrationTests reference src/Api
   dotnet add tests/UnitTests reference src/Application/Auth src/Application/Restaurants src/Application/Reservations src/Shared
   ```

### Code Snippets

**`Result<T>` in `CM.OpenTable.Common` namespace:**
```csharp
// server/src/Shared/Result.cs
namespace CM.OpenTable.Common;

public sealed record Result<T>
{
    public T? Data { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public int StatusCode { get; init; }
    public bool IsSuccess => StatusCode is >= 200 and < 300;

    public static Result<T> Success(T data, int statusCode = 200) =>
        new() { Data = data, StatusCode = statusCode };

    public static Result<T> Failure(int statusCode, params string[] errors) =>
        new() { StatusCode = statusCode, Errors = errors };
}
```

**`TypedResultHelper`:**
```csharp
// server/src/Shared/TypedResultHelper.cs
namespace CM.OpenTable.Common;

public static class TypedResultHelper
{
    public static IResult ToResult<T>(Result<T> result) => result.StatusCode switch
    {
        200 => Results.Ok(result.Data),
        201 => Results.Created(string.Empty, result.Data),
        204 => Results.NoContent(),
        400 => Results.BadRequest(new { errors = result.Errors }),
        401 => Results.Unauthorized(),
        403 => Results.Forbid(),
        404 => Results.NotFound(new { errors = result.Errors }),
        409 => Results.Conflict(new { errors = result.Errors }),
        _   => Results.StatusCode(result.StatusCode)
    };
}
```

**`Program.cs` skeleton:**
```csharp
// server/src/Api/Program.cs
using TableNow.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddScalar();
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

// Endpoints registered in Phase 4–6 tasks
// app.MapAuthEndpoints();
// app.MapRestaurantEndpoints();
// app.MapReservationEndpoints();

app.Run();

public partial class Program { } // needed for WebApplicationFactory in tests
```

**`ServiceCollectionExtensions.cs` skeleton:**
```csharp
// server/src/Api/ServiceCollectionExtensions.cs
namespace TableNow.Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Auth, Restaurants, Reservations modules added in later tasks
        return services;
    }
}
```

**`appsettings.json`:**
```json
{
  "Jwt": {
    "Secret": "",
    "Issuer": "tablenow-api",
    "Audience": "tablenow-client",
    "ExpiryHours": 24
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tablenow.db"
  },
  "Logging": {
    "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
  },
  "AllowedHosts": "*"
}
```

### Environment Variables

- `JWT__Secret` — JWT signing secret (≥ 32 chars). Set in environment or `launchSettings.json` (never committed).
- `ConnectionStrings__DefaultConnection` — Override for SQL Server: `Server=localhost;Database=TableNow;Trusted_Connection=True;`

## Acceptance Criteria

- [ ] `dotnet build` from `server/` succeeds with zero errors and zero warnings
- [ ] Solution file references all projects; `dotnet sln list` shows all 16 projects
- [ ] `server/src/Shared/Result.cs` defines `Result<T>` in namespace `CM.OpenTable.Common` with `Data`, `Errors`, `StatusCode`, `IsSuccess`
- [ ] `appsettings.json` has `Jwt` and `ConnectionStrings` sections; no real secrets in source
- [ ] `.gitignore` excludes `bin/`, `obj/`, `*.user`, `.env`, `appsettings.*.Local.json`

## Notes

- Use `dotnet new webapi --use-minimal-apis` — controller-based APIs are not the pattern for this project.
- The `Program` class must be `public partial class Program {}` so `WebApplicationFactory<Program>` works in integration tests.
- Keep `Program.cs` minimal — all service registration goes through `ServiceCollectionExtensions.RegisterServices()`.
- The `Mediator` package used here is `Mediator` (by martinothamar), not `MediatR`. It is source-generated and does not use reflection.
