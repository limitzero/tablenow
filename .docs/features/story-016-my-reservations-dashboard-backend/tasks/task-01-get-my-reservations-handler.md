# Task 01: GetMyReservations Handler

## Status

pending

## Wave

1

## Description

Creates `GetMyReservationsQuery` with EF join, Application handler, and `MyReservationDto` contract record.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-003 AppDbContext, STORY-014 Reservation rows exist)
**Blocks:** task-02-my-reservations-endpoint.md, STORY-018

**Context from dependencies:** STORY-003 created `AppDbContext` with `DbSet<Reservation>`, `DbSet<TimeSlot>`, `DbSet<Restaurant>`. The query joins all three. `MyReservationDto` is a new contract record.

## Files to Create

- `server/src/Contracts/Reservations/MyReservationDto.cs`
- `server/src/Data/Reservations/Queries/GetMyReservationsQuery.cs`
- `server/src/Data/Reservations/Queries/GetMyReservationsQueryHandler.cs`
- `server/src/Application/Reservations/Features/GetMyReservations/GetMyReservationsRequest.cs`
- `server/src/Application/Reservations/Features/GetMyReservations/GetMyReservationsRequestHandler.cs`

## Technical Details

### Code Snippets

```csharp
// Contracts/Reservations/MyReservationDto.cs
namespace TableNow.Contracts.Reservations;
public record MyReservationDto(
    Guid ReservationId, string RestaurantName,
    DateOnly Date, TimeOnly Time, int PartySize, string Status);
```

```csharp
// Data/Reservations/Queries/GetMyReservationsQuery.cs
namespace TableNow.Data.Reservations.Queries;
public record GetMyReservationsQuery(Guid UserId) : IRequest<Result<List<MyReservationDto>>>;

// Handler
public class GetMyReservationsQueryHandler(AppDbContext db)
    : IRequestHandler<GetMyReservationsQuery, Result<List<MyReservationDto>>>
{
    public async ValueTask<Result<List<MyReservationDto>>> Handle(
        GetMyReservationsQuery query, CancellationToken ct)
    {
        var reservations = await db.Reservations
            .Where(r => r.UserId == query.UserId)
            .Join(db.TimeSlots, r => r.SlotId, ts => ts.Id, (r, ts) => new { r, ts })
            .Join(db.Restaurants, x => x.ts.RestaurantId, rest => rest.Id, (x, rest) => new MyReservationDto(
                x.r.Id,
                rest.Name,
                DateOnly.FromDateTime(x.ts.DateTime.Date),
                TimeOnly.FromDateTime(x.ts.DateTime.DateTime),
                x.r.PartySize,
                x.r.Status))
            .OrderByDescending(r => r.Date)
            .ToListAsync(ct);

        return Result<List<MyReservationDto>>.Success(reservations);
    }
}
```

```csharp
// Application/Reservations/Features/GetMyReservations/GetMyReservationsRequest.cs
// No parameters — userId read from IHttpContextAccessor in handler
public record GetMyReservationsRequest : IRequest<Result<List<MyReservationDto>>>;

// Handler reads userId from claims and sends Data query
```

## Acceptance Criteria

- [ ] `MyReservationDto` has reservationId, restaurantName, date, time, partySize, status
- [ ] Query joins Reservation → TimeSlot → Restaurant
- [ ] Returns empty list (not 404) when user has no reservations
- [ ] Application handler reads userId from IHttpContextAccessor claims
