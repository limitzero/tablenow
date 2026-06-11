# Task 02: Restaurants Endpoints & Tests

## Status

pending

## Wave

2

## Description

Registers `GET /api/restaurants` and `GET /api/restaurants/{id}` in `RestaurantsEndpoints.cs` and writes one BDD unit test. Both endpoints are public (no `.RequireAuthorization()`).

## Dependencies

**Depends on:** task-01-get-restaurants-handler.md
**Blocks:** STORY-012 (frontend calls these endpoints), STORY-011 task-02 (slots endpoint added to same class)

**Context from dependencies:** task-01 created `GetRestaurantsRequest`, `GetRestaurantByIdRequest`, and their handlers. `TypedResultHelper` from STORY-001, `AuthEndpoints.cs` pattern from STORY-005. The `GET /{id}/slots` route will be added to this same class in STORY-011 task-02.

## Files to Create

- `server/src/Api/Endpoints/RestaurantsEndpoints.cs`
- `server/tests/UnitTests/Restaurants/describe_get_restaurants/when_restaurants_exist.cs`

## Files to Modify

- `server/src/Api/Program.cs` — map restaurants group

## Technical Details

### Code Snippets

```csharp
// server/src/Api/Endpoints/RestaurantsEndpoints.cs
namespace TableNow.Api.Endpoints;

public static class RestaurantsEndpoints
{
    public static RouteGroupBuilder MapRestaurantsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetRestaurantsRequest(), ct);
            return TypedResultHelper.ToResult(result);
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetRestaurantByIdRequest(id), ct);
            return TypedResultHelper.ToResult(result);
        });

        // GET /{id}/slots added in STORY-011 task-02
        return group;
    }
}
```

```csharp
// describe_get_restaurants/when_restaurants_exist.cs
namespace describe_get_restaurants;
public class when_restaurants_exist
{
    [Fact]
    public async Task it_should_return_all_restaurants()
    {
        var expected = new List<RestaurantDto>
        {
            new(Guid.NewGuid(), "Test Restaurant", "Italian", "123 Main St", "Desc", "url"),
        };
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetRestaurantsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<RestaurantDto>>.Success(expected));

        var handler = new GetRestaurantsRequestHandler(mediator.Object);
        var result = await handler.Handle(new GetRestaurantsRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }
}
```

## Acceptance Criteria

- [ ] `GET /api/restaurants` endpoint mapped with no auth requirement
- [ ] `GET /api/restaurants/{id}` endpoint mapped with no auth requirement
- [ ] Both use `TypedResultHelper.ToResult` to map response
- [ ] BDD test `it_should_return_all_restaurants` passes
