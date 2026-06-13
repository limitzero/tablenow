# Task 04: Review Endpoints

## Status

pending

## Wave

3

## Description

Add `POST /api/restaurants/{id}/reviews` (requires JWT auth) and `GET /api/restaurants/{id}/reviews?page=&pageSize=` (public) to a `ReviewsEndpoints` static class. The POST endpoint extracts `userId` from the JWT sub claim. A `ReviewsMapper` handles API DTO ↔ Application request mapping.

## Dependencies

**Depends on:** task-02-review-data-handlers.md, task-03-review-application-layer.md
**Blocks:** None

## Files to Create

- `server/src/Api/Endpoints/Restaurants/ReviewsEndpoints.cs` — Static class with `MapReviewEndpoints(RouteGroupBuilder)`.
- `server/src/Api/Endpoints/Restaurants/ReviewsMapper.cs` — Static mapper.
- `server/src/Contracts/Restaurants/SubmitReviewRequest.cs` — API DTO.

## Technical Details

```csharp
// POST — requires auth
group.MapPost("/{restaurantId:guid}/reviews", async (
    Guid restaurantId, SubmitReviewRequest body,
    HttpContext ctx, IMediator mediator, CancellationToken ct) =>
{
    var userId = Guid.Parse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await mediator.Send(ReviewsMapper.ToSubmitRequest(restaurantId, userId, body), ct);
    return TypedResultHelper.ToResult(result);
}).RequireAuthorization();

// GET — public
group.MapGet("/{restaurantId:guid}/reviews", async (
    Guid restaurantId, int page = 1, int pageSize = 20,
    IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetReviewsRequest(restaurantId, page, pageSize), ct);
    return TypedResultHelper.ToResult(result);
});
```

## Acceptance Criteria

- [ ] `POST /api/restaurants/{id}/reviews` with valid body and JWT returns 201.
- [ ] `POST` without JWT returns 401.
- [ ] `GET /api/restaurants/{id}/reviews` returns paginated reviews without auth.
- [ ] Rating validation (400 for out-of-range) is exercised end-to-end.
