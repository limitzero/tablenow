# Task 03: Photo Upload Handler & Endpoint

## Status

pending

## Wave

2

## Description

Creates application handler for photo upload with file validation (size ≤ 5 MB, MIME type JPEG/PNG/WebP) and operator role check. Adds `POST /api/restaurants/{id}/photos` endpoint. Also updates `GET /api/restaurants/{id}` to include the `photos` array.

## Dependencies

**Depends on:** task-01-photo-entity.md, task-02-photo-storage.md
**Blocks:** STORY-026

**Context from dependencies:**
- task-01: `Photo` entity, `RestaurantsDbContext.Photos`
- task-02: `IPhotoStorageService.SaveAsync(IFormFile)` returns URL string
- Operator role check: JWT `role` claim == "Operator"

## Files to Create

- `server/src/Data/CM.TableNow.Restaurants.Data/Commands/UploadPhoto/UploadPhotoCommand.cs`
- `server/src/Data/CM.TableNow.Restaurants.Data/Commands/UploadPhoto/UploadPhotoCommandHandler.cs`
- `server/src/Application/CM.TableNow.Restaurants.Application/Features/UploadPhoto/UploadPhotoRequest.cs`
- `server/src/Application/CM.TableNow.Restaurants.Application/Features/UploadPhoto/UploadPhotoRequestHandler.cs`

## Files to Modify

- `server/src/Api/Endpoints/RestaurantEndpoints.cs` — Add upload endpoint

## Technical Details

```csharp
// Endpoint (add to RestaurantEndpoints):
restaurants.MapPost("/{id:guid}/photos", async (
    Guid id, IFormFile file, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
{
    var role = user.FindFirstValue(ClaimTypes.Role);
    if (role != "Operator") return Results.Forbid();

    if (file.Length > 5 * 1024 * 1024)
        return Results.BadRequest("File must be ≤ 5 MB.");

    var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
    if (!allowedTypes.Contains(file.ContentType))
        return Results.BadRequest("File must be JPEG, PNG, or WebP.");

    var result = await mediator.Send(new UploadPhotoRequest(id, file), ct);
    return TypedResultHelper.ToResult(result);
})
.DisableAntiforgery()
.RequireAuthorization();
```

## Acceptance Criteria

- [ ] Valid upload by Operator returns 201 with photo URL
- [ ] File > 5 MB returns 400
- [ ] Invalid MIME type returns 400
- [ ] Non-Operator returns 403
- [ ] `dotnet build` exits with code 0
