# Task 03: Review Application Layer

## Status

pending

## Wave

2

## Description

Implement `SubmitReviewRequest` handler (validates rating 1–5 and non-empty body, dispatches `CreateReviewCommand`) and `GetReviewsRequest` handler (dispatches `GetReviewsQuery`). Both are Application-layer handlers.

## Dependencies

**Depends on:** task-01-review-entity-migration.md
**Blocks:** task-04-review-endpoints.md

## Files to Create

- `server/src/Application/Restaurants/Features/SubmitReview/SubmitReviewRequest.cs`
- `server/src/Application/Restaurants/Features/SubmitReview/SubmitReviewRequestHandler.cs`
- `server/src/Application/Restaurants/Features/GetReviews/GetReviewsRequest.cs`
- `server/src/Application/Restaurants/Features/GetReviews/GetReviewsRequestHandler.cs`

## Technical Details

### SubmitReviewRequest

```csharp
public sealed record SubmitReviewRequest(Guid RestaurantId, Guid UserId, int Rating, string Body)
    : IRequest<Result<Guid>>;
```
Handler: validate `Rating` is 1–5 → 400; validate `Body` not empty → 400; dispatch `CreateReviewCommand`.

### GetReviewsRequest

```csharp
public sealed record GetReviewsRequest(Guid RestaurantId, int Page = 1, int PageSize = 20)
    : IRequest<Result<IReadOnlyList<ReviewData>>>;
```
Handler: dispatch `GetReviewsQuery`, propagate result.

## Acceptance Criteria

- [ ] `SubmitReviewRequestHandler` returns 400 for rating outside 1–5.
- [ ] `SubmitReviewRequestHandler` returns 400 for empty body.
- [ ] `GetReviewsRequestHandler` returns paginated review data.
