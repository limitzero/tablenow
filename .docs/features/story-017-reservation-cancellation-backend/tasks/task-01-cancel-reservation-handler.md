# Task 01: Cancel Reservation Handler

## Status

pending

## Wave

1

## Description

Creates `CancelReservationCommand` and its handler. The handler loads the reservation, checks ownership, checks current status, then atomically sets status to "Cancelled" and restores slot capacity.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-003 AppDbContext, STORY-014 reservation rows)
**Blocks:** task-02-cancel-endpoint.md

**Context from dependencies:** STORY-003 created `AppDbContext` with `DbSet<Reservation>` and `DbSet<TimeSlot>`. STORY-014 created the reservation creation flow. `CancelReservationCommand` is a new command in `Data/Reservations/Commands/`.

## Files to Create

- `server/src/Data/Reservations/Commands/CancelReservationCommand.cs`
- `server/src/Data/Reservations/Commands/CancelReservationCommandHandler.cs`
- `server/src/Application/Reservations/Features/CancelReservation/CancelReservationRequest.cs`
- `server/src/Application/Reservations/Features/CancelReservation/CancelReservationRequestHandler.cs`

## Technical Details

### Code Snippets

```csharp
// CancelReservationCommand.cs
namespace TableNow.Data.Reservations.Commands;
public record CancelReservationCommand(Guid ReservationId, Guid RequestingUserId)
    : IRequest<Result<bool>>;

// CancelReservationCommandHandler.cs
public class CancelReservationCommandHandler(AppDbContext db)
    : IRequestHandler<CancelReservationCommand, Result<bool>>
{
    public async ValueTask<Result<bool>> Handle(
        CancelReservationCommand command, CancellationToken ct)
    {
        var reservation = await db.Reservations.FindAsync([command.ReservationId], ct);
        if (reservation is null)
            return Result<bool>.Failure("Reservation not found.", 404);

        if (reservation.UserId != command.RequestingUserId)
            return Result<bool>.Failure("Forbidden.", 403);

        if (reservation.Status == "Cancelled")
            return Result<bool>.Failure("Reservation is already cancelled.", 409);

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        reservation.Status = "Cancelled";

        var slot = await db.TimeSlots.FindAsync([reservation.SlotId], ct);
        if (slot is not null)
            slot.RemainingCapacity += reservation.PartySize;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Result<bool>.Success(true);
    }
}
```

```csharp
// CancelReservationRequest.cs (Application layer)
namespace TableNow.Application.Reservations.Features.CancelReservation;
public record CancelReservationRequest(Guid ReservationId) : IRequest<Result<bool>>;

// CancelReservationRequestHandler.cs reads userId from IHttpContextAccessor
// and sends CancelReservationCommand(ReservationId, userId)
```

## Acceptance Criteria

- [ ] Returns 404 if reservation not found
- [ ] Returns 403 if requesting user is not the owner
- [ ] Returns 409 if already cancelled
- [ ] Sets status to "Cancelled" and restores slot capacity in one transaction
- [ ] Application handler reads userId from JWT claims via IHttpContextAccessor
