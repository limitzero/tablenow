# Task 11: Reservation Management Backend

## Status

pending

## Phase

6

## Description

Add two reservation management endpoints to the existing reservations module: `GET /api/reservations/my` (lists all reservations for the authenticated user) and `DELETE /api/reservations/{id}` (cancels a reservation and atomically restores slot capacity). The `userId` is always extracted from the JWT — never from the request body. Cancellation enforces ownership (403 if the JWT user doesn't own the reservation) and is idempotent in that cancelling an already-cancelled reservation returns 409.

## Dependencies

**Depends on:** task-09-reservation-creation-backend  
**Blocks:** task-15-my-reservations-dashboard-frontend, task-16-reservation-booking-frontend

**Context from dependencies:** task-09 created `ReservationsEndpoints.cs` (the `MapReservationEndpoints()` static method), `ReservationsMapper.cs`, and the `ReservationsModule`. `AppDbContext` has `DbSet<ReservationModel>` with `UserId`, `TimeSlotId`, `PartySize`, `Status`, and `CreatedAt`. `TimeSlotModel` has `RemainingCapacity`. The JWT claim `"userId"` contains the user's GUID.

## Files to Create

**Data queries/commands:**
- `server/src/Data/Reservations/Queries/GetMyReservations/GetMyReservationsQuery.cs`
- `server/src/Data/Reservations/Queries/GetMyReservations/GetMyReservationsQueryHandler.cs`
- `server/src/Data/Reservations/Commands/CancelReservation/CancelReservationCommand.cs`
- `server/src/Data/Reservations/Commands/CancelReservation/CancelReservationCommandHandler.cs`

**Application layer:**
- `server/src/Application/Reservations/Features/GetMyReservations/GetMyReservationsRequest.cs`
- `server/src/Application/Reservations/Features/GetMyReservations/GetMyReservationsRequestHandler.cs`
- `server/src/Application/Reservations/Features/CancelReservation/CancelReservationRequest.cs`
- `server/src/Application/Reservations/Features/CancelReservation/CancelReservationRequestHandler.cs`

**Contracts:**
- `server/src/Contracts/Reservations/MyReservationDto.cs`

## Files to Modify

- `server/src/Api/Reservations/ReservationsEndpoints.cs` — add `GET /reservations/my` and `DELETE /reservations/{id}` routes
- `server/src/Api/Reservations/ReservationsMapper.cs` — add mapping methods for new requests

## Technical Details

### Implementation Steps

1. **Write `GetMyReservationsQueryHandler`** — joins Reservation + TimeSlot + Restaurant.

2. **Write `GetMyReservationsRequestHandler`** — passes userId through to data layer.

3. **Write `CancelReservationCommandHandler`** — ownership check + atomic status update + capacity restore.

4. **Write `CancelReservationRequestHandler`** — passes userId and reservationId through.

5. **Add routes to `ReservationsEndpoints`.**

### Code Snippets

**`MyReservationDto` contract:**
```csharp
namespace TableNow.Contracts.Reservations;
public sealed record MyReservationDto(
    Guid ReservationId,
    string RestaurantName,
    string Date,
    string Time,
    int PartySize,
    string Status);
```

**`GetMyReservationsQueryHandler`:**
```csharp
namespace TableNow.Reservations.Data.Queries.GetMyReservations;

public sealed record GetMyReservationsQuery(Guid UserId)
    : IRequest<IReadOnlyList<MyReservationQueryResult>>;

public sealed record MyReservationQueryResult(
    Guid ReservationId, string RestaurantName,
    string Date, string Time, int PartySize, string Status);

public sealed class GetMyReservationsQueryHandler(AppDbContext db)
    : IRequestHandler<GetMyReservationsQuery, IReadOnlyList<MyReservationQueryResult>>
{
    public async ValueTask<IReadOnlyList<MyReservationQueryResult>> Handle(
        GetMyReservationsQuery query, CancellationToken cancellationToken)
        => await db.Reservations
            .AsNoTracking()
            .Where(r => r.UserId == query.UserId)
            .Join(db.TimeSlots, r => r.TimeSlotId, s => s.Id, (r, s) => new { r, s })
            .Join(db.Restaurants, rs => rs.s.RestaurantId, rest => rest.Id,
                (rs, rest) => new MyReservationQueryResult(
                    rs.r.Id,
                    rest.Name,
                    rs.s.Date.ToString("yyyy-MM-dd"),
                    rs.s.Time.ToString("HH:mm"),
                    rs.r.PartySize,
                    rs.r.Status))
            .OrderByDescending(r => r.Date)
            .ToListAsync(cancellationToken);
}
```

**`CancelReservationCommandHandler` — ownership check + atomic update:**
```csharp
namespace TableNow.Reservations.Data.Commands.CancelReservation;

public sealed record CancelReservationCommand(Guid UserId, Guid ReservationId)
    : IRequest<CancelReservationCommandResult>;

public sealed record CancelReservationCommandResult(
    bool NotFound, bool Forbidden, bool AlreadyCancelled, bool Success);

public sealed class CancelReservationCommandHandler(AppDbContext db)
    : IRequestHandler<CancelReservationCommand, CancelReservationCommandResult>
{
    public async ValueTask<CancelReservationCommandResult> Handle(
        CancelReservationCommand command, CancellationToken cancellationToken)
    {
        var reservation = await db.Reservations
            .Include(r => r.TimeSlot)
            .FirstOrDefaultAsync(r => r.Id == command.ReservationId, cancellationToken);

        if (reservation is null)
            return new(NotFound: true, false, false, false);

        if (reservation.UserId != command.UserId)
            return new(false, Forbidden: true, false, false);

        if (reservation.Status == "Cancelled")
            return new(false, false, AlreadyCancelled: true, false);

        reservation.Status = "Cancelled";
        reservation.TimeSlot.RemainingCapacity += reservation.PartySize;

        await db.SaveChangesAsync(cancellationToken);
        return new(false, false, false, Success: true);
    }
}
```

**Application handler for cancellation:**
```csharp
public sealed class CancelReservationRequestHandler(IMediator mediator)
    : IRequestHandler<CancelReservationRequest, Result<string>>
{
    public async ValueTask<Result<string>> Handle(
        CancelReservationRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CancelReservationCommand(request.UserId, request.ReservationId),
            cancellationToken);

        return result switch
        {
            { NotFound: true }       => Result<string>.Failure(404, "Reservation not found."),
            { Forbidden: true }      => Result<string>.Failure(403,
                "You are not authorized to cancel this reservation."),
            { AlreadyCancelled: true } => Result<string>.Failure(409,
                "Reservation is already cancelled."),
            { Success: true }        => Result<string>.Success("Reservation cancelled."),
            _ => Result<string>.Failure(500, "Unexpected error.")
        };
    }
}
```

**Updated `ReservationsEndpoints.cs` — add these two routes inside `MapReservationEndpoints`:**
```csharp
reservations.MapGet("/my", async (
    IMediator mediator, HttpContext context, CancellationToken ct) =>
{
    var userId = Guid.Parse(context.User.FindFirstValue("userId")!);
    var result = await mediator.Send(new GetMyReservationsRequest(userId), ct);
    return TypedResultHelper.ToResult(result);
});

reservations.MapDelete("/{id:guid}", async (
    Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
{
    var userId = Guid.Parse(context.User.FindFirstValue("userId")!);
    var result = await mediator.Send(new CancelReservationRequest(userId, id), ct);
    return TypedResultHelper.ToResult(result);
});
```

**API response shapes:**
```json
// GET /api/v1/reservations/my — 200
[
  {
    "reservationId": "uuid",
    "restaurantName": "Bella Notte",
    "date": "2026-06-20",
    "time": "19:30",
    "partySize": 4,
    "status": "Confirmed"
  }
]

// DELETE /api/v1/reservations/{id} — 200
{ "data": "Reservation cancelled." }

// 403
{ "errors": ["You are not authorized to cancel this reservation."] }

// 409 (already cancelled)
{ "errors": ["Reservation is already cancelled."] }
```

## Acceptance Criteria

- [ ] `GET /api/v1/reservations/my` returns all reservations for the JWT user (empty array if none)
- [ ] Response includes `reservationId`, `restaurantName`, `date`, `time`, `partySize`, `status`
- [ ] `DELETE /api/v1/reservations/{id}` by the owner returns 200 and sets status to `Cancelled`
- [ ] Cancellation restores `TimeSlot.RemainingCapacity` by `partySize` in the same transaction
- [ ] Cancelling another user's reservation returns 403
- [ ] Cancelling an already-cancelled reservation returns 409
- [ ] Both endpoints return 401 for unauthenticated requests (inherited from `.RequireAuthorization()`)

## Notes

- `reservation.UserId != command.UserId` performs the ownership check — this is the authorization logic. No separate policy is needed for the MVP.
- The capacity restore in `CancelReservationCommandHandler` must not use `AsNoTracking()` on the TimeSlot — it needs to be tracked for the `RemainingCapacity` update.
- BDD test: `namespace describe_cancel_reservation`, `class when_user_is_not_owner`, `method it_should_return_forbidden`.
- `GET /reservations/my` must use `.AsNoTracking()` — it is a read-only display query.
