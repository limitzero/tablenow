# Task 01: GetMyReservations Data Query

## Status

pending

## Wave

1

## Description

Implements `GetMyReservationsQuery` / `GetMyReservationsQueryHandler` in `Data/Reservations/Queries/`. Because reservations, time slots, and restaurants live in separate DbContexts, this query does two EF reads (reservations + time slots + restaurant names) and joins in memory at the application level.

## Dependencies

**Depends on:** STORY-014 task-04-endpoint.md (Reservations DbContext registered and reservation table populated)
**Blocks:** task-02-my-reservations-endpoint.md

**Context from dependencies:**
- `ReservationsDbContext` has `DbSet<Reservation>` and `DbSet<TimeSlot>` (added in STORY-014 task-01)
- `Reservation`: Id, UserId, TimeSlotId, PartySize, Status, CreatedAt
- `TimeSlot`: Id, RestaurantId, SlotDateTime
- Restaurant names come from `RestaurantsDbContext.Restaurants`

## Files to Create

- `server/src/Data/CM.TableNow.Reservations.Data/Queries/GetMyReservations/GetMyReservationsQuery.cs`
- `server/src/Data/CM.TableNow.Reservations.Data/Queries/GetMyReservations/GetMyReservationsQueryHandler.cs`

## Technical Details

### Code Snippets

```csharp
// GetMyReservationsQuery.cs
public record GetMyReservationsQuery(Guid UserId) : IRequest<IReadOnlyList<MyReservationDto>>;

public record MyReservationDto(
    Guid ReservationId,
    string RestaurantName,
    DateOnly Date,
    TimeOnly Time,
    int PartySize,
    string Status);
```

```csharp
// GetMyReservationsQueryHandler.cs
using Microsoft.EntityFrameworkCore;
using CM.TableNow.Restaurants.Data;

public class GetMyReservationsQueryHandler(
    ReservationsDbContext reservationsDb,
    RestaurantsDbContext restaurantsDb)
    : IRequestHandler<GetMyReservationsQuery, IReadOnlyList<MyReservationDto>>
{
    public async ValueTask<IReadOnlyList<MyReservationDto>> Handle(
        GetMyReservationsQuery query,
        CancellationToken ct)
    {
        var reservations = await reservationsDb.Reservations
            .AsNoTracking()
            .Where(r => r.UserId == query.UserId)
            .ToListAsync(ct);

        if (!reservations.Any()) return [];

        var slotIds = reservations.Select(r => r.TimeSlotId).ToList();
        var slots = await reservationsDb.TimeSlots
            .AsNoTracking()
            .Where(s => slotIds.Contains(s.Id))
            .ToListAsync(ct);

        var restaurantIds = slots.Select(s => s.RestaurantId).Distinct().ToList();
        var restaurants = await restaurantsDb.Restaurants
            .AsNoTracking()
            .Where(r => restaurantIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name, ct);

        return reservations
            .Join(slots, r => r.TimeSlotId, s => s.Id, (r, s) => new MyReservationDto(
                r.Id,
                restaurants.GetValueOrDefault(s.RestaurantId, "Unknown Restaurant"),
                DateOnly.FromDateTime(s.SlotDateTime),
                TimeOnly.FromDateTime(s.SlotDateTime),
                r.PartySize,
                r.Status))
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.Time)
            .ToList();
    }
}
```

## Acceptance Criteria

- [ ] Handler returns all reservations for a given `UserId`
- [ ] Returns empty list (not null) when user has no reservations
- [ ] Each DTO includes restaurantName from the Restaurants context
- [ ] `dotnet build` exits with code 0
