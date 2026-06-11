# Requirements: Project Scaffolding — Backend

## Summary

The TableNow backend requires a .NET 10 solution structured as a modular monolith with CQRS. Three business contexts — Auth, Restaurants, and Reservations — each live in separate class library projects. This prevents cross-context coupling and makes each context independently testable.

The project structure follows the pattern: `server/src/{Layer}/{Context}` where each layer (Application, Domain, Data, Infrastructure) has a corresponding project per business context named `CM.TableNow.{Context}.{Layer}`. The API project is the single entry point that composes all modules.

The goal is a fully compilable solution with zero boilerplate errors, correct project references, and a Shared library providing the `Result<T>` return type that all handlers will use throughout the application.

## Goals

- Solution compiles with `dotnet build` from `./server` with zero errors
- All three business context projects (Auth, Restaurants, Reservations) are scaffolded with correct layer projects
- `Result<T>` type is available in the Shared project for use by all handlers
- `appsettings.json` has sections for Jwt and ConnectionStrings with no secrets committed
- `.gitignore` excludes secrets and environment files

## Non-Goals

- No actual business logic implementations (those are in subsequent stories)
- No database migrations (STORY-003)
- No endpoint implementations (per-feature stories)
- No CI/CD pipeline setup

## Acceptance Criteria

- [ ] `dotnet build` from `./server` exits with code 0
- [ ] All expected project folders exist per the architecture layout
- [ ] `Result<T>` in Shared has `Data`, `Errors`, `StatusCode`, and `IsSuccess` members
- [ ] `appsettings.json` contains `Jwt` and `ConnectionStrings` sections with placeholder values
- [ ] `.gitignore` excludes `*.env`, `appsettings.*.json` (except Development), `secrets.json`

## Assumptions

- Company/product naming convention is `CM.TableNow`
- Business contexts are: `Auth`, `Restaurants`, `Reservations`
- SQLite is used for local development; SQL Server for production
- The solution root is `./server/`

## Technical Constraints

- .NET 10 target framework throughout (`net10.0`)
- Mediator NuGet package: `Mediator` (source-generated, by martinothamar)
- File-scoped namespaces in all C# files
- Nullable enabled in all projects
- No AutoMapper anywhere in the solution
