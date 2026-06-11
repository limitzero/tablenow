# Task 03: Photos Endpoint

## Status

pending

## Wave

3

## Description

Registers `POST /api/restaurants/{id}/photos` with `IFormFile` upload and an Operator role authorization check. Configures max request size.

## Dependencies

**Depends on:** task-02-upload-handler-validation.md
**Blocks:** STORY-026 (frontend photo gallery calls GET which includes photos)

**Context from dependencies:** task-02 created `UploadPhotoRequest` and handler. STORY-010 created `RestaurantsEndpoints.cs`. This task adds the photos endpoint to that class.

## Files to Modify

- `server/src/Api/Endpoints/RestaurantsEndpoints.cs`
- `server/src/Api/Program.cs` — configure multipart request size limit

## Technical Details

### Code Snippets

```csharp
// Add to RestaurantsEndpoints:
group.MapPost("/{id:guid}/photos", async (
    Guid id,
    IFormFile file,
    HttpContext httpCtx,
    IMediator mediator,
    CancellationToken ct) =>
{
    var role = httpCtx.User.FindFirst(ClaimTypes.Role)?.Value;
    if (role != "Operator")
        return Results.Forbid();

    var result = await mediator.Send(new UploadPhotoRequest(id, file), ct);
    return TypedResultHelper.ToResult(result);
}).RequireAuthorization();
```

```csharp
// Program.cs — configure multipart size
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 6 * 1024 * 1024; // 6MB limit (server-side)
});
```

## Acceptance Criteria

- [ ] `POST /api/restaurants/{id}/photos` accepts `IFormFile`
- [ ] Returns 403 if JWT role is not "Operator"
- [ ] Returns 400 for invalid file (via validator)
- [ ] Returns 201 with photo URL on success
