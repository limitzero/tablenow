# Implementation Plan: Project Scaffolding — Backend

## Phase 1 — Foundation (parallel)

These two tasks touch disjoint files and run in parallel.

- [ ] **task-01-solution-structure** — Create `server/TableNow.sln`, the `CM.TableNow.Api` host project, and the per-context layer projects (`CM.TableNow.<Context>.Application/Domain/Data/Infrastructure/Migrations`), the shared `CM.TableNow.Shared` project, the `CM.TableNow.Contracts` project, and the `UnitTests` / `IntegrationTests` projects. Establish the `Endpoints/`, `Extensions/`, `Mappers/` folders in Api and `Features/` in Application. Add Mediator, EF Core, FluentValidation, and Scalar package references where appropriate. Verify `dotnet build` succeeds.
  - Technical detail: project prefix `CM.TableNow`; contexts `Auth`, `Restaurants`, `Reservations`; nullable enabled; file-scoped namespaces; `LangVersion` latest.
- [ ] **task-02-shared-result-type** — In `CM.TableNow.Shared`, implement the generic `Result<T>` type with `Data`, `Errors`, `StatusCode`, `IsSuccess` and factory helpers (`Success`, `Failure`), plus a `TypedResultHelper` static class that converts a `Result<T>` into an ASP.NET Core minimal-API typed result.
  - Technical detail: `StatusCode` maps to HTTP status; `Errors` is a collection of error descriptors; `IsSuccess` derived from status range 200–299.

## Phase 2 — Wiring

Runs after Phase 1 completes. Depends on the projects existing (task-01) and on `Result<T>` being available for reference patterns (task-02).

- [ ] **task-03-module-registration** — Add `Add[Context]Module()` extension methods (one per context, in each context's Infrastructure or Application `Extensions` folder) and a `ServiceCollectionExtensions.RegisterServices()` in the Api project that calls each. Create `appsettings.json` / `appsettings.Development.json` with empty-but-shaped `Jwt` and `ConnectionStrings` sections. Update root `.gitignore` to exclude secrets / `.env` / user-secrets. Wire `RegisterServices()` into `Program.cs`. Verify `dotnet build` still succeeds.
