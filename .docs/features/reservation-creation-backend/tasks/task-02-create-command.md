# Task 02: CreateReservation Data Command

## Status

pending

## Wave

2

## Description

Implements `CreateReservationCommand` / `CreateReservationCommandHandler` in `Data/Reservations/Commands/`. The handler reads the `TimeSlot` with tracking, checks capacity, inserts the `Reservation`, decrements capacity, and saves — all in one `SaveChangesAsync` call. Returns `Result<CreateReservationResult>` to surface 409 at the command level.

## Dependencies

**Depends on:** task-01-reservation-entity.md
**Blocks:** task-03-app-handler.md

**Context from dependencies:** task-01 added `DbSet<TimeSlot>` to `ReservationsDbContext` with RowVersion concurrency token. `TimeSlot`: `Id`, `RemainingCapacity`, `RowVersion`. `Reservation`: `Id, UserId, TimeSlotId, PartySize, Status`.

## Files to Create

- `server/src/Data/CM.TableNow.Reservations.Data/Commands/CreateReservation/CreateReservationCommand.cs`
- `server/src/Data/CM.TableNow.Reservations.Data/Commands/CreateReservation/CreateReservationCommandHandler.cs`

## Technical Details

### Code Snippets

```csharp
// CreateReservationCommand.cs
using CM.TableNow.Shared;

namespace CM.TableNow.Reservations.Data.Commands.CreateReservation;

public record CreateReservationCommand(Guid UserId, Guid TimeSlotId, int PartySize)
    : IRequest<Result<Guid>>;
```

```csharp
// CreateReservationCommandHandler.cs
using CM.TableNow.Reservations.Domain;
using CM.TableNow.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Reservations.Data.Commands.CreateReservation;

public class CreateReservationCommandHandler(ReservationsDbContext db)
    : IRequestHandler<CreateReservationCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(
        CreateReservationCommand command,
        CancellationToken ct)
    {
        // Load slot WITH tracking so EF can detect concurrency conflicts
        var slot = await db.TimeSlots.FindAsync([command.TimeSlotId], ct);
        if (slot is null)
            return ResultExtensions.NotFound<Guid>("Time slot not found.");

        if (slot.RemainingCapacity < command.PartySize)
            return ResultExtensions.Conflict<Guid>("This time slot is no longer available.");

        slot.RemainingCapacity -= command.PartySize;

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            TimeSlotId = command.TimeSlotId,
            PartySize = command.PartySize,
            Status = "Confirmed",
            CreatedAt = DateTime.UtcNow,
        };

        db.Reservations.Add(reservation);

        try
        {
            await db.SaveChangesAsync(ct);
            return ResultExtensions.Created(reservation.Id);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Concurrency conflict — reload the slot and re-check
            await db.Entry(slot).ReloadAsync(ct);
            if (slot.RemainingCapacity < command.PartySize)
                return ResultExtensions.Conflict<Guid>("This time slot is no longer available.");

            // Retry once after reload
            slot.RemainingCapacity -= command.PartySize;
            db.Reservations.Add(reservation);
            await db.SaveChangesAsync(ct);
            return ResultExtensions.Created(reservation.Id);
        }
    }
}
```

## Acceptance Criteria

- [ ] Command handler atomically decrements `RemainingCapacity` and inserts `Reservation`
- [ ] `DbUpdateConcurrencyException` is caught; slot is reloaded and re-checked
- [ ] Returns 409 when `RemainingCapacity < partySize` (before or after retry)
- [ ] Returns 201 (via `Created`) with new reservation `Guid` on success
- [ ] `dotnet build` exits with code 0
