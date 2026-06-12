# Implementation Plan: My Reservations Dashboard — Backend

## Overview

Build the `GET /api/reservations/my` read API across the three CQRS layers: a Data query that joins Reservation + TimeSlot + Restaurant and projects the dashboard shape, an Application request/handler that resolves the caller's `userId` from JWT claims and dispatches the query, and a protected Minimal API endpoint that maps the result with `TypedResultHelper`.

## Phase 1: Data Query & Application Handler (parallel)

The Data query and the Application handler are built in parallel. They live in different projects/layers and do not share files; the Application handler depends only on the data query's request/response contract documented in task 01.

### Tasks

- [ ] Task 01 — `GetMyReservationsQuery(UserId)` / `GetMyReservationsQueryHandler` in `Data/Reservations/Queries/`. EF LINQ join Reservation → TimeSlot → Restaurant, filtered by `UserId`, projected to a `MyReservationItem` record (`reservationId`, `restaurantName`, `date`, `time`, `partySize`, `status`). Returns `Result<IReadOnlyList<MyReservationItem>>`. Empty list is a successful 200.
- [ ] Task 02 — `GetMyReservationsRequest` / `GetMyReservationsRequestHandler` in `Application/Reservations/Features/GetMyReservations/`. Reads `userId` from JWT claims via `IHttpContextAccessor`, dispatches `GetMyReservationsQuery` through `IMediator`, returns `Result<IReadOnlyList<MyReservationResponse>>`.

### Technical Details

- Data query handler uses the `ReservationsDbContext` (or the module's `DbContext`) directly — no repository pattern.
- Projection performed server-side in LINQ (`.Select(...)`), not in memory.
- The `userId` claim name is the standard `ClaimTypes.NameIdentifier` (or the `"userId"`/`sub` claim configured in STORY-006/007). Read it from `IHttpContextAccessor.HttpContext.User`.
- A missing/invalid `userId` claim → return `Result` with 401 status (defensive; the endpoint's `RequireAuthorization()` should already block unauthenticated callers).

### Code Snippets

Data query (`Data/Reservations/Queries/GetMyReservationsQuery.cs`):

```csharp
namespace CM.OpenTable.Reservations.Data.Queries;

public sealed record MyReservationItem(
    Guid ReservationId,
    string RestaurantName,
    DateOnly Date,
    TimeOnly Time,
    int PartySize,
    string Status);

public sealed record GetMyReservationsQuery(Guid UserId) : IQuery<Result<IReadOnlyList<MyReservationItem>>>;

public sealed class GetMyReservationsQueryHandler(ReservationsDbContext db)
    : IQueryHandler<GetMyReservationsQuery, Result<IReadOnlyList<MyReservationItem>>>
{
    public async ValueTask<Result<IReadOnlyList<MyReservationItem>>> Handle(
        GetMyReservationsQuery query, CancellationToken cancellationToken)
    {
        var items = await db.Reservations
            .Where(r => r.UserId == query.UserId)
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

## Phase 2: Endpoint

### Tasks

- [ ] Task 03 — `GET /api/reservations/my` in `Api/Endpoints/Reservations/`. `RequireAuthorization()`. Dispatch `GetMyReservationsRequest` via `IMediator`, map the response with the static `ReservationMapper`, return via `TypedResultHelper`.

### Technical Details

- Add the route to the existing `ReservationEndpoints` static class (`MapReservationEndpoints(RouteGroupBuilder)`), created by STORY-014, or create it if absent.
- The endpoint group is protected with `.RequireAuthorization()` so unauthenticated requests get 401 before reaching the handler.
- `ReservationMapper` translates `MyReservationResponse` (Application) → `MyReservationApiModel` (Contracts/Api), or the Application response is already API-shaped and mapped 1:1.
