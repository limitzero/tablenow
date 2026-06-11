# Task 01: GetAvailableSlots Handler

## Status

pending

## Wave

1

## Description

Creates the `GetAvailableSlotsQuery` with EF LINQ filtering, the Application-layer request/handler, `SlotDto`, and a FluentValidation validator that rejects invalid date/partySize values.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-003 AppDbContext, STORY-001 projects)
**Blocks:** task-02-slots-endpoint.md, STORY-013 (frontend calls this)

**Context from dependencies:** STORY-003 created `AppDbContext.TimeSlots` with `DbSet<TimeSlot>`. `TimeSlot` has `RestaurantId`, `DateTime` (DateTimeOffset), `RemainingCapacity`. `SlotDto` is a new contract record to be created in `Contracts/Restaurants/`.

## Files to Create

- `server/src/Contracts/Restaurants/SlotDto.cs`
- `server/src/Data/Restaurants/Queries/GetAvailableSlotsQuery.cs`
- `server/src/Data/Restaurants/Queries/GetAvailableSlotsQueryHandler.cs`
- `server/src/Application/Restaurants/Features/GetAvailableSlots/GetAvailableSlotsRequest.cs`
- `server/src/Application/Restaurants/Features/GetAvailableSlots/GetAvailableSlotsRequestHandler.cs`
- `server/src/Application/Restaurants/Features/GetAvailableSlots/GetAvailableSlotsRequestValidator.cs`

## Technical Details

### Code Snippets

```csharp
// Contracts/Restaurants/SlotDto.cs
namespace TableNow.Contracts.Restaurants;
public record SlotDto(Guid SlotId, TimeOnly Time, int RemainingCapacity);
```

```csharp
// Data/Restaurants/Queries/GetAvailableSlotsQuery.cs
namespace TableNow.Data.Restaurants.Queries;
public record GetAvailableSlotsQuery(Guid RestaurantId, DateOnly Date, int PartySize)
    : IRequest<Result<List<SlotDto>>>;

// GetAvailableSlotsQueryHandler.cs
public class GetAvailableSlotsQueryHandler(AppDbContext db)
    : IRequestHandler<GetAvailableSlotsQuery, Result<List<SlotDto>>>
{
    public async ValueTask<Result<List<SlotDto>>> Handle(
        GetAvailableSlotsQuery query, CancellationToken ct)
    {
        var slots = await db.TimeSlots
            .Where(ts => ts.RestaurantId == query.RestaurantId
                      && DateOnly.FromDateTime(ts.DateTime.Date) == query.Date
                      && ts.RemainingCapacity >= query.PartySize)
            .OrderBy(ts => ts.DateTime)
            .Select(ts => new SlotDto(ts.Id, TimeOnly.FromDateTime(ts.DateTime.DateTime), ts.RemainingCapacity))
            .ToListAsync(ct);

        return Result<List<SlotDto>>.Success(slots);
    }
}
```

```csharp
// Application/Restaurants/Features/GetAvailableSlots/GetAvailableSlotsRequest.cs
namespace TableNow.Application.Restaurants.Features.GetAvailableSlots;
public record GetAvailableSlotsRequest(Guid RestaurantId, DateOnly Date, int PartySize)
    : IRequest<Result<List<SlotDto>>>;

// GetAvailableSlotsRequestValidator.cs
public class GetAvailableSlotsRequestValidator : AbstractValidator<GetAvailableSlotsRequest>
{
    public GetAvailableSlotsRequestValidator()
    {
        RuleFor(x => x.Date).NotEqual(default(DateOnly));
        RuleFor(x => x.PartySize).InclusiveBetween(1, 20);
    }
}
```

## Acceptance Criteria

- [ ] `SlotDto` exists in `Contracts/Restaurants/`
- [ ] Query filters `RemainingCapacity >= PartySize` in the LINQ WHERE clause (not post-query)
- [ ] Query filters by `RestaurantId` and `Date`
- [ ] Validator rejects default DateOnly and partySize outside 1–20
- [ ] Application handler delegates to Data query via IMediator
