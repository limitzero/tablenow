# Task 02: Application Handlers & Endpoints

## Status

pending

## Wave

2

## Description

Adds application-layer request/handlers for listing and detail, then wires them to Minimal API endpoints at `GET /api/restaurants` and `GET /api/restaurants/{id}`. Both endpoints are unauthenticated. Registers the Restaurants module.

## Dependencies

**Depends on:** task-01-restaurant-queries.md
**Blocks:** STORY-012 (frontend listing page consumes these endpoints)

**Context from dependencies:** task-01 created `GetRestaurantsQuery` and `GetRestaurantByIdQuery` returning `RestaurantDto` records. `RestaurantDto`: `Id, Name, Cuisine, Address, Description, ThumbnailUrl?`.

## Files to Create

- `server/src/Application/CM.TableNow.Restaurants.Application/Features/GetRestaurants/GetRestaurantsRequest.cs`
- `server/src/Application/CM.TableNow.Restaurants.Application/Features/GetRestaurants/GetRestaurantsRequestHandler.cs`
- `server/src/Application/CM.TableNow.Restaurants.Application/Features/GetRestaurantById/GetRestaurantByIdRequest.cs`
- `server/src/Application/CM.TableNow.Restaurants.Application/Features/GetRestaurantById/GetRestaurantByIdRequestHandler.cs`
- `server/src/Application/CM.TableNow.Restaurants.Application/Extensions/ServiceCollectionExtensions.cs`
- `server/src/Api/Endpoints/RestaurantEndpoints.cs`
- `server/src/Api/Mappers/RestaurantMapper.cs`

## Files to Modify

- `server/src/Api/Extensions/ServiceCollectionExtensions.cs` — Uncomment `services.AddRestaurantsModule()`
- `server/src/Api/Program.cs` — Map restaurant endpoints

## Technical Details

### Code Snippets

```csharp
// GetRestaurantsRequest.cs
public record GetRestaurantsRequest : IRequest<Result<IReadOnlyList<RestaurantSummary>>>;
public record RestaurantSummary(Guid Id, string Name, string Cuisine, string Address, string? ThumbnailUrl);
```

```csharp
// RestaurantEndpoints.cs (key portion)
public static class RestaurantEndpoints
{
    public static RouteGroupBuilder MapRestaurantEndpoints(this RouteGroupBuilder group)
    {
        var restaurants = group.MapGroup("/restaurants");

        restaurants.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetRestaurantsRequest(), ct);
            return TypedResultHelper.ToResult(result);
        }).WithName("GetRestaurants").Produces<IReadOnlyList<RestaurantSummary>>(200);

        restaurants.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetRestaurantByIdRequest(id), ct);
            return TypedResultHelper.ToResult(result);
        }).WithName("GetRestaurantById").Produces<RestaurantDetail>(200).ProducesProblem(404);

        return group;
    }
}
```

Add to `Program.cs`:
```csharp
app.MapGroup("/api")
   .MapAuthEndpoints()
   .MapRestaurantEndpoints();
```

## Acceptance Criteria

- [ ] `GET /api/restaurants` returns 200 with array of restaurants
- [ ] `GET /api/restaurants/{id}` returns 200 with single restaurant or 404
- [ ] No `RequireAuthorization()` on restaurant routes
- [ ] `dotnet build` exits with code 0
