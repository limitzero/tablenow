# Task 01: UserFavorite Entity & Migration

## Status

pending

## Wave

1

## Description

Add the `UserFavorite` entity to the Auth or Restaurants context (it bridges both — place in the Auth context since it's user-owned data). Create the EF model, Fluent config with composite primary key `(UserId, RestaurantId)`, and generate the migration.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-03-favorite-data-handlers.md

## Files to Create

- `server/src/Domain/Auth/Entities/UserFavorite.cs` — Entity.
- `server/src/Data/Auth/Configurations/UserFavoriteConfiguration.cs` — Fluent config.

## Files to Modify

- `server/src/Data/Auth/AuthDbContext.cs` — Add `DbSet<UserFavorite> UserFavorites`.
- Run `dotnet ef migrations add AddUserFavorites`.

## Technical Details

```csharp
public sealed class UserFavorite
{
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public DateTimeOffset SavedAt { get; set; }
}
```

Fluent config: `HasKey(f => new { f.UserId, f.RestaurantId })` (composite PK, no separate ID column needed).

## Acceptance Criteria

- [ ] `UserFavorite` entity with composite PK `(UserId, RestaurantId)`.
- [ ] Migration adds `UserFavorites` table.
- [ ] `dotnet ef database update` applies without errors.
