# Implementation Plan: Reservation Creation — Backend

## Phase 1 — Data and Application layers (parallel)

task-01 and task-02 are in different projects and can be implemented simultaneously.

- [ ] **task-01-create-reservation-command** — `CreateReservationCommand(UserId, SlotId, PartySize)` + handler that re-reads the slot for capacity, creates the `Reservation`, decrements `RemainingCapacity`, saves atomically, and handles `DbUpdateConcurrencyException` with a re-read + 409 on insufficient capacity.
- [ ] **task-02-create-reservation-application** — `CreateReservationRequest(SlotId, PartySize)` + handler that extracts `userId` from context and dispatches `CreateReservationCommand`.

### Technical Details

**Optimistic concurrency flow:**
```csharp
try
{
    await db.SaveChangesAsync(ct);
}
catch (DbUpdateConcurrencyException)
{
    await db.Entry(slot).ReloadAsync(ct);
    if (slot.RemainingCapacity < command.PartySize)
        return Result<...>.Failure(409, "This time slot is no longer available.");
    await db.SaveChangesAsync(ct);
}
```

**`userId` extraction** at the endpoint:
```csharp
var userId = Guid.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
```

## Phase 2 — Endpoint and BDD tests

- [ ] **task-03-reservation-endpoint** — `POST /api/reservations` with JWT auth, `userId` extraction from `ClaimsPrincipal`, `ReservationsMapper`, and BDD tests: `describe_create_reservation` / `when_slot_is_fully_booked` / `it_should_return_conflict_status`.
