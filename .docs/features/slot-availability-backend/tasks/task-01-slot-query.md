# Task 01: GetAvailableSlots Data Query

## Status

pending

## Wave

1

## Description

Implements `GetAvailableSlotsQuery` / `GetAvailableSlotsQueryHandler` in `Data/Restaurants/Queries/`. Filters time slots by restaurant, date, and minimum party size — entirely in EF LINQ.

## Dependencies

**Depends on:** STORY-003 task-02-ef-models-configs.md (RestaurantsDbContext with TimeSlots DbSet)
**Blocks:** task-02-slot-endpoint.md

**Context from dependencies:** `TimeSlot` entity: `Id`, `RestaurantId`, `SlotDateTime (DateTime)`, `TotalCapacity`, `RemainingCapacity`, `RowVersion`. The query must filter `s.SlotDateTime.Date == date && s.RemainingCapacity >= partySize`.

## Files to Create

- `server/src/Data/CM.TableNow.Restaurants.Data/Queries/GetAvailableSlots/GetAvailableSlotsQuery.cs`
- `server/src/Data/CM.TableNow.Restaurants.Data/Queries/GetAvailableSlots/GetAvailableSlotsQueryHandler.cs`

## Technical Details

### Code Snippets

```csharp
// GetAvailableSlotsQuery.cs
namespace CM.TableNow.Restaurants.Data.Queries.GetAvailableSlots;

public record GetAvailableSlotsQuery(Guid RestaurantId, DateOnly Date, int PartySize)
    : IRequest<IReadOnlyList<SlotDto>>;

public record SlotDto(Guid SlotId, TimeOnly Time, int RemainingCapacity);
```

```csharp
// GetAvailableSlotsQueryHandler.cs
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Restaurants.Data.Queries.GetAvailableSlots;

public class GetAvailableSlotsQueryHandler(RestaurantsDbContext db)
    : IRequestHandler<GetAvailableSlotsQuery, IReadOnlyList<SlotDto>>
{
    public async ValueTask<IReadOnlyList<SlotDto>> Handle(
        GetAvailableSlotsQuery query,
        CancellationToken ct)
        => await db.TimeSlots
            .AsNoTracking()
            .Where(s =>
                s.RestaurantId == query.RestaurantId &&
                DateOnly.FromDateTime(s.SlotDateTime) == query.Date &&
                s.RemainingCapacity >= query.PartySize)
            .OrderBy(s => s.SlotDateTime)
            .Select(s => new SlotDto(
                s.Id,
                TimeOnly.FromDateTime(s.SlotDateTime),
                s.RemainingCapacity))
            .ToListAsync(ct);
}
```

## Acceptance Criteria

- [ ] Query and handler exist at specified paths
- [ ] Filter applied in EF LINQ — no in-memory filtering
- [ ] Slots ordered by time ascending
- [ ] Fully booked slots (`RemainingCapacity = 0`) excluded when `partySize >= 1`
- [ ] `dotnet build` exits with code 0
