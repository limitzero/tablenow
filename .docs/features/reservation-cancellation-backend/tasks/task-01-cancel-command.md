# Task 01: CancelReservation Data Command

## Status

pending

## Wave

1

## Description

Implements `CancelReservationCommand` / `CancelReservationCommandHandler`. Within a single transaction: sets `Reservation.Status = "Cancelled"` and increments `TimeSlot.RemainingCapacity` by `PartySize`. Returns status code results: 200 success, 404 not found, 409 already cancelled.

## Dependencies

**Depends on:** STORY-016 task-02-my-reservations-endpoint.md (Reservation table populated)
**Blocks:** task-02-cancel-endpoint.md

**Context from dependencies:** `ReservationsDbContext` has `DbSet<Reservation>` and `DbSet<TimeSlot>`. `Reservation`: Id, UserId, TimeSlotId, PartySize, Status.

## Files to Create

- `server/src/Data/CM.TableNow.Reservations.Data/Commands/CancelReservation/CancelReservationCommand.cs`
- `server/src/Data/CM.TableNow.Reservations.Data/Commands/CancelReservation/CancelReservationCommandHandler.cs`

## Technical Details

### Code Snippets

```csharp
// CancelReservationCommand.cs
public record CancelReservationCommand(Guid ReservationId) : IRequest<Result<CancelledReservationResult>>;
public record CancelledReservationResult(Guid ReservationId, Guid OwnerId, int PartySize);
```

```csharp
// CancelReservationCommandHandler.cs
public class CancelReservationCommandHandler(ReservationsDbContext db)
    : IRequestHandler<CancelReservationCommand, Result<CancelledReservationResult>>
{
    public async ValueTask<Result<CancelledReservationResult>> Handle(
        CancelReservationCommand command,
        CancellationToken ct)
    {
        var reservation = await db.Reservations.FindAsync([command.ReservationId], ct);
        if (reservation is null)
            return ResultExtensions.NotFound<CancelledReservationResult>();

        if (reservation.Status == "Cancelled")
            return ResultExtensions.Conflict<CancelledReservationResult>("Reservation is already cancelled.");

        // Restore slot capacity
        var slot = await db.TimeSlots.FindAsync([reservation.TimeSlotId], ct);
        if (slot is not null)
            slot.RemainingCapacity += reservation.PartySize;

        reservation.Status = "Cancelled";
        await db.SaveChangesAsync(ct);

        return ResultExtensions.Ok(new CancelledReservationResult(
            reservation.Id, reservation.UserId, reservation.PartySize));
    }
}
```

## Acceptance Criteria

- [ ] Command sets `Status = "Cancelled"` and restores slot capacity atomically
- [ ] Returns 409 for already-cancelled reservation
- [ ] Returns 404 for unknown reservation
- [ ] Returns 200 with result on success
- [ ] `dotnet build` exits with code 0
