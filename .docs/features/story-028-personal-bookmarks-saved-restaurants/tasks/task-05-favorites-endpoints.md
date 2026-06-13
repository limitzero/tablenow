# Task 05: Favorites Endpoints

## Status

pending

## Wave

3

## Description

Add Application-layer handlers for add/remove/get favorites and wire up the `POST /api/favorites/{restaurantId}`, `DELETE /api/favorites/{restaurantId}`, and `GET /api/favorites` Minimal API endpoints. All require JWT auth.

## Dependencies

**Depends on:** task-03-favorite-data-handlers.md
**Blocks:** None

## Files to Create

- `server/src/Application/Auth/Features/AddFavorite/AddFavoriteRequest.cs` + handler.
- `server/src/Application/Auth/Features/RemoveFavorite/RemoveFavoriteRequest.cs` + handler.
- `server/src/Application/Auth/Features/GetFavorites/GetFavoritesRequest.cs` + handler.
- `server/src/Api/Endpoints/Favorites/FavoritesEndpoints.cs` — `MapFavoritesEndpoints(RouteGroupBuilder)`.

## Technical Details

```csharp
// All three endpoints require authorization
var group = app.MapGroup("/api/favorites").RequireAuthorization();

group.MapPost("/{restaurantId:guid}", async (Guid restaurantId, HttpContext ctx, IMediator mediator, CancellationToken ct) => {
    var userId = Guid.Parse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    return TypedResultHelper.ToResult(await mediator.Send(new AddFavoriteRequest(userId, restaurantId), ct));
});

group.MapDelete("/{restaurantId:guid}", async (Guid restaurantId, HttpContext ctx, IMediator mediator, CancellationToken ct) => {
    var userId = Guid.Parse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    return TypedResultHelper.ToResult(await mediator.Send(new RemoveFavoriteRequest(userId, restaurantId), ct));
});

group.MapGet("/", async (HttpContext ctx, IMediator mediator, CancellationToken ct) => {
    var userId = Guid.Parse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    return TypedResultHelper.ToResult(await mediator.Send(new GetFavoritesRequest(userId), ct));
});
```

## Acceptance Criteria

- [ ] `POST /api/favorites/{id}` with JWT saves the restaurant to favorites.
- [ ] `DELETE /api/favorites/{id}` with JWT removes the favorite.
- [ ] `GET /api/favorites` returns the user's favorites.
- [ ] All endpoints return 401 without a JWT.
