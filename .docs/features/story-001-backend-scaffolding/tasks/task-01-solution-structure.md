# Task 01: Solution & Project Structure

## Status

pending

## Wave

1

## Description

Create the complete .NET 10 modular-monolith solution skeleton for TableNow: a solution file, the Api host project, the per-business-context class library projects across every layer, the cross-cutting `Shared` and `Contracts` projects, and the two test projects. This is the foundation every other backend story builds on, so the solution must compile cleanly with `dotnet build` and follow the folder/naming rules in `.docs/design/backend/backend-project-structure.md`.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-03-module-registration

**Context from dependencies:** None. This is a greenfield task — no prior task output is required. It produces the project layout and `.csproj` references that task-03 (module registration) and all subsequent backend stories depend on.

## Files to Create

Solution and host:
- `server/TableNow.sln` — solution file referencing every project below.
- `server/src/Api/CM.TableNow.Api.csproj` — ASP.NET Core minimal-API host (`<OutputType>` exe, web SDK).
- `server/src/Api/Program.cs` — minimal `WebApplication` bootstrap (builder + `app.Run()`); leaves a `// RegisterServices wired in task-03` marker.
- `server/src/Api/Endpoints/.gitkeep` — placeholder for endpoint classes.
- `server/src/Api/Extensions/.gitkeep` — placeholder for DI extension classes.
- `server/src/Api/Mappers/.gitkeep` — placeholder for `[Context]Mapper` static classes.

Shared & Contracts:
- `server/src/Shared/CM.TableNow.Shared.csproj` — class library (no dependencies on other layers).
- `server/src/Contracts/CM.TableNow.Contracts.csproj` — class library for API-facing request/response models and `Contracts.Public` cross-module contracts.

Per-context layer projects (for each context `Auth`, `Restaurants`, `Reservations`):
- `server/src/Application/<Context>/CM.TableNow.<Context>.Application.csproj` — references `Shared`, `Domain` (same context), `Contracts`, Mediator.
- `server/src/Application/<Context>/Features/.gitkeep` — use-case folders go here.
- `server/src/Domain/<Context>/CM.TableNow.<Context>.Domain.csproj` — references `Shared` only; no EF, no Mediator.
- `server/src/Data/<Context>/CM.TableNow.<Context>.Data.csproj` — references `Shared`, `Domain` (same context), Mediator, EF Core.
- `server/src/Data/<Context>/Models/.gitkeep` — EF models go here.
- `server/src/Data/<Context>/Configurations/.gitkeep` — Fluent API configs go here.
- `server/src/Data/<Context>/Commands/.gitkeep` and `server/src/Data/<Context>/Queries/.gitkeep`.
- `server/src/Infrastructure/<Context>/CM.TableNow.<Context>.Infrastructure.csproj` — references `Shared`, `Application` (same context), config packages.
- `server/src/Migrations/<Context>/CM.TableNow.<Context>.Migrations.csproj` — references the context's `Data` project and EF Core Design.

Tests:
- `server/tests/UnitTests/CM.TableNow.UnitTests.csproj` — xUnit + FluentAssertions; references Application and Domain projects.
- `server/tests/IntegrationTests/CM.TableNow.IntegrationTests.csproj` — xUnit + FluentAssertions + `Microsoft.AspNetCore.Mvc.Testing`; references the Api project.

## Files to Modify

- None (greenfield).

## Technical Details

### Implementation Steps

