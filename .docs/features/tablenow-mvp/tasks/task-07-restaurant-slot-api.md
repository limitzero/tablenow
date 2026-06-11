# Task 07: Restaurant & Slot API

## Status

pending

## Phase

4

## Description

Implement three public (no-auth) endpoints for the restaurants module: `GET /api/restaurants` (list all), `GET /api/restaurants/{id}` (detail), and `GET /api/restaurants/{id}/slots?date=&partySize=` (filtered time slots). All three use the CQRS/Mediator pattern with dedicated Application handlers and Data query handlers. The slots endpoint is the most critical — it must filter in EF LINQ (not in memory) and must respond in < 300ms.

## Dependencies

**Depends on:** task-04-database-seed  
**Blocks:** task-09-reservation-creation-backend, task-13-restaurant-listing-frontend, task-14-restaurant-detail-frontend

**Context from dependencies:** task-04 seeded 15 `RestaurantModel` rows and 4 × 30 × 15 `TimeSlotModel` rows into `AppDbContext`. `RestaurantModel` has `Id`, `Name`, `Cuisine`, `Address`, `Description`, `ThumbnailUrl`. `TimeSlotModel` has `Id`, `RestaurantId`, `Date`, `Time`, `TotalCapacity`, `RemainingCapacity`, `RowVersion`.

## Files to Create

**Data queries:**
- `server/src/Data/Restaurants/Queries/GetRestaurants/GetRestaurantsQuery.cs`
- `server/src/Data/Restaurants/Queries/GetRestaurants/GetRestaurantsQueryHandler.cs`
- `server/src/Data/Restaurants/Queries/GetRestaurantById/GetRestaurantByIdQuery.cs`
- `server/src/Data/Restaurants/Queries/GetRestaurantById/GetRestaurantByIdQueryHandler.cs`
- `server/src/Data/Restaurants/Queries/GetAvailableSlots/GetAvailableSlotsQuery.cs`
- `server/src/Data/Restaurants/Queries/GetAvailableSlots/GetAvailableSlotsQueryHandler.cs`

**Application layer:**
- `server/src/Application/Restaurants/Features/GetRestaurants/GetRestaurantsRequest.cs`
- `server/src/Application/Restaurants/Features/GetRestaurants/GetRestaurantsRequestHandler.cs`
- `server/src/Application/Restaurants/Features/GetRestaurantById/GetRestaurantByIdRequest.cs`
- `server/src/Application/Restaurants/Features/GetRestaurantById/GetRestaurantByIdRequestHandler.cs`
- `server/src/Application/Restaurants/Features/GetAvailableSlots/GetAvailableSlotsRequest.cs`
- `server/src/Application/Restaurants/Features/GetAvailableSlots/GetAvailableSlotsRequestHandler.cs`
- `server/src/Application/Restaurants/RestaurantsModule.cs`

**API layer:**
- `server/src/Api/Restaurants/RestaurantsEndpoints.cs`
- `server/src/Api/Restaurants/RestaurantsMapper.cs`
- `server/src/Contracts/Restaurants/RestaurantSummaryDto.cs`
- `server/src/Contracts/Restaurants/RestaurantDetailDto.cs`
- `server/src/Contracts/Restaurants/SlotDto.cs`

## Files to Modify

- `server/src/Api/ServiceCollectionExtensions.cs` — call `services.AddRestaurantsModule()`
- `server/src/Api/Program.cs` — register `MapRestaurantEndpoints()` on the API group

## Technical Details

### Implementation Steps

1. **Write all Data query records and handlers** (directly against `AppDbContext`, `AsNoTracking()`).

2. **Write Application request/response records and handlers** (send data queries via `IMediator`).

3. **Write Contracts DTOs**.

4. **Write `RestaurantsMapper`** (static class mapping API DTOs ↔ Application requests/responses).

5. **Write `RestaurantsEndpoints`** with the three routes.

6. **Register module and endpoints** in `ServiceCollectionExtensions` and `Program.cs`.

### Code Snippets

**Contracts DTOs:**
```csharp
// RestaurantSummaryDto.cs
namespace TableNow.Contracts.Restaurants;
public sealed record RestaurantSummaryDto(
    Guid Id, string Name, string Cuisine, string Address, string? ThumbnailUrl);

// RestaurantDetailDto.cs
public sealed record RestaurantDetailDto(
    Guid Id, string Name, string Cuisine, string Address,
    string Description, string? ThumbnailUrl);

// SlotDto.cs
public sealed record SlotDto(Guid SlotId, string Time, int RemainingCapacity);
```

