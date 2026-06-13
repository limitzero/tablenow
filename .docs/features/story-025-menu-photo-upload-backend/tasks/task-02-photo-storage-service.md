# Task 02: Photo Storage Service

## Status

pending

## Wave

1

## Description

Implement `IPhotoStorageService` abstraction and `LocalDiskPhotoStorageService` for development. The interface has a single `StoreAsync(IFormFile file, Guid restaurantId)` method returning the public URL of the stored file. The local implementation saves files to a configured directory and returns a relative URL. Registered in DI as a singleton.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-04-photo-application-layer.md

## Files to Create

- `server/src/Infrastructure/Photos/IPhotoStorageService.cs` — Interface.
- `server/src/Infrastructure/Photos/LocalDiskPhotoStorageService.cs` — Local disk implementation.
- `server/src/Infrastructure/Photos/PhotoStorageOptions.cs` — Options record with `StoragePath` and `BaseUrl`.

## Files to Modify

- `server/src/Api/Extensions/ServiceCollectionExtensions.cs` — Register the service.
- `server/src/Api/appsettings.json` — Add `Photos` config section.

## Technical Details

### Interface

```csharp
public interface IPhotoStorageService
{
    Task<string> StoreAsync(IFormFile file, Guid restaurantId, CancellationToken ct = default);
}
```

### LocalDiskPhotoStorageService

1. Read `StoragePath` from `PhotoStorageOptions`.
2. `Directory.CreateDirectory(path)` if it doesn't exist.
3. Generate a unique filename: `$"{restaurantId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}"`.
4. Save with `file.CopyToAsync(stream)`.
5. Return `$"{options.BaseUrl}/photos/{fileName}"`.

### appsettings.json

```json
"Photos": {
  "StoragePath": "./uploads/photos",
  "BaseUrl": "http://localhost:5000"
}
```

## Acceptance Criteria

- [ ] `IPhotoStorageService.StoreAsync` saves the file to disk and returns a URL.
- [ ] The storage directory is created automatically if it doesn't exist.
- [ ] Filenames are unique (GUID-based) to prevent collisions.
- [ ] `IPhotoStorageService` is registered in DI.
