# Task 01: Get Restaurants Handler

## Status

pending

## Wave

1

## Description

Creates the Application + Data layer for fetching all restaurants and a single restaurant by ID. Both follow the same CQRS pattern: an Application-layer request/handler that sends a Data-layer query via IMediator, which hits EF Core.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-003 AppDbContext, STORY-001 project structure)
**Blocks:** task-02-restaurants-endpoints.md, STORY-012 (frontend listing)

**Context from dependencies:** STORY-003 created `AppDbContext` with `DbSet<Restaurant>`. STORY-001 created `Application.Restaurants` and `Data.Restaurants` projects and `Contracts.csproj`. The `Result<T>` type from STORY-001 task-02 is available. This task creates 6 files across three projects.

## Files to Create

- `server/src/Contracts/Restaurants/RestaurantDto.cs`
- `server/src/Data/Restaurants/Queries/GetRestaurantsQuery.cs`
- `server/src/Data/Restaurants/Queries/GetRestaurantsQueryHandler.cs`
- `server/src/Data/Restaurants/Queries/GetRestaurantByIdQuery.cs`
- `server/src/Data/Restaurants/Queries/GetRestaurantByIdQueryHandler.cs`
- `server/src/Application/Restaurants/Features/GetRestaurants/GetRestaurantsRequest.cs`
- `server/src/Application/Restaurants/Features/GetRestaurants/GetRestaurantsRequestHandler.cs`
- `server/src/Application/Restaurants/Features/GetRestaurantById/GetRestaurantByIdRequest.cs`
- `server/src/Application/Restaurants/Features/GetRestaurantById/GetRestaurantByIdRequestHandler.cs`

## Technical Details

### Code Snippets

```csharp
// Contracts/Restaurants/RestaurantDto.cs
namespace TableNow.Contracts.Restaurants;
public record RestaurantDto(Guid Id, string Name, string Cuisine, string Address, string Description, string ThumbnailUrl);
```

```csharp
// Data/Restaurants/Queries/GetRestaurantsQuery.cs
namespace TableNow.Data.Restaurants.Queries;
public record GetRestaurantsQuery : IRequest<Result<List<RestaurantDto>>>;

// GetRestaurantsQueryHandler.cs
public class GetRestaurantsQueryHandler(AppDbContext db)
    : IRequestHandler<GetRestaurantsQuery, Result<List<RestaurantDto>>>
{
    public async ValueTask<Result<List<RestaurantDto>>> Handle(
        GetRestaurantsQuery query, CancellationToken ct)
    {
        var restaurants = await db.Restaurants
            .Select(r => new RestaurantDto(r.Id, r.Name, r.Cuisine, r.Address, r.Description, r.ThumbnailUrl))
            .ToListAsync(ct);
        return Result<List<RestaurantDto>>.Success(restaurants);
    }
}
```

```csharp
// Data/Restaurants/Queries/GetRestaurantByIdQuery.cs
namespace TableNow.Data.Restaurants.Queries;
public record GetRestaurantByIdQuery(Guid RestaurantId) : IRequest<Result<RestaurantDto>>;

// GetRestaurantByIdQueryHandler.cs
public class GetRestaurantByIdQueryHandler(AppDbContext db)
    : IRequestHandler<GetRestaurantByIdQuery, Result<RestaurantDto>>
{
    public async ValueTask<Result<RestaurantDto>> Handle(
        GetRestaurantByIdQuery query, CancellationToken ct)
    {
        var r = await db.Restaurants.FindAsync([query.RestaurantId], ct);
        return r is null
            ? Result<RestaurantDto>.Failure("Restaurant not found.", 404)
            : Result<RestaurantDto>.Success(
                new RestaurantDto(r.Id, r.Name, r.Cuisine, r.Address, r.Description, r.ThumbnailUrl));
    }
}
```

```csharp
// Application/Restaurants/Features/GetRestaurants/GetRestaurantsRequest.cs
namespace TableNow.Application.Restaurants.Features.GetRestaurants;
public record GetRestaurantsRequest : IRequest<Result<List<RestaurantDto>>>;

// GetRestaurantsRequestHandler.cs — delegates to Data query
public class GetRestaurantsRequestHandler(IMediator mediator)
    : IRequestHandler<GetRestaurantsRequest, Result<List<RestaurantDto>>>
{
    public async ValueTask<Result<List<RestaurantDto>>> Handle(
        GetRestaurantsRequest request, CancellationToken ct)
        => await mediator.Send(new GetRestaurantsQuery(), ct);
}
```

```csharp
// Application/Restaurants/Features/GetRestaurantById — same pattern with GetRestaurantByIdRequest(Guid Id)
```

## Acceptance Criteria

- [ ] `RestaurantDto` exists in `Contracts/Restaurants/`
- [ ] `GetRestaurantsQuery` and handler return all restaurants projected to `RestaurantDto`
- [ ] `GetRestaurantByIdQuery` and handler return single restaurant or 404
- [ ] Application handlers delegate to Data layer via IMediator (no direct EF access in Application layer)
