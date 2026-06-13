# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**TableNow** — a dual-sided restaurant reservation web app (OpenTable-like). Angular 21 frontend + .NET 10 backend. See `.docs/prd/PRD.md` for full product context and phased roadmap.

---

## Backend (.NET 10)

Root directory: `./server`

### Commands

```powershell
dotnet build
dotnet test
dotnet test --filter "FullyQualifiedName~describe_[feature]"   # run a single describe block
dotnet ef migrations add <Name> --project server/src/Migrations/...
dotnet ef database update --project server/src/Migrations/...
```

### Architecture

Modular Monolith with CQRS via [Mediator](https://github.com/martinothamar/Mediator) (source-generated). The request flow is:

```
HTTP → Minimal API Endpoint
     → IMediator.Send(AppRequest)         [Application layer]
     → AppRequestHandler
     → IMediator.Send(DataQuery/Command)  [Data layer]
     → EF Core DbContext → SQL Server
```

Business contexts are **separate class library projects**, named `Company.Product.BusinessContext.Layer`:

```
server/
  src/Api/                                     ← entry point, endpoints, middleware
  src/Application/<Context>/Features/<UseCase> ← request/handler/response per use case
  src/Domain/<Context>/                        ← entities, value objects (no EF annotations)
  src/Data/<Context>/Commands|Queries/         ← EF DbContext operations
  src/Contracts/                               ← API-facing request/response models
  src/Infrastructure/<Context>/                ← external services, configuration
  src/Shared/                                  ← cross-cutting types (Result<T>, etc.)
  src/Migrations/                              ← EF migrations per context
  tests/UnitTests/                             ← Application + Domain layer tests
  tests/IntegrationTests/                      ← API + database tests
```

### Key Patterns

- **Result\<T\>** — all handlers return `Result<T>` from `CM.OpenTable.Common` (`Data`, `Errors`, `StatusCode`, `IsSuccess`). Use `TypedResultHelper` at endpoints. Never throw for business logic failures.
- **Static mappers** — each Endpoints project has a `[Context]Mapper` static class for API model ↔ Application model translation. No AutoMapper.
- **EF model** — one model per entity in `Data/Models/`; Fluent API in `Configurations/`; `OnModelCreating` uses `ApplyConfigurationsFromAssembly`.
- **Cross-module communication** — dispatch via `IMediator` referencing only the target module's `Contracts.Public` assembly. Never reference another module's Application or Data projects.
- **Module registration** — each module self-registers via `Add[Context]Module()` called from `ServiceCollectionExtensions.RegisterServices()`.

### Naming Conventions

| Layer | Pattern |
|---|---|
| Application request | `[UseCase]Request` / `[UseCase]Response` / `[UseCase]RequestHandler` |
| Data command | `Create[Entity]Command` / `Create[Entity]CommandHandler` |
| Data query | `Get[Entity]Query` / `Get[Entity]QueryHandler` |
| Endpoint class | `static class [Context]Endpoints` with `Map[Context]Endpoints(RouteGroupBuilder)` |

**Code style:** primary constructors for DI, records for DTOs/commands, file-scoped namespaces, `CancellationToken` on all async methods, nullable enabled.

### Test Naming (BDD)

```csharp
namespace describe_create_reservation   // lowercase, no Handler/Request/etc. suffix
{
    public class when_slot_is_fully_booked
    {
        [Fact]
        public async Task it_should_return_conflict_status() { ... }
    }
}
```

Base classes: `module_fixture` (mocked `IMediator`), `api_fixture` (`WebApplicationFactory<Program>`), `data_handler_fixture` (DbSet mock), `data_context_fixture` (EF in-memory).

### What to Avoid

- No repository pattern — use `DbContext` directly in Data handlers.
- No AutoMapper in module code.
- Do not reference one module's Application or Data project from another module.
- Do not split EF entity into Persistence + Domain models.

---

## Frontend (Angular 21)

Root directory: `./client`

### Commands

```bash
npm run test      # Vitest unit tests
npm run e2e       # Playwright end-to-end tests
npm run lint      # ESLint
npm run build     # Production build
```

### Architecture

Standalone components only (no NgModules). Feature-based folder structure:

```
client/
  src/app/
    core/                   # auth service, HTTP interceptors, guards
    shared/                 # reusable components, pipes, directives
    features/
      <feature-name>/
        components/
        services/
        store/              # NgRx Signal Store slice
        models/
        routes/
        index.ts            # barrel export (required for every feature folder)
```

### Key Conventions

- **State:** NgRx Signal Store — one store slice per feature.
- **Change detection:** `OnPush` on every component, no exceptions.
- **API calls:** through services only, never directly in components.
- **Template control flow:** use `@if` / `@for` — never `*ngIf` / `*ngFor`.
- **DI:** inject() function — no constructor injection.
- **Async:** do not call `.subscribe()` in components; use `httpResource()` or async pipe.
- **UI:** Angular Material with a custom theme.

---

## Design Documents

Authoritative references for architecture decisions:

| Document | Content |
|---|---|
| `.docs/design/backend/backend-project-structure.md` | Folder layout + business context rules |
| `.docs/design/backend/backend-tech-stack.md` | Full dependency list |
| `.docs/design/backend/backend-patterns.md` | CQRS, Result\<T\>, static mappers, EF conventions |
| `.docs/design/backend/backend-naming-standards.md` | Naming rules + BDD test conventions |
| `.docs/design/frontend/angular-standards.md` | Angular project conventions |
| `.docs/prd/PRD.md` | Product requirements + 4-phase roadmap |
| `.docs/adr/index.md` | Architecture Decision Records — binding decisions on patterns and trade-offs |
