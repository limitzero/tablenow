# Task 04: Photo Application Layer

## Status

pending

## Wave

2

## Description

Implement `UploadPhotoRequest` handler that validates file size (≤ 5 MB) and MIME type (JPEG/PNG/WebP), calls `IPhotoStorageService.StoreAsync(...)`, and dispatches `SavePhotoCommand` with the returned URL. Returns `Result<PhotoResponse>` with the stored URL.

## Dependencies

**Depends on:** task-02-photo-storage-service.md
**Blocks:** task-05-photo-endpoints.md

## Files to Create

- `server/src/Application/Restaurants/Features/UploadPhoto/UploadPhotoRequest.cs`
- `server/src/Application/Restaurants/Features/UploadPhoto/UploadPhotoRequestHandler.cs`

## Technical Details

```csharp
public sealed record UploadPhotoRequest(Guid RestaurantId, IFormFile File)
    : IRequest<Result<PhotoResponse>>;
public sealed record PhotoResponse(Guid PhotoId, string Url);
```

Handler validation:
```csharp
if (request.File.Length > 5_242_880)
    return Result<PhotoResponse>.Failure(400, "File size must not exceed 5 MB");

var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
if (!allowedTypes.Contains(request.File.ContentType))
    return Result<PhotoResponse>.Failure(400, "File must be JPEG, PNG, or WebP");
```

Then call `storageService.StoreAsync(request.File, request.RestaurantId, ct)` to get URL, dispatch `SavePhotoCommand`, return `PhotoResponse`.

## Acceptance Criteria

- [ ] Returns 400 for files over 5 MB.
- [ ] Returns 400 for non-image MIME types.
- [ ] Returns 201-compatible result with `photoId` and `url` on success.
- [ ] URL comes from `IPhotoStorageService` (injectable, testable).
