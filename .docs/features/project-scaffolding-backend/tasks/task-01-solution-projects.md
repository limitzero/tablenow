# Task 01: Create .NET Solution and Project Files

## Status

pending

## Wave

1

## Description

Creates the entire .NET 10 solution scaffold for the TableNow backend: one `.sln` file, one API entry-point project, and a set of class library projects per business context (Auth, Restaurants, Reservations). This task establishes every `.csproj` file, folder path, and project-to-project reference needed so that Phase 2 tasks can immediately start adding code without touching project structure.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-02-shared-library.md, task-03-api-startup.md

**Context from dependencies:** None — this is the foundation task.

## Files to Create

- `server/TableNow.sln` — Solution file referencing all projects
- `server/src/Api/CM.TableNow.Api.csproj` — Entry point (ASP.NET Core web app)
- `server/src/Shared/CM.TableNow.Shared.csproj` — Shared library (Result\<T\> etc.)
- `server/src/Application/CM.TableNow.Auth.Application.csproj`
- `server/src/Application/CM.TableNow.Restaurants.Application.csproj`
- `server/src/Application/CM.TableNow.Reservations.Application.csproj`
- `server/src/Domain/CM.TableNow.Auth.Domain.csproj`
- `server/src/Domain/CM.TableNow.Restaurants.Domain.csproj`
- `server/src/Domain/CM.TableNow.Reservations.Domain.csproj`
- `server/src/Data/CM.TableNow.Auth.Data.csproj`
- `server/src/Data/CM.TableNow.Restaurants.Data.csproj`
- `server/src/Data/CM.TableNow.Reservations.Data.csproj`
- `server/src/Infrastructure/CM.TableNow.Auth.Infrastructure.csproj`
- `server/src/Infrastructure/CM.TableNow.Restaurants.Infrastructure.csproj`
- `server/src/Infrastructure/CM.TableNow.Reservations.Infrastructure.csproj`
- `server/src/Contracts/CM.TableNow.Contracts.csproj` — API-facing request/response models
- `server/src/Migrations/CM.TableNow.Auth.Migrations.csproj`
- `server/src/Migrations/CM.TableNow.Restaurants.Migrations.csproj`
- `server/src/Migrations/CM.TableNow.Reservations.Migrations.csproj`
- `server/tests/UnitTests/CM.TableNow.UnitTests.csproj`
- `server/tests/IntegrationTests/CM.TableNow.IntegrationTests.csproj`
- `server/.gitignore` — Excludes secrets, local settings, build artifacts

## Files to Modify

None — this task creates everything from scratch.

## Technical Details

### Implementation Steps

1. Create the `server/` root directory and run `dotnet new sln -n TableNow` inside it.

2. For each project, run `dotnet new` with the appropriate template:
   - API: `dotnet new webapi -n CM.TableNow.Api --no-openapi` (inside `src/Api/`)
   - Shared + all Application/Domain/Data/Infrastructure/Contracts/Migrations: `dotnet new classlib -n <Name>` in the appropriate subfolder
   - UnitTests + IntegrationTests: `dotnet new xunit -n <Name>` in `tests/`

3. Add all projects to the solution:
   ```powershell
   Get-ChildItem -Recurse -Filter "*.csproj" | ForEach-Object { dotnet sln add $_.FullName }
   ```

4. Set up project references (each layer references the layer below it, within the same context):
   - `Api` → all `Application` projects, `Shared`, `Contracts`
   - `Application/<Context>` → `Domain/<Context>`, `Shared`
   - `Data/<Context>` → `Domain/<Context>`, `Shared`
   - `Infrastructure/<Context>` → `Application/<Context>`, `Shared`
   - `Migrations/<Context>` → `Data/<Context>`
   - `UnitTests` → all `Application` + `Domain` projects
   - `IntegrationTests` → `Api`

5. Add NuGet packages at the solution level:
   ```powershell
   # In src/Api/
   dotnet add package Mediator --version 3.*
   dotnet add package Scalar.AspNetCore
   dotnet add package Microsoft.EntityFrameworkCore.Sqlite
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
   dotnet add package BCrypt.Net-Next

   # In Application projects:
   dotnet add package Mediator
   dotnet add package FluentValidation

   # In Data projects:
   dotnet add package Microsoft.EntityFrameworkCore

   # In Migrations projects:
   dotnet add package Microsoft.EntityFrameworkCore.Tools
   dotnet add package Microsoft.EntityFrameworkCore.Design

   # In test projects:
   dotnet add package FluentAssertions
   dotnet add package NSubstitute
   ```

6. Set `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>` in a `Directory.Build.props` at `server/` root:
   ```xml
   <Project>
     <PropertyGroup>
       <TargetFramework>net10.0</TargetFramework>
       <Nullable>enable</Nullable>
       <ImplicitUsings>enable</ImplicitUsings>
       <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
     </PropertyGroup>
   </Project>
   ```

7. Create `.gitignore` at `server/`:
   ```
   bin/
   obj/
   *.user
   appsettings.*.json
   !appsettings.Development.json
   secrets.json
   .env
   *.env
   ```

### Code Snippets

Each `.csproj` for class libraries should be minimal:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
</Project>
```

The API project:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
</Project>
```

### Folder Structure After Task Completes

```
server/
  Directory.Build.props
  TableNow.sln
  .gitignore
  src/
    Api/CM.TableNow.Api.csproj
    Shared/CM.TableNow.Shared.csproj
    Contracts/CM.TableNow.Contracts.csproj
    Application/
      CM.TableNow.Auth.Application.csproj
      CM.TableNow.Restaurants.Application.csproj
      CM.TableNow.Reservations.Application.csproj
    Domain/
      CM.TableNow.Auth.Domain.csproj
      CM.TableNow.Restaurants.Domain.csproj
      CM.TableNow.Reservations.Domain.csproj
    Data/
      CM.TableNow.Auth.Data.csproj
      CM.TableNow.Restaurants.Data.csproj
      CM.TableNow.Reservations.Data.csproj
    Infrastructure/
      CM.TableNow.Auth.Infrastructure.csproj
      CM.TableNow.Restaurants.Infrastructure.csproj
      CM.TableNow.Reservations.Infrastructure.csproj
    Migrations/
      CM.TableNow.Auth.Migrations.csproj
      CM.TableNow.Restaurants.Migrations.csproj
      CM.TableNow.Reservations.Migrations.csproj
  tests/
    UnitTests/CM.TableNow.UnitTests.csproj
    IntegrationTests/CM.TableNow.IntegrationTests.csproj
```

## Acceptance Criteria

- [ ] `dotnet build` from `server/` exits with code 0 with zero errors
- [ ] All project files listed above exist at the correct paths
- [ ] Project references are wired correctly (Application references Domain + Shared; Api references all Application projects)
- [ ] `Directory.Build.props` sets `net10.0`, nullable enable, implicit usings
- [ ] `.gitignore` excludes secrets and build artifacts

## Notes

Cross-module communication rule: `Application/<ContextA>` must never reference `Application/<ContextB>` or `Data/<ContextB>`. Cross-context calls happen only through `IMediator` using types from `Contracts/`. This is not enforced by project references in this task — it is a coding convention enforced during feature implementation.