**`GetAvailableSlotsQuery` (the critical filtered query):**
```csharp
namespace TableNow.Restaurants.Data.Queries.GetAvailableSlots;

public sealed record GetAvailableSlotsQuery(
    Guid RestaurantId, DateOnly Date, int PartySize)
    : IRequest<IReadOnlyList<SlotQueryResult>>;

public sealed record SlotQueryResult(Guid SlotId, TimeOnly Time, int RemainingCapacity);

public sealed class GetAvailableSlotsQueryHandler(AppDbContext db)
    : IRequestHandler<GetAvailableSlotsQuery, IReadOnlyList<SlotQueryResult>>
{
    public async ValueTask<IReadOnlyList<SlotQueryResult>> Handle(
        GetAvailableSlotsQuery query, CancellationToken cancellationToken)
        => await db.TimeSlots
            .AsNoTracking()
            .Where(s => s.RestaurantId == query.RestaurantId
                     && s.Date == query.Date
                     && s.RemainingCapacity >= query.PartySize)
            .OrderBy(s => s.Time)
            .Select(s => new SlotQueryResult(s.Id, s.Time, s.RemainingCapacity))
            .ToListAsync(cancellationToken);
}
```

**`GetAvailableSlotsRequestHandler` (Application layer):**
```csharp
namespace TableNow.Restaurants.Application.Features.GetAvailableSlots;

public sealed record GetAvailableSlotsRequest(
    Guid RestaurantId, DateOnly Date, int PartySize)
    : IRequest<Result<IReadOnlyList<SlotResponse>>>;

public sealed record SlotResponse(Guid SlotId, string Time, int RemainingCapacity);

public sealed class GetAvailableSlotsRequestHandler(IMediator mediator)
    : IRequestHandler<GetAvailableSlotsRequest, Result<IReadOnlyList<SlotResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<SlotResponse>>> Handle(
        GetAvailableSlotsRequest request, CancellationToken cancellationToken)
    {
        if (request.PartySize < 1 || request.PartySize > 20)
            return Result<IReadOnlyList<SlotResponse>>.Failure(400, "Party size must be 1–20.");

        var slots = await mediator.Send(
            new GetAvailableSlotsQuery(request.RestaurantId, request.Date, request.PartySize),
            cancellationToken);

        var response = slots.Select(s =>
            new SlotResponse(s.SlotId, s.Time.ToString("HH:mm"), s.RemainingCapacity))
            .ToList();

        return Result<IReadOnlyList<SlotResponse>>.Success(response);
    }
}
```

**`RestaurantsEndpoints.cs`:**
```csharp
namespace TableNow.Api.Restaurants;

public static class RestaurantsEndpoints
{
    public static RouteGroupBuilder MapRestaurantEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/restaurants", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetRestaurantsRequest(), ct);
            return TypedResultHelper.ToResult(result);
        });

        group.MapGet("/restaurants/{id:guid}", async (
            Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetRestaurantByIdRequest(id), ct);
            return TypedResultHelper.ToResult(result);
        });

        group.MapGet("/restaurants/{id:guid}/slots", async (
            Guid id,
            [FromQuery] string date,
            [FromQuery] int partySize,
            IMediator mediator,
            CancellationToken ct) =>
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
                return Results.BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });

            var result = await mediator.Send(
                new GetAvailableSlotsRequest(id, parsedDate, partySize), ct);
            return TypedResultHelper.ToResult(result);
        });

        return group;
    }
}
```

**API response shapes:**
```json
// GET /api/v1/restaurants — 200
[
  {
    "id": "uuid",
    "name": "Bella Notte",
    "cuisine": "Italian",
    "address": "123 Main St, Chicago, IL",
    "thumbnailUrl": null
  }
]

// GET /api/v1/restaurants/{id}/slots?date=2026-06-20&partySize=4 — 200
[
  { "slotId": "uuid", "time": "18:00", "remainingCapacity": 6 },
  { "slotId": "uuid", "time": "20:00", "remainingCapacity": 10 }
]
```

## Acceptance Criteria

- [ ] `GET /api/v1/restaurants` returns 200 with all 15+ seeded restaurants (no auth required)
- [ ] `GET /api/v1/restaurants/{id}` returns the single restaurant detail or 404 if not found
- [ ] `GET /api/v1/restaurants/{id}/slots?date=2026-06-20&partySize=4` returns only slots with `RemainingCapacity >= 4`
- [ ] Slots with `RemainingCapacity = 0` are excluded from slot results
- [ ] Invalid `date` format returns 400 with a descriptive error
- [ ] Slot filtering is done in EF LINQ — no `ToList()` before the `Where()` clause
- [ ] `dotnet build` passes after all files are added

## Notes

- Use `AsNoTracking()` on all read-only queries — these are display-only operations.
- The `TimeOnly.ToString("HH:mm")` format produces `"18:00"` — consistent with the API spec in the PRD.
- Add a composite index on `(RestaurantId, Date)` in `TimeSlotConfiguration` (done in task-03) — this is critical for the slots query performance target of < 300ms.
- `[FromQuery]` is used for `date` and `partySize` parameters since they come from the query string.
