# Task 01: Photo Entity & EF Config

## Status

pending

## Wave

1

## Description

Creates the `Photo` domain entity, adds it to `RestaurantsDbContext`, creates Fluent API configuration, and generates an EF migration for the `Photos` table.

## Dependencies

**Depends on:** STORY-003 task-03-migrations.md
**Blocks:** task-03-photo-endpoint.md

## Files to Create

- `server/src/Domain/CM.TableNow.Restaurants.Domain/Photo.cs`
- `server/src/Data/CM.TableNow.Restaurants.Data/Configurations/PhotoConfiguration.cs`

## Files to Modify

- `server/src/Data/CM.TableNow.Restaurants.Data/RestaurantsDbContext.cs` — Add `DbSet<Photo>`

## Technical Details

```csharp
// Photo.cs
public class Photo
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
}
```

Generate migration:
```powershell
dotnet ef migrations add AddPhotos `
  --project src/Migrations/CM.TableNow.Restaurants.Migrations `
  --startup-project src/Api --context RestaurantsDbContext
```

## Acceptance Criteria

- [ ] `Photo` entity exists; `RestaurantsDbContext.Photos` added; migration created
- [ ] `dotnet build` exits with code 0
