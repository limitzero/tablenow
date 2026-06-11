# Task 02: Photo Storage Service

## Status

pending

## Wave

1

## Description

Creates `IPhotoStorageService` abstraction and a local-disk implementation that saves uploaded files to `server/uploads/`. Returns a public URL path for the saved file.

## Dependencies

**Depends on:** STORY-001 task-01-solution-projects.md
**Blocks:** task-03-photo-endpoint.md

## Files to Create

- `server/src/Infrastructure/CM.TableNow.Restaurants.Infrastructure/Storage/IPhotoStorageService.cs`
- `server/src/Infrastructure/CM.TableNow.Restaurants.Infrastructure/Storage/LocalPhotoStorageService.cs`
- `server/src/Infrastructure/CM.TableNow.Restaurants.Infrastructure/Storage/PhotoStorageOptions.cs`

## Technical Details

```csharp
// IPhotoStorageService.cs
public interface IPhotoStorageService
{
    Task<string> SaveAsync(IFormFile file, CancellationToken ct = default);
}

// PhotoStorageOptions.cs
public class PhotoStorageOptions
{
    public const string SectionName = "PhotoStorage";
    public string BasePath { get; set; } = "uploads";
    public string BaseUrl { get; set; } = "/uploads";
}

// LocalPhotoStorageService.cs
public class LocalPhotoStorageService(IOptions<PhotoStorageOptions> opts) : IPhotoStorageService
{
    public async Task<string> SaveAsync(IFormFile file, CancellationToken ct)
    {
        var basePath = opts.Value.BasePath;
        Directory.CreateDirectory(basePath);
        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(basePath, fileName);
        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream, ct);
        return $"{opts.Value.BaseUrl}/{fileName}";
    }
}
```

Register and add static file serving in `Program.cs`:
```csharp
app.UseStaticFiles(); // serves from wwwroot
// For uploads folder:
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});
```

## Acceptance Criteria

- [ ] `IPhotoStorageService` and `LocalPhotoStorageService` exist
- [ ] Saved files are accessible at `/uploads/{filename}`
- [ ] `dotnet build` exits with code 0
