# Task 02: Review Commands & Queries

## Status

pending

## Wave

2

## Description

Implements `CreateReviewCommand` / `GetReviewsQuery` and their handlers in `Data/Restaurants/`. Paginated review listing, newest first.

## Dependencies

**Depends on:** task-01-review-entity.md
**Blocks:** task-03-review-endpoints.md

**Context from dependencies:** `Review` entity: `Id, RestaurantId, UserId, Rating, Body, CreatedAt`. `RestaurantsDbContext.Reviews` DbSet available.

## Files to Create

- `server/src/Data/CM.TableNow.Restaurants.Data/Commands/CreateReview/CreateReviewCommand.cs`
- `server/src/Data/CM.TableNow.Restaurants.Data/Commands/CreateReview/CreateReviewCommandHandler.cs`
- `server/src/Data/CM.TableNow.Restaurants.Data/Queries/GetReviews/GetReviewsQuery.cs`
- `server/src/Data/CM.TableNow.Restaurants.Data/Queries/GetReviews/GetReviewsQueryHandler.cs`

## Technical Details

### Code Snippets

```csharp
// CreateReviewCommand.cs
public record CreateReviewCommand(Guid RestaurantId, Guid UserId, int Rating, string Body)
    : IRequest<Guid>;
```

```csharp
// GetReviewsQuery.cs
public record GetReviewsQuery(Guid RestaurantId, int Page = 1, int PageSize = 20)
    : IRequest<IReadOnlyList<ReviewDto>>;

public record ReviewDto(Guid ReviewId, Guid UserId, int Rating, string Body, DateTime CreatedAt);
```

```csharp
// GetReviewsQueryHandler.cs
public class GetReviewsQueryHandler(RestaurantsDbContext db)
    : IRequestHandler<GetReviewsQuery, IReadOnlyList<ReviewDto>>
{
    public async ValueTask<IReadOnlyList<ReviewDto>> Handle(GetReviewsQuery query, CancellationToken ct)
        => await db.Reviews
            .AsNoTracking()
            .Where(r => r.RestaurantId == query.RestaurantId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new ReviewDto(r.Id, r.UserId, r.Rating, r.Body, r.CreatedAt))
            .ToListAsync(ct);
}
```

## Acceptance Criteria

- [ ] CreateReview command handler inserts review and returns new Guid
- [ ] GetReviews query handler returns paginated reviews newest-first
- [ ] `dotnet build` exits with code 0
