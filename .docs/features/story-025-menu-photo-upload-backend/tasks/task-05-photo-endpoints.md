# Task 05: Photo Endpoints

## Status

pending

## Wave

3

## Description

Add `POST /api/restaurants/{id}/photos` (operator-only, `IFormFile` multipart upload) to `PhotosEndpoints`. Also extend `GET /api/restaurants/{id}` to include a `photos` array by updating `GetRestaurantByIdQueryHandler` to join with the `Photos` table.

## Dependencies

**Depends on:** task-03-photo-data-handlers.md, task-04-photo-application-layer.md
**Blocks:** None

## Files to Create

- `server/src/Api/Endpoints/Restaurants/PhotosEndpoints.cs` — Static class with upload endpoint.
- `server/src/Contracts/Restaurants/PhotoResponse.cs` — API DTO (if not already created by task-04).

## Files to Modify

- `server/src/Data/Restaurants/Queries/GetRestaurantById/GetRestaurantByIdQueryHandler.cs` — Include `Photos` in the projection.

## Technical Details

```csharp
// POST — operator only
group.MapPost("/{restaurantId:guid}/photos",
    async (Guid restaurantId, IFormFile file, HttpContext ctx, IMediator mediator, CancellationToken ct) =>
    {
        var role = ctx.User.FindFirstValue(ClaimTypes.Role);
        if (role != "Operator")
            return Results.Forbid();

        var result = await mediator.Send(new UploadPhotoRequest(restaurantId, file), ct);
        return TypedResultHelper.ToResult(result);
    }).RequireAuthorization();
```

Configure multipart form options if needed: `builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 5_242_880)`.

## Acceptance Criteria

- [ ] `POST /api/restaurants/{id}/photos` by Operator returns 201 with photo URL.
- [ ] Non-Operator JWT returns 403.
- [ ] File > 5 MB returns 400.
- [ ] `GET /api/restaurants/{id}` response includes `photos` array.
