# Requirements: Project Scaffolding — Backend

## Summary

TableNow's backend is a .NET 10 modular monolith following CQRS via the source-generated Mediator library. Before any feature work can begin the solution structure must exist with proper project isolation per business context (Auth, Restaurants, Reservations), cross-project references enforced by architecture rules, and all shared infrastructure types in place.

This story produces a compilable skeleton. Subsequent stories add the implementation content. The key deliverables are the `Result<T>` return type used by all handlers, the `TypedResultHelper` that maps results to HTTP responses at Minimal API endpoints, and the module self-registration pattern that keeps the Api project thin.

The solution must compile with zero errors from `dotnet build` at `./server`, and secrets must never be committed to source control.

## Goals

- Create .NET 10 solution with all 17 projects and correct cross-project references
- Implement `Result<T>` with `Data`, `Errors`, `StatusCode`, `IsSuccess` in Shared
- Implement `TypedResultHelper` in Api mapping Result status codes to IResult
- Create module self-registration pattern (`AddAuthModule()`, `AddRestaurantsModule()`, `AddReservationsModule()`)
- Configure `appsettings.json` with `Jwt` and `ConnectionStrings` sections (placeholder values, no secrets)
- Add `.gitignore` covering `bin/`, `obj/`, `appsettings.*.json` secrets, `*.user`

## Non-Goals

- No implementation logic in any project — project structure and cross-cutting types only
- No EF Core migrations (STORY-003)
- No real JWT configuration values (STORY-006)
- No seeded data (STORY-004)
- No HTTP endpoints (STORY-005+)

## Acceptance Criteria

- [ ] `dotnet build server/tablenow.sln` compiles with zero errors
- [ ] All 17 projects exist with correct SDK types and cross-project references
- [ ] `Result<T>` in Shared has `Data`, `Errors`, `StatusCode`, `IsSuccess` with static factory methods
- [ ] `TypedResultHelper` in Api maps 200/201/400/401/403/404/409 correctly
- [ ] `appsettings.json` has `Jwt` and `ConnectionStrings` sections with no committed secrets
- [ ] `.gitignore` prevents committing `bin/`, `obj/`, and secret config files

## Assumptions

- .NET 10 SDK is installed (`dotnet --version` reports 10.x)
- The `server/` directory does not yet exist in the repository
- No deployment infrastructure needed for this story

## Technical Constraints

- Each business context is a separate class library project — no monolithic Application or Data project
- Application projects reference Domain and Shared only (never Data of another module)
- Data projects reference Domain and Shared only (never Application)
- Api references all Application, Data, Infrastructure, and Contracts projects
- No module may reference another module's Application or Data project directly — cross-module calls go through IMediator + Contracts
- Use file-scoped namespaces, C# primary constructors, `nullable enable`, `net10.0` throughout
