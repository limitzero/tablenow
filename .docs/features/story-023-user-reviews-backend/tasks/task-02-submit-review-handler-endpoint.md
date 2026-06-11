# Task 02: Submit Review Handler & Endpoint

## Status

pending

## Wave

2

## Description

Creates the Application + Data layers for submitting a review and registers `POST /api/restaurants/{id}/reviews`. Requires authentication (userId from JWT). Validates rating 1–5.

## Dependencies

**Depends on:** task-01-review-entity-migration.md
**Blocks:** STORY-024 (frontend review form calls this)

**Context from dependencies:** task-01 created `Review` entity and `DbSet<Review>`. This task creates the CQRS stack and endpoint. Parallel with task-03 — different handler and endpoint files.

## Files to Create

- `server/src/Application/Restaurants/Features/SubmitReview/SubmitReviewRequest.cs` (+ Response + Handler + Validator)
- `server/src/Data/Restaurants/Commands/CreateReviewCommand.cs` (+ Handler)
- `server/src/Contracts/Restaurants/ReviewDto.cs`

## Files to Modify

- `server/src/Api/Endpoints/RestaurantsEndpoints.cs` — add POST /{id}/reviews

## Technical Details

### Code Snippets

```csharp
// SubmitReviewRequest.cs
public record SubmitReviewRequest(Guid RestaurantId, int Rating, string Body)
    : IRequest<Result<ReviewDto>>;

// Validator
public class SubmitReviewRequestValidator : AbstractValidator<SubmitReviewRequest>
{
    public SubmitReviewRequestValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
    }
}
```

```csharp
// ReviewDto.cs (Contracts/Restaurants/)
public record ReviewDto(Guid ReviewId, string AuthorName, int Rating, string Body, DateTimeOffset CreatedAt);
```

```csharp
// POST /{id}/reviews in RestaurantsEndpoints:
group.MapPost("/{id:guid}/reviews", async (
    Guid id,
    [FromBody] SubmitReviewBody body,
    HttpContext httpCtx,
    IMediator mediator,
    CancellationToken ct) =>
{
    var result = await mediator.Send(
        new SubmitReviewRequest(id, body.Rating, body.Body), ct);
    return TypedResultHelper.ToResult(result);
}).RequireAuthorization();

record SubmitReviewBody(int Rating, string Body);
```

## Acceptance Criteria

- [ ] `POST /api/restaurants/{id}/reviews` returns 201 on valid request
- [ ] Rating outside 1–5 returns 400
- [ ] Unauthenticated request returns 401
- [ ] UserId read from JWT claims in handler
