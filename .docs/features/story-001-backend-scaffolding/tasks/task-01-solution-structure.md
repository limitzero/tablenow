# Task 01: Solution Structure

## Status

pending

## Wave

1

## Description

Creates the .NET 10 solution file and all 17 project files for the TableNow backend. No implementation code is written — only the structural skeleton with correct cross-project references and NuGet package declarations. All subsequent backend tasks across all stories depend on this project structure existing.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-02-shared-result-type.md, task-03-module-registration.md

**Context from dependencies:** None — this is the first backend task.

## Files to Create

- `server/Directory.Build.props` — shared TFM, nullable, implicit usings
- `server/tablenow.sln` — solution file
- `server/src/Api/Api.csproj` — web entry point (`Sdk.Web`)
- `server/src/Api/Program.cs` — minimal stub
- `server/src/Application/Auth/Application.Auth.csproj`
- `server/src/Application/Restaurants/Application.Restaurants.csproj`
- `server/src/Application/Reservations/Application.Reservations.csproj`
- `server/src/Domain/Auth/Domain.Auth.csproj`
- `server/src/Domain/Restaurants/Domain.Restaurants.csproj`
- `server/src/Domain/Reservations/Domain.Reservations.csproj`
- `server/src/Data/Auth/Data.Auth.csproj`
- `server/src/Data/Restaurants/Data.Restaurants.csproj`
- `server/src/Data/Reservations/Data.Reservations.csproj`
- `server/src/Contracts/Contracts.csproj`
- `server/src/Shared/Shared.csproj`
- `server/src/Infrastructure/Auth/Infrastructure.Auth.csproj`
- `server/src/Infrastructure/Notifications/Infrastructure.Notifications.csproj`
- `server/src/Migrations/Migrations.csproj`
- `server/tests/UnitTests/UnitTests.csproj`
- `server/tests/IntegrationTests/IntegrationTests.csproj`

## Technical Details

### Implementation Steps

1. Create `server/Directory.Build.props` with shared properties for all projects.

2. Create each `.csproj`. Class libraries use `Microsoft.NET.Sdk`. The `Api` project uses `Microsoft.NET.Sdk.Web`. Test projects include `<IsPackable>false</IsPackable>`.

3. Add NuGet packages to the correct projects:
   - `Api.csproj`: `Mediator` (3.x), `Mediator.SourceGenerator` (3.x, OutputItemType=Analyzer), `FluentValidation.AspNetCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`
   - `Data/*.csproj`: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Sqlite`
   - `Infrastructure.Auth.csproj`: `BCrypt.Net-Next`, `System.IdentityModel.Tokens.Jwt`, `Microsoft.Extensions.Options`
   - `Infrastructure.Notifications.csproj`: `SendGrid`, `Polly`, `Microsoft.Extensions.Http.Polly`
   - `Migrations.csproj`: `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Sqlite`
   - `UnitTests.csproj`: `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions`, `Microsoft.NET.Test.Sdk`
   - `IntegrationTests.csproj`: `Microsoft.AspNetCore.Mvc.Testing`, `Testcontainers.MsSql`, `FluentAssertions`, `Microsoft.NET.Test.Sdk`

4. Wire cross-project references:
   - `Application/*` → `Domain/same-context`, `Shared`, `Contracts`
   - `Data/*` → `Domain/same-context`, `Shared`
   - `Infrastructure/*` → `Shared`
   - `Api` → all `Application/*`, all `Data/*`, all `Infrastructure/*`, `Contracts`, `Shared`
   - `Migrations` → all `Data/*`, all `Domain/*`, `Shared`
   - `UnitTests` → all `Application/*`, all `Domain/*`, `Shared`, `Contracts`
   - `IntegrationTests` → `Api`

5. Run `dotnet sln tablenow.sln add` for each project to register in solution.

6. Create a minimal `server/src/Api/Program.cs` stub so the solution compiles.

### Code Snippets

```xml
<!-- server/Directory.Build.props -->
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

```xml
<!-- server/src/Api/Api.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <AssemblyName>TableNow.Api</AssemblyName>
    <RootNamespace>TableNow.Api</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Mediator" Version="3.*" />
    <PackageReference Include="Mediator.SourceGenerator" Version="3.*" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.*" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\Application\Auth\Application.Auth.csproj" />
    <ProjectReference Include="..\..\Application\Restaurants\Application.Restaurants.csproj" />
    <ProjectReference Include="..\..\Application\Reservations\Application.Reservations.csproj" />
    <ProjectReference Include="..\..\Infrastructure\Auth\Infrastructure.Auth.csproj" />
    <ProjectReference Include="..\..\Infrastructure\Notifications\Infrastructure.Notifications.csproj" />
    <ProjectReference Include="..\..\Contracts\Contracts.csproj" />
    <ProjectReference Include="..\..\Shared\Shared.csproj" />
  </ItemGroup>
</Project>
```

```csharp
// server/src/Api/Program.cs (minimal stub — expanded in later tasks)
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.Run();
```

## Acceptance Criteria

- [ ] `dotnet build server/tablenow.sln` succeeds with zero errors
- [ ] All 17 projects exist with correct `.csproj` SDK types
- [ ] Cross-project references are wired as specified (Application refs Domain+Shared, Data refs Domain+Shared, Api refs all modules)
- [ ] NuGet packages are declared in the correct project files
- [ ] `Directory.Build.props` sets `net10.0`, `Nullable=enable`, `ImplicitUsings=enable`