1. Create the solution: `dotnet new sln -n TableNow -o server`.
2. Create the Api host: `dotnet new web -n CM.TableNow.Api -o server/src/Api`. Strip the default minimal endpoint down to a bare bootstrap.
3. Create the `Shared` and `Contracts` class libraries: `dotnet new classlib`.
4. For each context (`Auth`, `Restaurants`, `Reservations`), create the five layer libraries (`Application`, `Domain`, `Data`, `Infrastructure`, `Migrations`) with `dotnet new classlib` at the paths listed above.
5. Create the two test projects: `dotnet new xunit`.
6. Add every project to the solution with `dotnet sln server/TableNow.sln add <path>`.
7. Wire project references (`dotnet add <proj> reference <proj>`) following the dependency rules below.
8. Add NuGet packages to the appropriate projects (see Package matrix).
9. Set common properties in every `.csproj`: `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, `<LangVersion>latest</LangVersion>`, `<TargetFramework>net10.0</TargetFramework>`.
10. Run `dotnet build server/TableNow.sln` and confirm zero errors.

### Project reference rules

- `Domain.<Context>` → `Shared` only.
- `Application.<Context>` → `Shared`, `Domain.<Context>`, `Contracts`, Mediator.
- `Data.<Context>` → `Shared`, `Domain.<Context>`, Mediator, EF Core.
- `Infrastructure.<Context>` → `Shared`, `Application.<Context>`.
- `Migrations.<Context>` → `Data.<Context>`.
- `Api` → all `Application.<Context>`, all `Infrastructure.<Context>`, all `Data.<Context>` (for DI registration), `Contracts`, `Shared`.
- **Forbidden:** one context's Application/Data referencing another context's Application/Data. Cross-module communication uses `IMediator` + the `Contracts` (`Contracts.Public`) assembly only.

### Package matrix

| Project | Packages |
|---|---|
| `Application.<Context>` | `Mediator.Abstractions`, `FluentValidation` |
| `Data.<Context>` | `Microsoft.EntityFrameworkCore`, `Mediator.Abstractions` |
| `Migrations.<Context>` | `Microsoft.EntityFrameworkCore.Design` |
| `Api` | `Mediator.SourceGenerator`, `Scalar.AspNetCore`, `Microsoft.EntityFrameworkCore.Design` |
| `UnitTests` | `xunit`, `FluentAssertions`, `Microsoft.NET.Test.Sdk` |
| `IntegrationTests` | `xunit`, `FluentAssertions`, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.NET.Test.Sdk` |

Use the source-generated Mediator: `Mediator.SourceGenerator` analyzer in the Api project (the composition root) and `Mediator.Abstractions` everywhere handlers/requests are declared.

### Code Snippets

Minimal `Program.cs` bootstrap (to be expanded by task-03):

```csharp
var builder = WebApplication.CreateBuilder(args);

// RegisterServices wired in task-03 (story-001) via builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

app.Run();

// Exposed for WebApplicationFactory<Program> integration tests.
public partial class Program;
```

Common `.csproj` property block:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
```

## Acceptance Criteria

- [ ] `server/TableNow.sln` exists and references the Api, Shared, Contracts, all three contexts' Application/Domain/Data/Infrastructure/Migrations projects, and both test projects.
- [ ] All projects target `net10.0` with nullable and implicit usings enabled.
- [ ] Project references follow the rules above; no cross-context Application/Data reference exists.
- [ ] The Api project has `Endpoints/`, `Extensions/`, and `Mappers/` folders.
- [ ] Each Application context has a `Features/` folder; each Data context has `Models/`, `Configurations/`, `Commands/`, and `Queries/` folders.
- [ ] `Program.cs` declares `public partial class Program;` so integration tests can use `WebApplicationFactory<Program>`.
- [ ] `dotnet build server/TableNow.sln` completes with zero errors.

## Notes

- CLAUDE.md references the `Result<T>` type as living in `CM.OpenTable.Common`; for this project use the `CM.TableNow.Shared` project. Keep the type's public shape (`Data`, `Errors`, `StatusCode`, `IsSuccess`) consistent with task-02 — do not implement `Result<T>` here, just ensure the `Shared` project exists and is referenced.
- Do not add the JWT bearer or CORS packages here — those belong to STORY-007.
- `.gitkeep` files keep empty folders under source control; remove them as real files are added in later stories.
