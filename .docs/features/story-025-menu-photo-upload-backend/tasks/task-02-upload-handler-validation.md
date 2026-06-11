# Task 02: Upload Handler & Validation

## Status

pending

## Wave

2

## Description

Creates `IPhotoStorageService` abstraction, local disk implementation, the Application-layer handler, and Data command. Validates file size and MIME type. Stores file to configured path.

## Dependencies

**Depends on:** task-01-photo-entity-migration.md
**Blocks:** task-03-photos-endpoint.md

**Context from dependencies:** task-01 created `Photo` entity and `DbSet<Photo>`. This task creates the upload pipeline.

## Files to Create

- `server/src/Infrastructure/Notifications/IPhotoStorageService.cs` (or Infrastructure/Restaurants/)
- `server/src/Infrastructure/Restaurants/LocalPhotoStorageService.cs`
- `server/src/Application/Restaurants/Features/UploadPhoto/UploadPhotoRequest.cs` (+ Handler + Validator)
- `server/src/Data/Restaurants/Commands/SavePhotoCommand.cs` (+ Handler)

## Technical Details

### Code Snippets

```csharp
// IPhotoStorageService.cs
public interface IPhotoStorageService
{
    Task<string> SaveAsync(Stream fileStream, string filename, CancellationToken ct);
}

// LocalPhotoStorageService.cs
public class LocalPhotoStorageService(IWebHostEnvironment env) : IPhotoStorageService
{
    private static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxBytes = 5 * 1024 * 1024; // 5MB

    public async Task<string> SaveAsync(Stream fileStream, string filename, CancellationToken ct)
    {
        var uploadsPath = Path.Combine(env.WebRootPath, "photos");
        Directory.CreateDirectory(uploadsPath);
        var uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(filename)}";
        var filePath = Path.Combine(uploadsPath, uniqueName);
        await using var dest = File.OpenWrite(filePath);
        await fileStream.CopyToAsync(dest, ct);
        return $"/photos/{uniqueName}";
    }
}
```

```csharp
// Validator (in Application layer)
public class UploadPhotoRequestValidator : AbstractValidator<UploadPhotoRequest>
{
    private static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp"];

    public UploadPhotoRequestValidator()
    {
        RuleFor(x => x.File.Length).LessThanOrEqualTo(5 * 1024 * 1024)
            .WithMessage("File must be 5MB or smaller.");
        RuleFor(x => x.File.ContentType)
            .Must(ct => AllowedMimeTypes.Contains(ct))
            .WithMessage("Only JPEG, PNG, and WebP images are allowed.");
    }
}
```

## Acceptance Criteria

- [ ] `IPhotoStorageService.SaveAsync` abstracts storage
- [ ] `LocalPhotoStorageService` saves to `wwwroot/photos/`
- [ ] Validator rejects files > 5MB
- [ ] Validator rejects non image/jpeg/png/webp MIME types
- [ ] Handler stores file and inserts `Photo` record
