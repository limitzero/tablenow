# Task 03: Review Application Handlers & Endpoints

## Status

pending

## Wave

3

## Description

Creates application-layer handlers for CreateReview and GetReviews, and adds `POST /api/restaurants/{id}/reviews` (auth required) and `GET /api/restaurants/{id}/reviews` (public) to `RestaurantEndpoints`.

## Dependencies

**Depends on:** task-02-review-commands.md
**Blocks:** STORY-024

**Context from dependencies:** task-02 created data commands/queries. `CreateReviewCommand(RestaurantId, UserId, Rating, Body)` and `GetReviewsQuery(RestaurantId, Page, PageSize)`.

## Files to Create

- `server/src/Application/CM.TableNow.Restaurants.Application/Features/CreateReview/CreateReviewRequest.cs`
- `server/src/Application/CM.TableNow.Restaurants.Application/Features/CreateReview/CreateReviewRequestHandler.cs`
- `server/src/Application/CM.TableNow.Restaurants.Application/Features/GetReviews/GetReviewsRequest.cs`
- `server/src/Application/CM.TableNow.Restaurants.Application/Features/GetReviews/GetReviewsRequestHandler.cs`

## Files to Modify

- `server/src/Api/Endpoints/RestaurantEndpoints.cs` — Add review routes

## Technical Details

```csharp
// POST /api/restaurants/{id}/reviews (auth required)
restaurants.MapPost("/{id:guid}/reviews", async (
    Guid id, CreateReviewApiRequest body, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
{
    if (body.Rating is < 1 or > 5) return Results.BadRequest("Rating must be between 1 and 5.");
    var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await mediator.Send(new CreateReviewRequest(id, userId, body.Rating, body.Body), ct);
    return TypedResultHelper.ToResult(result);
}).RequireAuthorization();

// GET /api/restaurants/{id}/reviews (public)
restaurants.MapGet("/{id:guid}/reviews", async (
    Guid id, [FromQuery] int page, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetReviewsRequest(id, Math.Max(1, page)), ct);
    return TypedResultHelper.ToResult(result);
});
```

## Acceptance Criteria

- [ ] `POST /api/restaurants/{id}/reviews` requires auth, returns 201
- [ ] Rating outside 1–5 returns 400
- [ ] `GET /api/restaurants/{id}/reviews` is public, returns newest-first
- [ ] `dotnet build` exits with code 0
