# Task 01: CancelReservation Data Command

## Status

pending

## Wave

1

## Description

Implement the Data-layer command that atomically cancels a reservation and restores the associated time slot's capacity. The handler performs the ownership check (returns 403 if the requesting user is not the owner), the double-cancel guard (returns 409 if already cancelled), and then in a single `SaveChangesAsync` call updates `Reservation.Status = Cancelled` and increments `TimeSlot.RemainingCapacity` by `PartySize`.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-03-cancel-endpoint.md

**Context from dependencies:** Assumes STORY-016 created `ReservationsDbContext` with a `Reservations` DbSet and that `RestaurantsDbContext` holds `TimeSlots`. The `Reservation` entity has `Id`, `UserId`, `TimeSlotId`, `PartySize`, `Status` (enum: `Confirmed`/`Cancelled`).

## Files to Create

- `server/src/Data/Reservations/Commands/CancelReservation/CancelReservationCommand.cs` — Command record.
- `server/src/Data/Reservations/Commands/CancelReservation/CancelReservationCommandHandler.cs` — Handler.

## Files to Modify

- None.

## Technical Details

### Implementation Steps

1. Define `CancelReservationCommand(Guid ReservationId, Guid RequestingUserId)` implementing `ICommand<Result<Unit>>`.
2. Handler primary constructor: `(ReservationsDbContext reservationsDb, RestaurantsDbContext restaurantsDb)`.
3. Load reservation: `await reservationsDb.Reservations.FindAsync([command.ReservationId], ct)` — return 404 if null.
4. Ownership: `if (reservation.UserId != command.RequestingUserId) return Result<Unit>.Failure(403, "Forbidden")`.
5. Already cancelled: `if (reservation.Status == ReservationStatus.Cancelled) return Result<Unit>.Failure(409, "Reservation is already cancelled")`.
6. Load slot: `await restaurantsDb.TimeSlots.FindAsync([reservation.TimeSlotId], ct)` — restore capacity.
7. `reservation.Status = ReservationStatus.Cancelled; slot!.RemainingCapacity += reservation.PartySize`.
8. `await reservationsDb.SaveChangesAsync(ct); await restaurantsDb.SaveChangesAsync(ct)` — or wrap both in a `TransactionScope`.
9. Return `Result<Unit>.Success(Unit.Value)`.

### Code Snippets

```csharp
using CM.TableNow.Reservations.Domain.Enums;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Reservations.Data.Commands.CancelReservation;

public sealed record CancelReservationCommand(
    Guid ReservationId,
    Guid RequestingUserId) : ICommand<Result<Unit>>;
```

## Acceptance Criteria

- [ ] Returns 404 when the reservation does not exist.
- [ ] Returns 403 when `RequestingUserId != reservation.UserId`.
- [ ] Returns 409 when the reservation is already cancelled.
- [ ] Sets `Reservation.Status = Cancelled` on success.
- [ ] Restores `TimeSlot.RemainingCapacity += reservation.PartySize` on success.
- [ ] Both changes are saved atomically.

## Notes

- If the two DbContexts share the same underlying database connection, wrap saves in a `using var transaction = await reservationsDb.Database.BeginTransactionAsync(ct)` and commit once both updates are done.
