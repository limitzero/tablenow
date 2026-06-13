# Task 01: Review Entity & Migration

## Status

pending

## Wave

1

## Description

Add the `Review` domain entity to the Restaurants context, create its EF model and Fluent API configuration in `Data/Restaurants/`, and generate an EF Core migration adding the `Reviews` table. This is the data foundation for all review features.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-02-review-data-handlers.md, task-03-review-application-layer.md

**Context from dependencies:** STORY-003 created the Restaurants context with `RestaurantsDbContext` and `Restaurant` entity. This task extends that context with `Review`.

## Files to Create

- `server/src/Domain/Restaurants/Entities/Review.cs` — `Review` domain entity.
- `server/src/Data/Restaurants/Models/ReviewModel.cs` — EF model (or same as domain if not split).
- `server/src/Data/Restaurants/Configurations/ReviewConfiguration.cs` — Fluent API config.

## Files to Modify

- `server/src/Data/Restaurants/RestaurantsDbContext.cs` — Add `DbSet<Review> Reviews`.
- Run `dotnet ef migrations add AddReviews --project server/src/Migrations/...`.

## Technical Details

### Entity Shape

```csharp
public sealed class Review
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }       // 1–5
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
}
```

### Fluent Config

- FK: `RestaurantId → Restaurants.Id`, cascade delete.
- Index on `(RestaurantId, CreatedAt DESC)` for efficient newest-first queries.
- `Rating` constrained 1–5 via check constraint or application-layer validation (application layer handles it).

## Acceptance Criteria

- [ ] `Review` entity exists in `Domain/Restaurants/Entities/`.
- [ ] `RestaurantsDbContext.Reviews` DbSet is configured.
- [ ] EF migration adds `Reviews` table with correct columns and FK.
- [ ] `dotnet ef database update` applies without errors.
