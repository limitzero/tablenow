# Task 02: Review Data Handlers

## Status

pending

## Wave

2

## Description

Implement `CreateReviewCommand` (inserts a review record) and `GetReviewsQuery` (returns paginated reviews for a restaurant, newest-first). Both are CQRS Data-layer handlers operating directly on `RestaurantsDbContext`.

## Dependencies

**Depends on:** task-01-review-entity-migration.md
**Blocks:** task-04-review-endpoints.md

**Context from dependencies:** task-01 created the `Review` entity and `RestaurantsDbContext.Reviews` DbSet.

## Files to Create

- `server/src/Data/Restaurants/Commands/CreateReview/CreateReviewCommand.cs` — Command + handler.
- `server/src/Data/Restaurants/Queries/GetReviews/GetReviewsQuery.cs` — Query + `ReviewData` record + handler.

## Technical Details

### CreateReviewCommand

```csharp
public sealed record CreateReviewCommand(Guid RestaurantId, Guid UserId, int Rating, string Body)
    : ICommand<Result<Guid>>;
```
Handler: insert `Review` entity, return `Result<Guid>.Success(review.Id)`.

### GetReviewsQuery

```csharp
public sealed record ReviewData(Guid Id, string AuthorName, int Rating, string Body, DateTimeOffset CreatedAt);
public sealed record GetReviewsQuery(Guid RestaurantId, int Page, int PageSize)
    : IQuery<Result<IReadOnlyList<ReviewData>>>;
```
Handler: join `Reviews` with `Users` (from `AuthDbContext` or via a cross-context read of `authorName` by `UserId`) to get author name. Order by `CreatedAt DESC`, skip + take for pagination.

**Note:** If cross-DbContext joins are not feasible, store `AuthorName` as a denormalized string on `Review` at creation time (denormalization approach). Document the choice.

## Acceptance Criteria

- [ ] `CreateReviewCommand` inserts a review and returns its ID.
- [ ] `GetReviewsQuery` returns reviews sorted newest-first with correct pagination.
- [ ] Each `ReviewData` includes `authorName`, `rating`, `body`, `createdAt`.
