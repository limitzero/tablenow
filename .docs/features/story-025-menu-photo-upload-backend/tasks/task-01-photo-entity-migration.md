# Task 01: Photo Entity & Migration

## Status

pending

## Wave

1

## Description

Add the `Photo` domain entity to the Restaurants context, create its EF model and Fluent API configuration, and generate an EF Core migration adding the `Photos` table. The entity stores `Id`, `RestaurantId`, `Url`, and `UploadedAt`.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-03-photo-data-handlers.md

**Context from dependencies:** STORY-003 created the Restaurants context with `RestaurantsDbContext` and `Restaurant`. This task extends that context.

## Files to Create

- `server/src/Domain/Restaurants/Entities/Photo.cs` — `Photo` domain entity.
- `server/src/Data/Restaurants/Configurations/PhotoConfiguration.cs` — Fluent API config.

## Files to Modify

- `server/src/Data/Restaurants/RestaurantsDbContext.cs` — Add `DbSet<Photo> Photos`.
- Run `dotnet ef migrations add AddPhotos --project server/src/Migrations/...`.

## Technical Details

### Entity Shape

```csharp
public sealed class Photo
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public required string Url { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
}
```

### Fluent Config

- FK: `RestaurantId → Restaurants.Id`, cascade delete.
- `Url` max length 2048.
- Index on `RestaurantId` for efficient lookup per restaurant.

## Acceptance Criteria

- [ ] `Photo` entity exists with `Id`, `RestaurantId`, `Url`, `UploadedAt`.
- [ ] `RestaurantsDbContext.Photos` DbSet is configured.
- [ ] Migration adds `Photos` table with correct FK.
- [ ] `dotnet ef database update` applies without errors.
