# Task 01: Slot Availability Data Query

## Status

complete

## Wave

1

## Description

Implement the Data-layer CQRS query that fetches available time slots from EF Core. `GetAvailableSlotsQuery` accepts a `restaurantId`, `date`, and `partySize` and returns `Result<IReadOnlyList<SlotData>>`. The handler filters entirely in the database — no loading all rows and filtering in memory — and projects to a lightweight `SlotData` record using `AsNoTracking`. This is the data source the Application layer (task-02) dispatches into.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-03-slot-availability-endpoint.md

**Context from dependencies:** This is a Wave 1 task. It assumes STORY-001 created the `CM.TableNow.Restaurants.Data` project with the Mediator source generator configured and `Result<T>` in `CM.TableNow.Shared.Results`. It assumes STORY-003 created the `TimeSlot` EF model (`Id`, `RestaurantId`, `StartTime`, `TotalCapacity`, `RemainingCapacity`, `RowVersion`) and the `RestaurantsDbContext` with a `TimeSlots` `DbSet<TimeSlot>`. The `SlotData` record defined here is the contract that task-02 maps to `TimeSlotResponse`.

## Files to Create

- `server/src/Data/Restaurants/Queries/GetAvailableSlots/GetAvailableSlotsQuery.cs` — `SlotData` record + `GetAvailableSlotsQuery` Mediator query.
- `server/src/Data/Restaurants/Queries/GetAvailableSlots/GetAvailableSlotsQueryHandler.cs` — EF LINQ handler.

## Files to Modify

- None (the `RestaurantsDbContext` and `TimeSlot` EF model already exist from STORY-003).

## Technical Details

### Implementation Steps

1. Define `SlotData` as a `record` with `Guid SlotId`, `DateTimeOffset Time`, `int RemainingCapacity`. Place it in the same file as the query so both layers can reference it via the Data project.
2. Define `GetAvailableSlotsQuery(Guid RestaurantId, DateOnly Date, int PartySize)` as a `record` implementing the Mediator query interface returning `Result<IReadOnlyList<SlotData>>`.
3. Implement `GetAvailableSlotsQueryHandler` using a primary constructor for `RestaurantsDbContext` DI.
4. In the handler, convert `DateOnly Date` to a UTC `DateTimeOffset` window: `startUtc = new DateTimeOffset(Date.Year, Date.Month, Date.Day, 0, 0, 0, TimeSpan.Zero)` and `endUtc = startUtc.AddDays(1)`.
5. Query with `.AsNoTracking().Where(ts => ts.RestaurantId == query.RestaurantId && ts.StartTime >= startUtc && ts.StartTime < endUtc && ts.RemainingCapacity >= query.PartySize)`.
6. Order by `ts.StartTime`, project to `SlotData` server-side (do not materialize entities then map), `ToListAsync(cancellationToken)`.
7. Return `Result<IReadOnlyList<SlotData>>.Success(slots)`.

### Code Snippets

```csharp
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Restaurants.Data.Queries.GetAvailableSlots;

public sealed record SlotData(
    Guid SlotId,
    DateTimeOffset Time,
    int RemainingCapacity);

public sealed record GetAvailableSlotsQuery(
    Guid RestaurantId,
    DateOnly Date,
    int PartySize) : IQuery<Result<IReadOnlyList<SlotData>>>;
```

```csharp
using CM.TableNow.Shared.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Restaurants.Data.Queries.GetAvailableSlots;

public sealed class GetAvailableSlotsQueryHandler(RestaurantsDbContext db)
    : IQueryHandler<GetAvailableSlotsQuery, Result<IReadOnlyList<SlotData>>>
{
    public async ValueTask<Result<IReadOnlyList<SlotData>>> Handle(
        GetAvailableSlotsQuery query,
        CancellationToken cancellationToken)
    {
        var startUtc = new DateTimeOffset(query.Date.Year, query.Date.Month, query.Date.Day, 0, 0, 0, TimeSpan.Zero);
        var endUtc = startUtc.AddDays(1);

        var slots = await db.TimeSlots
            .AsNoTracking()
            .Where(ts =>
                ts.RestaurantId == query.RestaurantId &&
                ts.StartTime >= startUtc &&
                ts.StartTime < endUtc &&
                ts.RemainingCapacity >= query.PartySize)
            .OrderBy(ts => ts.StartTime)
            .Select(ts => new SlotData(ts.Id, ts.StartTime, ts.RemainingCapacity))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<SlotData>>.Success(slots);
    }
}
```

## Acceptance Criteria

- [ ] `GetAvailableSlotsQueryHandler` returns only slots where `RemainingCapacity >= query.PartySize` on the requested date.
- [ ] A slot with `RemainingCapacity = 0` is excluded from results.
- [ ] The filter is applied in EF LINQ (server-side), not in memory.
- [ ] The handler uses `AsNoTracking` and projects server-side to `SlotData`.
- [ ] The handler accepts and propagates a `CancellationToken`.
- [ ] `SlotData` exposes `SlotId` (`Guid`), `Time` (`DateTimeOffset`), `RemainingCapacity` (`int`).

## Notes

- No repository pattern — `DbContext` is used directly in the handler per CLAUDE.md.
- The `DateOnly → DateTimeOffset` window conversion assumes slots are stored in UTC. If the project later adds timezone support this conversion point is where it would change.
- An empty result set is a success (`Result.Success` with an empty list), not a failure — the Application layer does not need to special-case it.
