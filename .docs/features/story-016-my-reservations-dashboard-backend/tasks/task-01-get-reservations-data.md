# Task 01: GetMyReservations Data Query

## Status

pending

## Wave

1

## Description

Create the Data-layer query that retrieves all reservations for a given user, enriched for the dashboard. `GetMyReservationsQuery(UserId)` and its handler join `Reservation` → `TimeSlot` → `Restaurant` in EF LINQ and project each row into a flat `MyReservationItem` record carrying `reservationId`, `restaurantName`, `date`, `time`, `partySize`, and `status`. The handler returns a `Result<IReadOnlyList<MyReservationItem>>` with a 200 status; an empty list is a successful result, not an error. This is the data foundation the Application handler (task 02) dispatches.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-03-my-reservations-endpoint.md (and consumed by task-02 via the query contract documented here)

**Context from dependencies:** None — Wave 1. Relies on prior stories: STORY-003 created the EF model with `User`, `Restaurant`, `TimeSlot`, and `Reservation` tables and Fluent API configurations applied via `ApplyConfigurationsFromAssembly`. STORY-014 created the `Reservation` entity/EF model and the `Reservations` module's `DbContext`, with `Reservation` having `Id`, `UserId`, a `TimeSlotId` foreign key (navigation `TimeSlot`), `PartySize`, and a `Status` (enum with `Confirmed`/`Cancelled`). `TimeSlot` has `Date`, `Time`, and a `Restaurant` navigation; `Restaurant` has `Name`. The shared `Result<T>` type lives in `CM.OpenTable.Common`. The Mediator query abstractions (`IQuery<T>` / `IQueryHandler<TQuery, TResult>`) follow the project's CQRS conventions.

## Files to Create

- `server/src/Data/Reservations/Queries/GetMyReservationsQuery.cs` — the query record, the `MyReservationItem` projection record, and the query handler.

## Files to Modify

None. (The `Reservations` module's `DbContext` and DI registration already exist from STORY-014; this handler is discovered by Mediator's source generator.)

## Technical Details

### Implementation Steps

1. Create the file in `Data/Reservations/Queries/`.
2. Define a `MyReservationItem` record with `ReservationId`, `RestaurantName`, `Date` (`DateOnly`), `Time` (`TimeOnly`), `PartySize` (`int`), `Status` (`string`).
3. Define `GetMyReservationsQuery(Guid UserId)` implementing the project's query marker interface returning `Result<IReadOnlyList<MyReservationItem>>`.
4. Implement `GetMyReservationsQueryHandler` with a primary constructor injecting the `Reservations` module `DbContext`.
5. Build the query in EF LINQ: filter `Reservations` by `UserId`, then `.Select(...)` projecting the joined fields via navigations (`r.TimeSlot.Restaurant.Name`, `r.TimeSlot.Date`, `r.TimeSlot.Time`). Convert the `Status` enum to string with `.ToString()`. Materialize with `ToListAsync(cancellationToken)`.
6. Return `Result<IReadOnlyList<MyReservationItem>>.Success(items, StatusCodes.Status200OK)`. An empty list is still a success.
7. `CancellationToken` flows to `ToListAsync`.

### Code Snippets

```csharp
namespace CM.OpenTable.Reservations.Data.Queries;

using CM.OpenTable.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

public sealed record MyReservationItem(
    Guid ReservationId,
    string RestaurantName,
    DateOnly Date,
    TimeOnly Time,
    int PartySize,
    string Status);

public sealed record GetMyReservationsQuery(Guid UserId)
    : IQuery<Result<IReadOnlyList<MyReservationItem>>>;

public sealed class GetMyReservationsQueryHandler(ReservationsDbContext db)
    : IQueryHandler<GetMyReservationsQuery, Result<IReadOnlyList<MyReservationItem>>>
{
    public async ValueTask<Result<IReadOnlyList<MyReservationItem>>> Handle(
        GetMyReservationsQuery query,
        CancellationToken cancellationToken)
    {
        var items = await db.Reservations
            .Where(r => r.UserId == query.UserId)
            .OrderBy(r => r.TimeSlot.Date)
            .ThenBy(r => r.TimeSlot.Time)
            .Select(r => new MyReservationItem(
                r.Id,
                r.TimeSlot.Restaurant.Name,
                r.TimeSlot.Date,
                r.TimeSlot.Time,
                r.PartySize,
                r.Status.ToString()))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<MyReservationItem>>.Success(items, StatusCodes.Status200OK);
    }
}
```

## Acceptance Criteria

- [ ] `GetMyReservationsQuery(Guid UserId)` exists in `Data/Reservations/Queries/` and returns `Result<IReadOnlyList<MyReservationItem>>`.
- [ ] The handler joins Reservation → TimeSlot → Restaurant via navigations and projects `reservationId`, `restaurantName`, `date`, `time`, `partySize`, `status` server-side in LINQ.
- [ ] The query filters by `UserId`.
- [ ] An empty result returns a successful `Result` with status 200 (not an error/404).
- [ ] The handler uses a primary constructor for DI, a `CancellationToken` on the async method, file-scoped namespace, and nullable enabled.

## Notes

- Confirm the exact `DbContext` type name and `Reservations`/`TimeSlots`/`Restaurants` `DbSet` names from STORY-014's Data project and adjust the snippet accordingly. The shape (filter by `UserId`, project via navigations) is the contract that matters.
- If `Status` is stored as a string column rather than an enum, drop the `.ToString()` and select it directly.
- Ordering by date/time is a nicety for the dashboard; it is not strictly required by the acceptance criteria but keeps the list stable.
- Do not use the repository pattern — use the `DbContext` directly.
