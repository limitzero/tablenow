# Task 01: Favorites Backend (Entity + Endpoints)

## Status

pending

## Wave

1

## Description

Creates `UserFavorite` entity, EF config, migration, data commands/queries, and `POST/DELETE /api/favorites/{restaurantId}` endpoints. Auth required. Returns the user's saved state for a restaurant.

## Dependencies

**Depends on:** STORY-003 task-03-migrations.md, STORY-007 task-02-cors-authorization.md
**Blocks:** task-02-favorites-store.md

**Context from dependencies:** `UserFavorite`: `UserId (Guid)`, `RestaurantId (Guid)`, `SavedAt (DateTime)`. Composite primary key (UserId + RestaurantId).

## Files to Create

- `server/src/Domain/CM.TableNow.Auth.Domain/UserFavorite.cs`
- `server/src/Data/CM.TableNow.Auth.Data/Configurations/UserFavoriteConfiguration.cs`
- `server/src/Data/CM.TableNow.Auth.Data/Commands/AddFavorite/AddFavoriteCommand.cs` + Handler
- `server/src/Data/CM.TableNow.Auth.Data/Commands/RemoveFavorite/RemoveFavoriteCommand.cs` + Handler
- `server/src/Data/CM.TableNow.Auth.Data/Queries/GetFavorites/GetFavoritesQuery.cs` + Handler
- `server/src/Api/Endpoints/FavoritesEndpoints.cs`

## Files to Modify

- `server/src/Data/CM.TableNow.Auth.Data/AuthDbContext.cs` — Add `DbSet<UserFavorite>`
- `server/src/Api/Program.cs` — Map favorites endpoints

## Technical Details

```csharp
// UserFavorite.cs
public class UserFavorite
{
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public DateTime SavedAt { get; set; }
}

// UserFavoriteConfiguration.cs
builder.HasKey(f => new { f.UserId, f.RestaurantId }); // composite key
builder.HasIndex(f => f.UserId);
```

```csharp
// FavoritesEndpoints.cs
var favs = group.MapGroup("/favorites").RequireAuthorization();

favs.MapPost("/{restaurantId:guid}", async (Guid restaurantId, ClaimsPrincipal user, ...) =>
{
    var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    await mediator.Send(new AddFavoriteCommand(userId, restaurantId), ct);
    return Results.Created();
});

favs.MapDelete("/{restaurantId:guid}", async (Guid restaurantId, ClaimsPrincipal user, ...) =>
{
    var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    await mediator.Send(new RemoveFavoriteCommand(userId, restaurantId), ct);
    return Results.Ok();
});

favs.MapGet("/", async (ClaimsPrincipal user, ...) =>
{
    var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await mediator.Send(new GetFavoritesQuery(userId), ct);
    return TypedResultHelper.ToResult(result);
});
```

Generate migration:
```powershell
dotnet ef migrations add AddUserFavorites --project src/Migrations/CM.TableNow.Auth.Migrations --startup-project src/Api --context AuthDbContext
```

## Acceptance Criteria

- [ ] `POST /api/favorites/{restaurantId}` saves favorite; auth required
- [ ] `DELETE /api/favorites/{restaurantId}` removes; auth required
- [ ] `GET /api/favorites` returns list of saved restaurantIds
- [ ] `dotnet build` exits with code 0
