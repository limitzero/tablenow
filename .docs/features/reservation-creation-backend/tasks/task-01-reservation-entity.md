# Task 01: Reservation Entity & TimeSlot Concurrency Config

## Status

pending

## Wave

1

## Description

Ensures the `Reservation` domain entity exists and that `TimeSlot` has a `RowVersion` byte array property configured as a concurrency token. The `ReservationsDbContext` must include both `Reservations` and (read-only) access to `TimeSlots` for the concurrency check. This task is the foundation for the double-booking prevention mechanism.

## Dependencies

**Depends on:** STORY-003 task-01-domain-entities.md (Reservation entity), STORY-003 task-02-ef-models-configs.md (TimeSlot EF config)
**Blocks:** task-02-create-command.md

**Context from dependencies:**
- STORY-003 task-01 created `Reservation` entity: `Id, UserId, TimeSlotId, PartySize, Status, CreatedAt`
- STORY-003 task-01 created `TimeSlot` with `RowVersion (byte[])` property
- STORY-003 task-02 configured `TimeSlotConfiguration` with `.IsRowVersion()` on `RowVersion`
- `ReservationsDbContext` was created in STORY-003 task-02 with only `DbSet<Reservation>`

## Files to Modify

- `server/src/Data/CM.TableNow.Reservations.Data/ReservationsDbContext.cs` — Add `DbSet<TimeSlot>` for concurrency checks

## Files to Create

None (entities and EF config already exist from STORY-003).

## Technical Details

### Implementation Steps

The `ReservationsDbContext` needs access to `TimeSlots` to read and check/decrement capacity within the same DbContext transaction. Since `TimeSlot` lives in the Restaurants context, we have a choice:

**Approach (cross-context, single DB):** Add `TimeSlot` as a read/write entity in `ReservationsDbContext`. This is acceptable because both contexts share the same physical database in this monolith, and atomic capacity decrement requires updating `TimeSlot` in the same transaction as creating the `Reservation`.

Add to `ReservationsDbContext.cs`:
```csharp
using CM.TableNow.Restaurants.Domain; // Add project reference if needed

public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
```

If `ReservationsDbContext` doesn't already reference `CM.TableNow.Restaurants.Domain`, add a project reference:
```powershell
dotnet add server/src/Data/CM.TableNow.Reservations.Data reference server/src/Domain/CM.TableNow.Restaurants.Domain
```

Also add a minimal EF config for `TimeSlot` in the Reservations context (only the concurrency token — no full mapping):
```csharp
// server/src/Data/CM.TableNow.Reservations.Data/Configurations/TimeSlotConcurrencyConfig.cs
public class TimeSlotConcurrencyConfig : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> builder)
    {
        builder.ToTable("TimeSlots"); // must match the Restaurants context table name
        builder.HasKey(t => t.Id);
        builder.Property(t => t.RowVersion).IsRowVersion();
    }
}
```

## Acceptance Criteria

- [ ] `ReservationsDbContext` has `DbSet<TimeSlot> TimeSlots`
- [ ] `TimeSlot.RowVersion` is configured as a row version (concurrency token) in the Reservations context
- [ ] `dotnet build` exits with code 0

## Notes

This cross-context entity sharing is a pragmatic choice in a shared-database monolith. The alternative — dispatching a Mediator query to the Restaurants context and then hoping the data hasn't changed — cannot be atomic. Use the shared-DB approach here.
