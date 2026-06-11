# Task 01: Restaurant Data Queries

## Status

pending

## Wave

1

## Description

Implements `GetRestaurantsQuery` / `GetRestaurantsQueryHandler` and `GetRestaurantByIdQuery` / `GetRestaurantByIdQueryHandler` in the Data/Restaurants layer. Both use `RestaurantsDbContext` directly — no repository.

## Dependencies

**Depends on:** STORY-003 task-02-ef-models-configs.md (RestaurantsDbContext with DbSet<Restaurant> must exist)
**Blocks:** task-02-restaurant-endpoints.md

**Context from dependencies:** STORY-003 created `RestaurantsDbContext` with `DbSet<Restaurant> Restaurants` and `DbSet<TimeSlot> TimeSlots`. Restaurant entity: `Id (Guid)`, `Name`, `Cuisine`, `Address`, `Description`, `ThumbnailUrl?`, `CreatedAt`.

## Files to Create

- `server/src/Data/CM.TableNow.Restaurants.Data/Queries/GetRestaurants/GetRestaurantsQuery.cs`
- `server/src/Data/CM.TableNow.Restaurants.Data/Queries/GetRestaurants/GetRestaurantsQueryHandler.cs`
- `server/src/Data/CM.TableNow.Restaurants.Data/Queries/GetRestaurantById/GetRestaurantByIdQuery.cs`
- `server/src/Data/CM.TableNow.Restaurants.Data/Queries/GetRestaurantById/GetRestaurantByIdQueryHandler.cs`

## Files to Modify

None.

## Technical Details

### Code Snippets

```csharp
// GetRestaurantsQuery.cs
namespace CM.TableNow.Restaurants.Data.Queries.GetRestaurants;

public record GetRestaurantsQuery : IRequest<IReadOnlyList<RestaurantDto>>;

public record RestaurantDto(
    Guid Id, string Name, string Cuisine,
    string Address, string Description, string? ThumbnailUrl);
```

```csharp
// GetRestaurantsQueryHandler.cs
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Restaurants.Data.Queries.GetRestaurants;

public class GetRestaurantsQueryHandler(RestaurantsDbContext db)
    : IRequestHandler<GetRestaurantsQuery, IReadOnlyList<RestaurantDto>>
{
    public async ValueTask<IReadOnlyList<RestaurantDto>> Handle(
        GetRestaurantsQuery query,
        CancellationToken ct)
        => await db.Restaurants
            .AsNoTracking()
            .Select(r => new RestaurantDto(r.Id, r.Name, r.Cuisine, r.Address, r.Description, r.ThumbnailUrl))
            .ToListAsync(ct);
}
```

```csharp
// GetRestaurantByIdQuery.cs
namespace CM.TableNow.Restaurants.Data.Queries.GetRestaurantById;

public record GetRestaurantByIdQuery(Guid Id) : IRequest<RestaurantDto?>;
```

```csharp
// GetRestaurantByIdQueryHandler.cs
using CM.TableNow.Restaurants.Data.Queries.GetRestaurants;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Restaurants.Data.Queries.GetRestaurantById;

public class GetRestaurantByIdQueryHandler(RestaurantsDbContext db)
    : IRequestHandler<GetRestaurantByIdQuery, RestaurantDto?>
{
    public async ValueTask<RestaurantDto?> Handle(
        GetRestaurantByIdQuery query,
        CancellationToken ct)
        => await db.Restaurants
            .AsNoTracking()
            .Where(r => r.Id == query.Id)
            .Select(r => new RestaurantDto(r.Id, r.Name, r.Cuisine, r.Address, r.Description, r.ThumbnailUrl))
            .FirstOrDefaultAsync(ct);
}
```

## Acceptance Criteria

- [ ] Both queries and handlers exist at specified paths
- [ ] `GetRestaurantsQueryHandler` returns all restaurants using `AsNoTracking()`
- [ ] `GetRestaurantByIdQueryHandler` returns `null` when id not found
- [ ] Projection to `RestaurantDto` done in LINQ (no entity materialization)
- [ ] `dotnet build` exits with code 0
