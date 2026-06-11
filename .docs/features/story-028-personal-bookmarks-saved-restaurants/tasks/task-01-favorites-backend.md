# Task 01: Favorites Backend

## Status

pending

## Wave

1

## Description

Creates the `UserFavorite` entity, EF configuration, migration, `ToggleFavoriteCommand`, and three endpoints: `POST /api/favorites/{restaurantId}`, `DELETE /api/favorites/{restaurantId}`, `GET /api/favorites`. All require auth.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-003 AppDbContext, STORY-007 auth middleware)
**Blocks:** task-02-favorites-frontend.md

## Files to Create

- `server/src/Domain/Restaurants/UserFavorite.cs`
- `server/src/Data/Restaurants/Configurations/UserFavoriteConfiguration.cs`
- `server/src/Data/Restaurants/Commands/ToggleFavoriteCommand.cs` (+ Handler)
- `server/src/Data/Restaurants/Queries/GetFavoritesQuery.cs` (+ Handler)
- `server/src/Api/Endpoints/FavoritesEndpoints.cs`

## Files to Modify

- `server/src/Data/Reservations/AppDbContext.cs` — add `DbSet<UserFavorite>`
- `server/src/Api/Program.cs` — map favorites group

## Technical Details

### Code Snippets

```csharp
// Domain/Restaurants/UserFavorite.cs
public class UserFavorite
{
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public DateTimeOffset SavedAt { get; set; }
}
```

```csharp
// ToggleFavoriteCommandHandler.cs
// If UserFavorite(UserId, RestaurantId) exists → remove it; else → add it
// Return Result<bool> where Data = true (now saved) or false (now unsaved)
```

```csharp
// FavoritesEndpoints.cs
group.MapPost("/{restaurantId:guid}", ...).RequireAuthorization(); // toggle on
group.MapDelete("/{restaurantId:guid}", ...).RequireAuthorization(); // toggle off
group.MapGet("/", ...).RequireAuthorization(); // list favorites
```

After creating files: `dotnet ef migrations add AddUserFavorites --project server/src/Migrations --startup-project server/src/Migrations`

## Acceptance Criteria

- [ ] `POST` and `DELETE` are idempotent (calling twice has same effect as once)
- [ ] `GET /api/favorites` returns user's saved restaurants
- [ ] All endpoints require auth
- [ ] UserFavorite has composite PK (UserId, RestaurantId)
