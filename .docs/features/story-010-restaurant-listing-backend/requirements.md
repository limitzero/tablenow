# Requirements: Restaurant Listing — Backend

## Summary

The TableNow frontend listing and detail pages need a backend source of restaurant data. This feature provides the read API for the restaurant catalog seeded in STORY-004.

Following the modular-monolith CQRS pattern, the request flow is: HTTP endpoint → `IMediator.Send(AppRequest)` → application request handler → `IMediator.Send(DataQuery)` → EF Core `DbContext`. Two reads are exposed: a full list (`GET /api/restaurants`) and a single detail (`GET /api/restaurants/{id}`). Both are public — no authentication is required to browse restaurants. Each restaurant includes `id`, `name`, `cuisine`, `address`, `description`, and `thumbnailUrl`.

The expected outcome is two reliable, fast, anonymous endpoints that return the seeded restaurants in the shape the Angular listing (STORY-012) and detail (STORY-013) pages expect, all wired through the established `Result<T>` and static-mapper conventions.

## Goals

- Provide `GetRestaurantsQuery` / `GetRestaurantsQueryHandler` and `GetRestaurantByIdQuery` / `GetRestaurantByIdQueryHandler` in the Data layer, returning `Result<T>`.
- Provide `GetRestaurantsRequest` / `GetRestaurantsRequestHandler` and `GetRestaurantByIdRequest` / `GetRestaurantByIdRequestHandler` in the Application layer that map data results to API responses.
- Expose `GET /api/restaurants` (200 with the full list) and `GET /api/restaurants/{id}` (200 with detail, 404 when not found), with no auth required.
- Provide a `RestaurantMapper` static class for API model ↔ application model translation.

## Non-Goals

- Slot availability querying (delivered in STORY-011).
- Cuisine filtering on the server (the frontend filters client-side per STORY-012).
- Pagination, sorting, or search.
- Any write operations (create/update/delete restaurants).
- Authentication or authorization on these endpoints.

## Acceptance Criteria

- [ ] `GET /api/restaurants` called without auth returns `200` with all seeded restaurants.
- [ ] Each restaurant in the response includes `id`, `name`, `cuisine`, `address`, `description`, and `thumbnailUrl`.
- [ ] `GET /api/restaurants/{id}` with a valid id returns a single restaurant detail object (`200`); an unknown id returns `404`.
- [ ] An integration test confirms the list endpoint returns the expected number of seeded restaurants.
- [ ] All handlers return `Result<T>`; endpoints translate it with `TypedResultHelper`.

## Assumptions

- STORY-001 backend scaffolding exists: the `Restaurants` module (Api/Application/Data/Domain/Contracts) projects, `Mediator` source generator, `Result<T>` in `CM.OpenTable.Common`, and `ServiceCollectionExtensions.RegisterServices()` calling `AddRestaurantsModule()`.
- STORY-003/004 are complete: the `Restaurant` EF model and `DbContext` exist and the database is seeded with ≥ 15 restaurants.
- The `Restaurant` entity/model exposes at least `Id`, `Name`, `Cuisine`, `Address`, `Description`, and `ThumbnailUrl`.

## Technical Constraints

- CQRS via `Mediator` (source-generated); no repository pattern — use `DbContext` directly in Data handlers.
- No AutoMapper — use the static `RestaurantMapper`.
- Reads should be projected/`AsNoTracking` for efficiency; do not return EF entities directly from the API.
- Primary constructors for DI, records for DTOs/commands/queries, file-scoped namespaces, nullable enabled, `CancellationToken` on all async methods.
- Endpoints use `TypedResultHelper`; never throw for business outcomes (a missing restaurant is a `Result` with a 404 status, not an exception).
- Cross-module rules: this is self-contained within the `Restaurants` module; do not reference other modules' Application or Data projects.
