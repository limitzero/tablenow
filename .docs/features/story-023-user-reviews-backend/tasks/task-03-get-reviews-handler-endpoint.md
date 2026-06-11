# Task 03: Get Reviews Handler & Endpoint

## Status

pending

## Wave

2

## Description

Creates the query + handler for listing reviews and registers `GET /api/restaurants/{id}/reviews`. No auth required. Sorted by `CreatedAt DESC`. Also adds reviews to the `GET /api/restaurants/{id}` detail response.

## Dependencies

**Depends on:** task-01-review-entity-migration.md
**Blocks:** STORY-024 (frontend review list uses this endpoint)

**Context from dependencies:** task-01 created `Review` entity. STORY-010 created `RestaurantsEndpoints` and `GET /api/restaurants/{id}`. Parallel with task-02 — different handler files, and different endpoint additions (reviews GET vs. reviews POST).

## Files to Create

- `server/src/Data/Restaurants/Queries/GetReviewsQuery.cs` (+ Handler)
- `server/src/Application/Restaurants/Features/GetReviews/GetReviewsRequest.cs` (+ Handler)

## Files to Modify

- `server/src/Api/Endpoints/RestaurantsEndpoints.cs` — add GET /{id}/reviews

## Technical Details

### Code Snippets

```csharp
// GetReviewsQuery.cs
public record GetReviewsQuery(Guid RestaurantId) : IRequest<Result<List<ReviewDto>>>;

// Handler: join Review + User, project to ReviewDto, order by CreatedAt DESC
var reviews = await db.Reviews
    .Where(r => r.RestaurantId == query.RestaurantId)
    .OrderByDescending(r => r.CreatedAt)
    .Join(db.Users, r => r.UserId, u => u.Id,
        (r, u) => new ReviewDto(r.Id, u.Name, r.Rating, r.Body, r.CreatedAt))
    .Take(50) // max 50 reviews per page
    .ToListAsync(ct);
```

```csharp
// Add to RestaurantsEndpoints:
group.MapGet("/{id:guid}/reviews", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetReviewsRequest(id), ct);
    return TypedResultHelper.ToResult(result);
});
```

## Acceptance Criteria

- [ ] `GET /api/restaurants/{id}/reviews` returns reviews sorted newest first
- [ ] No auth required on GET
- [ ] Each review includes authorName, rating, body, createdAt
