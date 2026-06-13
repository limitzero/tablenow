# Task 03: Photo Data Handlers

## Status

pending

## Wave

2

## Description

Implement `SavePhotoCommand` (inserts a `Photo` record with URL) and `GetPhotosQuery` (returns all photos for a restaurant). Both are Data-layer CQRS handlers.

## Dependencies

**Depends on:** task-01-photo-entity-migration.md
**Blocks:** task-05-photo-endpoints.md

## Files to Create

- `server/src/Data/Restaurants/Commands/SavePhoto/SavePhotoCommand.cs` — Command + handler.
- `server/src/Data/Restaurants/Queries/GetPhotos/GetPhotosQuery.cs` — Query + `PhotoData` record + handler.

## Technical Details

### SavePhotoCommand

```csharp
public sealed record SavePhotoCommand(Guid RestaurantId, string Url) : ICommand<Result<Guid>>;
```
Handler: insert `Photo { Id = Guid.NewGuid(), RestaurantId, Url, UploadedAt = DateTimeOffset.UtcNow }`, return ID.

### GetPhotosQuery

```csharp
public sealed record PhotoData(Guid Id, string Url, DateTimeOffset UploadedAt);
public sealed record GetPhotosQuery(Guid RestaurantId) : IQuery<Result<IReadOnlyList<PhotoData>>>;
```
Handler: `AsNoTracking`, project to `PhotoData`, order by `UploadedAt DESC`.

## Acceptance Criteria

- [ ] `SavePhotoCommand` inserts a photo record and returns its ID.
- [ ] `GetPhotosQuery` returns photos ordered newest-first.
- [ ] Both handlers use `CancellationToken`.
