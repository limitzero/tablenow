# Requirements: Project Scaffolding — Backend

## Summary

TableNow's backend is a .NET 10 modular monolith using CQRS via the source-generated [Mediator](https://github.com/martinothamar/Mediator) library. Before any feature work can begin, the solution needs a consistent, ready-to-use project foundation: a solution file, layer projects (Api, Application, Domain, Data, Contracts, Shared, Infrastructure, Migrations), per-business-context class libraries (Auth, Restaurants, Reservations), and the two test projects (UnitTests, IntegrationTests).

This story produces that scaffold. It establishes the folder layout from `.docs/design/backend/backend-project-structure.md`, the cross-cutting `Result<T>` type that every handler returns, and the module-registration mechanism (`Add[Context]Module()`) wired through `ServiceCollectionExtensions.RegisterServices()`. Configuration files (`appsettings.json` with `Jwt` and `ConnectionStrings` sections) are created with placeholder, secret-free values, and `.gitignore` is updated to keep secrets out of source control.

The expected outcome is that `dotnet build` from `./server` compiles with zero errors and that all subsequent backend stories can add features by dropping use-case folders into the appropriate context project without restructuring.

## Goals

- A `.sln` at `./server` referencing all layer and context projects plus both test projects.
- Per-context class library projects named `CM.TableNow.<BusinessContext>.<Layer>` for the Auth, Restaurants, and Reservations contexts.
- An Api host project (`CM.TableNow.Api`) with `Endpoints/`, `Extensions/`, and `Mappers/` folders.
- A `Shared` project containing a generic `Result<T>` type (`Data`, `Errors`, `StatusCode`, `IsSuccess`) and a `TypedResultHelper` for mapping results to minimal-API typed results.
- `ServiceCollectionExtensions.RegisterServices()` that calls `Add[Context]Module()` for each context.
- `appsettings.json` with `Jwt` and `ConnectionStrings` sections holding no real secrets.
- `.gitignore` entries that exclude secrets / `.env` / user-secrets artifacts.
- `dotnet build` from `./server` succeeds with zero errors.

## Non-Goals

- No domain entities, EF models, migrations, or seed data (covered by STORY-003 / STORY-004).
- No feature handlers, endpoints, or business logic (covered by later stories).
- No JWT bearer middleware wiring or CORS policy (covered by STORY-007) — only the empty `Jwt`/`ConnectionStrings` configuration sections are added here.
- No frontend work (covered by STORY-002).

## Acceptance Criteria

- [ ] Running `dotnet build` from `./server` compiles the solution with zero errors.
- [ ] The solution contains `Api`, `Application`, `Domain`, `Data`, `Contracts`, `Shared`, `Infrastructure`, and `Migrations` projects following the per-module layout.
- [ ] The `Shared` project contains a `Result<T>` type with `Data`, `Errors`, `StatusCode`, and `IsSuccess` members.
- [ ] `appsettings.json` has `Jwt` and `ConnectionStrings` sections with no committed secrets.
- [ ] `ServiceCollectionExtensions.RegisterServices()` wires up `Add[Context]Module()` calls.
- [ ] `.gitignore` excludes secrets / `.env` files.

## Assumptions

- The .NET 10 SDK is installed on the developer machine and `dotnet` is on the PATH.
- The company/product prefix for all projects is `CM.TableNow` (per CLAUDE.md project naming `CM.TableNow.<BusinessContext>.<Layer>`).
- The business contexts for Phase 1 MVP are `Auth`, `Restaurants`, and `Reservations`. Empty context projects may be created now and populated by later stories.
- The Mediator NuGet package is referenced where handlers will live (Application and Data projects), even though no handlers exist yet.

## Technical Constraints

- Modular monolith with CQRS via Mediator (source-generated). Request flow: HTTP → Minimal API Endpoint → `IMediator.Send(AppRequest)` → AppRequestHandler → `IMediator.Send(DataQuery/Command)` → EF Core DbContext.
- Each business context is a **separate** class library project per layer. A context's Application/Data project must never reference another context's Application/Data project (cross-module calls go through `Contracts.Public` via `IMediator`).
- All handlers return `Result<T>` from the Shared project (`CM.TableNow.Shared` / referenced as `CM.OpenTable.Common` namespace per CLAUDE.md — use the `CM.TableNow.Shared` project with a `Result<T>` type).
- Code style: file-scoped namespaces, nullable enabled, primary constructors for DI, records for DTOs/commands, `CancellationToken` on all async methods.
- No repository pattern, no AutoMapper.
- `OnModelCreating` (later stories) will use `ApplyConfigurationsFromAssembly` — the Data projects must be structured to support this.
