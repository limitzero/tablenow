# Task 02: Slot Availability Application Handler & Endpoint

## Status

pending

## Wave

2

## Description

Wraps `GetAvailableSlotsQuery` in an application request/handler and exposes it at `GET /api/restaurants/{id}/slots?date=&partySize=`. Returns 400 for missing or invalid query parameters.

## Dependencies

**Depends on:** task-01-slot-query.md
**Blocks:** STORY-013 (frontend detail page calls this endpoint)

**Context from dependencies:** task-01 created `GetAvailableSlotsQuery(RestaurantId, Date, PartySize)` returning `IReadOnlyList<SlotDto>`. `SlotDto`: `SlotId (Guid)`, `Time (TimeOnly)`, `RemainingCapacity (int)`.

## Files to Create

- `server/src/Application/CM.TableNow.Restaurants.Application/Features/GetAvailableSlots/GetAvailableSlotsRequest.cs`
- `server/src/Application/CM.TableNow.Restaurants.Application/Features/GetAvailableSlots/GetAvailableSlotsRequestHandler.cs`

## Files to Modify

- `server/src/Api/Endpoints/RestaurantEndpoints.cs` — Add `GET /{id}/slots` route

## Technical Details

### Code Snippets

```csharp
// GetAvailableSlotsRequest.cs
public record GetAvailableSlotsRequest(Guid RestaurantId, DateOnly Date, int PartySize)
    : IRequest<Result<IReadOnlyList<SlotResponse>>>;

public record SlotResponse(Guid SlotId, string Time, int RemainingCapacity);
```

Add to `RestaurantEndpoints.MapRestaurantEndpoints()`:
```csharp
restaurants.MapGet("/{id:guid}/slots", async (
    Guid id,
    [FromQuery] string? date,
    [FromQuery] int? partySize,
    IMediator mediator,
    CancellationToken ct) =>
{
    if (!DateOnly.TryParse(date, out var parsedDate) || partySize is null or < 1)
        return Results.BadRequest("Valid 'date' (YYYY-MM-DD) and 'partySize' (≥1) are required.");

    var result = await mediator.Send(
        new GetAvailableSlotsRequest(id, parsedDate, partySize.Value), ct);
    return TypedResultHelper.ToResult(result);
})
.WithName("GetAvailableSlots")
.Produces<IReadOnlyList<SlotResponse>>(200)
.ProducesProblem(400);
```

### API Endpoint

```
GET /api/restaurants/{id}/slots?date=2026-07-15&partySize=2
200: [{ "slotId": "guid", "time": "18:00", "remainingCapacity": 4 }, ...]
400: when date or partySize missing/invalid
```

## Acceptance Criteria

- [ ] `GET /api/restaurants/{id}/slots?date=2026-07-15&partySize=2` returns 200 with filtered slots
- [ ] Missing `date` or `partySize` returns 400
- [ ] `partySize=0` returns 400
- [ ] Invalid date format (not YYYY-MM-DD) returns 400
- [ ] BDD test class `describe_get_available_slots` / `when_party_size_exceeds_remaining_capacity` passes
- [ ] `dotnet build` exits with code 0
