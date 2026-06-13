# Task 01: CreateReservation Data Command

## Status

pending

## Wave

1

## Description

Implement the Data-layer CQRS command that atomically creates a `Reservation` record and decrements `TimeSlot.RemainingCapacity` in a single EF Core transaction. Uses EF Core optimistic concurrency on `TimeSlot.RowVersion` to prevent double-booking: on `DbUpdateConcurrencyException`, the slot is reloaded and if capacity is still insufficient a 409 failure is returned; otherwise the save is retried. This is the critical concurrency safety mechanism for the entire reservation feature.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-03-reservation-endpoint.md

**Context from dependencies:** Assumes STORY-003 created the `TimeSlot` EF model with `RowVersion` (`byte[]`) configured as `.IsRowVersion()` (SQL Server) / `.IsConcurrencyToken()` (SQLite), and the `Reservation` EF model with `Id`, `UserId`, `TimeSlotId`, `PartySize`, `Status`, `CreatedAt`. The `ReservationsDbContext` has `Reservations` and needs access to `TimeSlots` (either via a cross-context read or a shared DbContext). Check the project structure — if `TimeSlots` lives in `RestaurantsDbContext`, use a transaction across contexts or add a read from `RestaurantsDbContext` injected into the command handler.

## Files to Create

- `server/src/Data/Reservations/Commands/CreateReservation/CreateReservationCommand.cs` — Command record.
- `server/src/Data/Reservations/Commands/CreateReservation/CreateReservationCommandHandler.cs` — Handler with optimistic concurrency logic.

## Files to Modify

- None.

## Technical Details

### Implementation Steps

1. Define `CreateReservationCommand(Guid UserId, Guid SlotId, int PartySize)` implementing `ICommand<Result<ReservationData>>`.
2. Define `ReservationData(Guid ReservationId, Guid SlotId, int PartySize, string Status)` as a result record.
3. Implement handler with primary constructor injecting both `ReservationsDbContext` and `RestaurantsDbContext` (to access `TimeSlots`).
4. Fetch the time slot: `var slot = await restaurantsDb.TimeSlots.FirstOrDefaultAsync(s => s.Id == command.SlotId, ct)` — return 404 if not found.
5. Check capacity: if `slot.RemainingCapacity < command.PartySize` return `Result.Failure(409, "This time slot is no longer available.")`.
6. Decrement: `slot.RemainingCapacity -= command.PartySize`.
7. Create reservation: `var reservation = new Reservation { Id = Guid.NewGuid(), UserId = command.UserId, TimeSlotId = command.SlotId, PartySize = command.PartySize, Status = ReservationStatus.Confirmed, CreatedAt = DateTimeOffset.UtcNow }`.
8. Add reservation and save — wrap in try/catch for `DbUpdateConcurrencyException`:
   ```csharp
   catch (DbUpdateConcurrencyException)
   {
       await restaurantsDb.Entry(slot).ReloadAsync(ct);
       if (slot.RemainingCapacity < command.PartySize)
           return Result<ReservationData>.Failure(409, "This time slot is no longer available.");
       slot.RemainingCapacity -= command.PartySize;
       await restaurantsDb.SaveChangesAsync(ct);
   }
   ```
9. Return `Result<ReservationData>.Success(new ReservationData(...))`.

### Code Snippets

```csharp
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Reservations.Data.Commands.CreateReservation;

public sealed record CreateReservationCommand(
    Guid UserId,
    Guid SlotId,
    int PartySize) : ICommand<Result<ReservationData>>;

public sealed record ReservationData(
    Guid ReservationId,
    Guid SlotId,
    int PartySize,
    string Status);
```

## Acceptance Criteria

- [ ] Handler creates a `Reservation` record and decrements `TimeSlot.RemainingCapacity` in one atomic operation.
- [ ] Returns 409 when `RemainingCapacity < partySize` before attempting save.
- [ ] Returns 409 after a `DbUpdateConcurrencyException` when the reloaded slot still has insufficient capacity.
- [ ] Two concurrent calls for the last capacity unit result in exactly one success and one 409.
- [ ] Returns 404 when the `SlotId` does not exist.

## Notes

- The cross-context access (reading `TimeSlots` from `RestaurantsDbContext` in the Reservations command) is acceptable in a modular monolith where both DbContexts are in-process. Use a shared `IDbContextTransaction` or a `TransactionScope` if both need to participate in the same transaction.
- If the architecture evolves to use separate database schemas per context, this command will need a refactor — document that decision in the migration notes.
