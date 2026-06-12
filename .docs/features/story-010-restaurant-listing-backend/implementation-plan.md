# Implementation Plan: Restaurant Listing — Backend

Two phases following the CQRS request flow: Data queries + Application handlers (Phase 1, parallel against a shared contract), then the Minimal API endpoints (Phase 2).

## Phase 1 — Data queries + Application handlers (parallel)

- [ ] **task-01-restaurant-data-queries** — `Data/Restaurants/Queries/`
  - `GetRestaurantsQuery` (record, no params) / `GetRestaurantsQueryHandler` → `Result<IReadOnlyList<RestaurantData>>`.
  - `GetRestaurantByIdQuery(Guid Id)` / `GetRestaurantByIdQueryHandler` → `Result<RestaurantData>` (404 status when not found).
  - Use `DbContext` directly, `AsNoTracking`, project to a `RestaurantData` record (Id, Name, Cuisine, Address, Description, ThumbnailUrl).

- [ ] **task-02-restaurant-application** — `Application/Restaurants/Features/GetRestaurants/` and `.../GetRestaurantById/`
  - `GetRestaurantsRequest` / `GetRestaurantsRequestHandler` → sends `GetRestaurantsQuery`, maps `RestaurantData` → `GetRestaurantsResponse` item.
  - `GetRestaurantByIdRequest(Guid Id)` / `GetRestaurantByIdRequestHandler` → sends `GetRestaurantByIdQuery`, maps to `GetRestaurantByIdResponse`; propagates 404.
  - Returns `Result<T>`; depends on the `RestaurantData` contract from task-01.

## Phase 2 — Endpoints

- [ ] **task-03-restaurant-listing-endpoint** — `Api/Endpoints/Restaurants/`
  - `static class RestaurantEndpoints` with `MapRestaurantEndpoints(RouteGroupBuilder)`.
  - `GET /api/restaurants` → send `GetRestaurantsRequest`, `TypedResultHelper`, 200.
  - `GET /api/restaurants/{id:guid}` → send `GetRestaurantByIdRequest`, 200/404.
  - `RestaurantMapper` static class (application response ↔ API model). No `RequireAuthorization()`.
  - Register the endpoints group in the API's endpoint wiring.

## Verification

- `dotnet build` — zero errors.
- `dotnet test` — `describe_get_restaurants` BDD tests pass; integration test asserts the seeded count.
- Manual: `GET /api/restaurants` returns 200 + list; `GET /api/restaurants/{validId}` returns 200; unknown id returns 404; no auth needed.
