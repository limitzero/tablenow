# Task 02: Create Reservation Command (Data Layer)

## Status

pending

## Wave

1

## Description

Creates the Data-layer command and handler for reservation creation. The handler opens a transaction, checks capacity, decrements `RemainingCapacity`, inserts the Reservation, and handles `DbUpdateConcurrencyException` by re-reading the slot. This is the core of the double-booking prevention.

## Dependencies

**Depends on:** None (Wave 1 — parallel with task-01, different project/files)
**Blocks:** task-03-reservation-endpoint.md, task-04-reservation-unit-tests.md

**Context from dependencies:** STORY-003 created `AppDbContext` with `DbSet<TimeSlot>` and `DbSet<Reservation>`. `TimeSlot.RowVersion` is configured as `IsRowVersion()` — EF will include it in UPDATE WHERE clauses automatically, causing `DbUpdateConcurrencyException` when two writers race. This task creates files in `Data/Reservations/Commands/` — different path from task-01's `Application/Reservations/`.

## Files to Create

- `server/src/Data/Reservations/Commands/CreateReservationCommand.cs`
- `server/src/Data/Reservations/Commands/CreateReservationCommandHandler.cs`
- `server/src/Contracts/Reservations/ReservationCreatedDto.cs`

## Technical Details

### Code Snippets

```csharp
// CreateReservationCommand.cs
namespace TableNow.Data.Reservations.Commands;

public record CreateReservationCommand(Guid UserId, Guid SlotId, int PartySize)
    : IRequest<Result<ReservationCreatedDto>>;
```

```csharp
// ReservationCreatedDto.cs (in Contracts/Reservations/)
namespace TableNow.Contracts.Reservations;

public record ReservationCreatedDto(
    Guid ReservationId, Guid SlotId, string RestaurantName,
    DateTimeOffset DateTime, int PartySize, string Status);
```

```csharp
// CreateReservationCommandHandler.cs
namespace TableNow.Data.Reservations.Commands;

public class CreateReservationCommandHandler(AppDbContext db)
    : IRequestHandler<CreateReservationCommand, Result<ReservationCreatedDto>>
{
    private const string SlotUnavailable = "This time slot is no longer available.";

    public async ValueTask<Result<ReservationCreatedDto>> Handle(
        CreateReservationCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var slot = await db.TimeSlots
                .Include(ts => ts.Restaurant)
                .FirstOrDefaultAsync(ts => ts.Id == command.SlotId, cancellationToken);

            if (slot is null)
                return Result<ReservationCreatedDto>.Failure("Slot not found.", 404);

            if (slot.RemainingCapacity < command.PartySize)
                return Result<ReservationCreatedDto>.Failure(SlotUnavailable, 409);

            slot.RemainingCapacity -= command.PartySize;

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                SlotId = command.SlotId,
                PartySize = command.PartySize,
                Status = "Confirmed",
                CreatedAt = DateTimeOffset.UtcNow,
            };
            db.Reservations.Add(reservation);

            await db.SaveChangesAsync(cancellationToken); // may throw DbUpdateConcurrencyException
            await transaction.CommitAsync(cancellationToken);

            return Result<ReservationCreatedDto>.Success(new ReservationCreatedDto(
                reservation.Id, slot.Id, slot.Restaurant.Name,
                slot.DateTime, command.PartySize, "Confirmed"));
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);

            // Re-read slot to check if it's still insufficient
            var current = await db.TimeSlots.AsNoTracking()
                .FirstOrDefaultAsync(ts => ts.Id == command.SlotId, cancellationToken);

            if (current is null || current.RemainingCapacity < command.PartySize)
                return Result<ReservationCreatedDto>.Failure(SlotUnavailable, 409);

            // Slot has capacity now (rare race — caller may retry)
            return Result<ReservationCreatedDto>.Failure(SlotUnavailable, 409);
        }
    }
}
```

## Acceptance Criteria

- [ ] Command and handler exist in `Data/Reservations/Commands/`
- [ ] Handler uses a DB transaction wrapping capacity check + decrement + insert
- [ ] `DbUpdateConcurrencyException` caught and returns 409
- [ ] `RemainingCapacity` decremented by `PartySize` before `SaveChangesAsync`
- [ ] `ReservationCreatedDto` contract record exists in `Contracts/Reservations/`

## Notes

The `RowVersion` concurrency token means EF Core generates `UPDATE TimeSlots SET RemainingCapacity=? WHERE Id=? AND RowVersion=<original>`. If another transaction already updated the row (changing `RowVersion`), this UPDATE affects 0 rows and EF throws `DbUpdateConcurrencyException`. This guarantees exactly-once capacity decrement under concurrent load.
