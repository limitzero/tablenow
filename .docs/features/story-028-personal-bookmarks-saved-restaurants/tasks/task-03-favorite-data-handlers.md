# Task 03: Favorites Data Handlers

## Status

pending

## Wave

2

## Description

Implement `AddFavoriteCommand`, `RemoveFavoriteCommand`, and `GetFavoritesQuery` as Data-layer CQRS handlers operating on `AuthDbContext.UserFavorites`.

## Dependencies

**Depends on:** task-01-favorite-entity.md
**Blocks:** task-05-favorites-endpoints.md

## Files to Create

- `server/src/Data/Auth/Commands/AddFavorite/AddFavoriteCommand.cs` — Command + handler.
- `server/src/Data/Auth/Commands/RemoveFavorite/RemoveFavoriteCommand.cs` — Command + handler.
- `server/src/Data/Auth/Queries/GetFavorites/GetFavoritesQuery.cs` — Query + `FavoriteData` + handler.

## Technical Details

### AddFavoriteCommand

```csharp
public sealed record AddFavoriteCommand(Guid UserId, Guid RestaurantId) : ICommand<Result<Unit>>;
```
Handler: check if already exists → return 409 if so; else insert `UserFavorite { UserId, RestaurantId, SavedAt = DateTimeOffset.UtcNow }`.

### RemoveFavoriteCommand

```csharp
public sealed record RemoveFavoriteCommand(Guid UserId, Guid RestaurantId) : ICommand<Result<Unit>>;
```
Handler: find and delete; return 404 if not found.

### GetFavoritesQuery

```csharp
public sealed record FavoriteData(Guid RestaurantId, DateTimeOffset SavedAt);
public sealed record GetFavoritesQuery(Guid UserId) : IQuery<Result<IReadOnlyList<FavoriteData>>>;
```
Handler: `AsNoTracking`, filter by `UserId`, project to `FavoriteData`, order by `SavedAt DESC`.

## Acceptance Criteria

- [ ] `AddFavoriteCommand` inserts a favorite and returns 409 if already exists.
- [ ] `RemoveFavoriteCommand` removes a favorite and returns 404 if not found.
- [ ] `GetFavoritesQuery` returns favorites ordered by `SavedAt DESC`.
