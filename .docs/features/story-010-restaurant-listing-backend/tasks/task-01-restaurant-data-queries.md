# Task 01: Restaurant Data Queries

## Status

pending

## Wave

1

## Description

Implement the Data-layer CQRS queries that read the seeded restaurant catalog from EF Core. `GetRestaurantsQuery` returns every restaurant; `GetRestaurantByIdQuery` returns one by id (or a 404 result when absent). Both query handlers use the `DbContext` directly (no repository), project to a lightweight `RestaurantData` record with `AsNoTracking`, and return `Result<T>`. These queries are the data source the Application layer (task-02) dispatches into.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-02-restaurant-application.md

**Context from dependencies:** This is a Wave 1 task. It assumes the STORY-001 `Restaurants` module projects exist, the `Mediator` source generator is configured, and `Result<T>` lives in `CM.OpenTable.Common`. It assumes STORY-003/004 created the `Restaurant` EF model (with `Id`, `Name`, `Cuisine`, `Address`, `Description`, `ThumbnailUrl`) and the `Restaurants` `DbContext`, and seeded ≥ 15 restaurants. The `RestaurantData` record defined here is the contract consumed by task-02.

## Files to Create

- `server/src/Data/Restaurants/Queries/GetRestaurants/GetRestaurantsQuery.cs` — Query + `RestaurantData` record.
- `server/src/Data/Restaurants/Queries/GetRestaurants/GetRestaurantsQueryHandler.cs` — Handler.
- `server/src/Data/Restaurants/Queries/GetRestaurantById/GetRestaurantByIdQuery.cs` — Query.
- `server/src/Data/Restaurants/Queries/GetRestaurantById/GetRestaurantByIdQueryHandler.cs` — Handler.
- `server/tests/UnitTests/Restaurants/describe_get_restaurants_data/...` — BDD data-layer tests (optional but recommended).

## Files to Modify

- None (the `Restaurants` `DbContext` and EF model already exist from STORY-003).

## Technical Details

### Implementation Steps

1. Define `RestaurantData` as a `record` carrying `Id`, `Name`, `Cuisine`, `Address`, `Description`, `ThumbnailUrl`. Place it alongside the list query so both layers can reference it via the Data project.
2. Define `GetRestaurantsQuery` as a parameterless `record` implementing the `Mediator` query interface returning `Result<IReadOnlyList<RestaurantData>>`.
3. Implement `GetRestaurantsQueryHandler` using a primary constructor for `DbContext` DI. Query with `AsNoTracking`, project directly to `RestaurantData` (server-side projection — do not materialize entities then map), `ToListAsync(cancellationToken)`, and return `Result.Success(list)`.
4. Define `GetRestaurantByIdQuery(Guid Id)` returning `Result<RestaurantData>`.
5. Implement `GetRestaurantByIdQueryHandler`: project + `FirstOrDefaultAsync` on `Id`. When found, `Result.Success`; when null, return a failure `Result` with a 404 status code and a descriptive error (do not throw).
6. Add `CancellationToken` to every async method; file-scoped namespaces; nullable enabled.

### Code Snippets

```csharp
namespace CM.OpenTable.Restaurants.Data.Queries.GetRestaurants;

public sealed record RestaurantData(
    Guid Id,
    string Name,
    string Cuisine,
    string Address,
    string Description,
    string ThumbnailUrl);

public sealed record GetRestaurantsQuery
    : IQuery<Result<IReadOnlyList<RestaurantData>>>;

public sealed class GetRestaurantsQueryHandler(RestaurantsDbContext db)
    : IQueryHandler<GetRestaurantsQuery, Result<IReadOnlyList<RestaurantData>>>
{
    public async ValueTask<Result<IReadOnlyList<RestaurantData>>> Handle(
        GetRestaurantsQuery query,
        CancellationToken cancellationToken)
    {
        var restaurants = await db.Restaurants
            .AsNoTracking()
            .Select(r => new RestaurantData(
                r.Id, r.Name, r.Cuisine, r.Address, r.Description, r.ThumbnailUrl))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<RestaurantData>>(restaurants);
    }
}
```

```csharp
namespace CM.OpenTable.Restaurants.Data.Queries.GetRestaurantById;

public sealed record GetRestaurantByIdQuery(Guid Id)
    : IQuery<Result<RestaurantData>>;

public sealed class GetRestaurantByIdQueryHandler(RestaurantsDbContext db)
    : IQueryHandler<GetRestaurantByIdQuery, Result<RestaurantData>>
{
    public async ValueTask<Result<RestaurantData>> Handle(
        GetRestaurantByIdQuery query,
        CancellationToken cancellationToken)
    {
        var restaurant = await db.Restaurants
            .AsNoTracking()
            .Where(r => r.Id == query.Id)
            .Select(r => new RestaurantData(
                r.Id, r.Name, r.Cuisine, r.Address, r.Description, r.ThumbnailUrl))
            .FirstOrDefaultAsync(cancellationToken);

        return restaurant is null
            ? Result.Failure<RestaurantData>("Restaurant not found.", StatusCodes.Status404NotFound)
            : Result.Success(restaurant);
    }
}
```

> The exact `Mediator` interface names (`IQuery`/`IQueryHandler` vs `IRequest`/`IRequestHandler`) and the `Result` factory signatures must match the conventions established in STORY-001's `Shared` project. Match the existing handlers (e.g. STORY-005/006 auth handlers) rather than these illustrative names if they differ.

## Acceptance Criteria

- [ ] `GetRestaurantsQueryHandler` returns `Result.Success` with all seeded restaurants projected to `RestaurantData`.
- [ ] `GetRestaurantByIdQueryHandler` returns `Result.Success` for a known id and a 404 failure `Result` for an unknown id (no exception thrown).
- [ ] Both handlers use `AsNoTracking` and project server-side (no full-entity materialization).
- [ ] Both handlers accept and propagate a `CancellationToken`.
- [ ] `RestaurantData` exposes `Id`, `Name`, `Cuisine`, `Address`, `Description`, `ThumbnailUrl`.

## Notes

- No repository pattern — `DbContext` is used directly in the handler per CLAUDE.md.
- Keep `RestaurantData` in the Data project so the Application layer can reference it without crossing into another module.
