# Task 02: Seed Time Slots

## Status

pending

## Wave

2

## Description

Generate time slot records for every seeded restaurant covering the next 30 days, with multiple slot times per day. Each slot is initialized with a realistic `TotalCapacity` and matching `RemainingCapacity`. At least one slot must have `RemainingCapacity = 0` so the slot availability endpoint (STORY-011) can be tested with a slot that should be excluded from results. The seed is idempotent — if time slots already exist it skips insertion.

## Dependencies

**Depends on:** task-01-seed-users-restaurants.md
**Blocks:** None

**Context from dependencies:** task-01 creates 15+ restaurant records using the stable GUIDs defined in `RestaurantDataSeeder.RestaurantIds`. This task loops over those IDs to generate the per-restaurant time slots. The `RestaurantsDbContext` contains a `TimeSlots` `DbSet<TimeSlot>` with columns: `Id`, `RestaurantId`, `StartTime` (DateTimeOffset UTC), `TotalCapacity`, `RemainingCapacity`, `RowVersion`.

## Files to Create

- `server/src/Data/Restaurants/Seed/TimeSlotDataSeeder.cs` — Generates and inserts time slot records; idempotent.

## Files to Modify

- `server/src/Api/Extensions/ServiceCollectionExtensions.cs` (or `Program.cs` startup call) — Call `TimeSlotDataSeeder.SeedAsync` after `RestaurantDataSeeder.SeedAsync`.

## Technical Details

### Implementation Steps

1. In `TimeSlotDataSeeder.SeedAsync(RestaurantsDbContext db)`, check `db.TimeSlots.AnyAsync()` — return early if slots already exist.
2. Define the daily slot times as `TimeSpan[]`: `[12:00, 13:00, 18:00, 19:00, 20:00]` UTC.
3. Define capacities per slot index: e.g., lunch slots (index 0, 1) → capacity 20; dinner slots (index 2, 3, 4) → capacity 30.
4. Loop `date` from `DateTimeOffset.UtcNow.Date` to `+ 30 days` (inclusive).
5. For each `date`, loop over `RestaurantDataSeeder.RestaurantIds`, then over slot times, creating a `TimeSlot` with a `Guid.NewGuid()` ID.
6. After generating all slots, set `RemainingCapacity = 0` on one specific slot (e.g., the first lunch slot of the first restaurant on the current date) to create the test zero-capacity scenario.
7. `db.TimeSlots.AddRange(allSlots)` and `await db.SaveChangesAsync(ct)`.

### Code Snippets

```csharp
namespace CM.TableNow.Restaurants.Data.Seed;

public static class TimeSlotDataSeeder
{
    private static readonly TimeSpan[] SlotTimes = [
        TimeSpan.FromHours(12),
        TimeSpan.FromHours(13),
        TimeSpan.FromHours(18),
        TimeSpan.FromHours(19),
        TimeSpan.FromHours(20),
    ];

    private static readonly int[] Capacities = [20, 20, 30, 30, 30];

    public static async Task SeedAsync(RestaurantsDbContext db, CancellationToken ct = default)
    {
        if (await db.TimeSlots.AnyAsync(ct)) return;

        var slots = new List<TimeSlot>();
        var today = DateTimeOffset.UtcNow.Date;

        foreach (var restaurantId in RestaurantDataSeeder.RestaurantIds)
        {
            for (int d = 0; d < 30; d++)
            {
                var date = today.AddDays(d);
                for (int t = 0; t < SlotTimes.Length; t++)
                {
                    slots.Add(new TimeSlot
                    {
                        Id = Guid.NewGuid(),
                        RestaurantId = restaurantId,
                        StartTime = new DateTimeOffset(date + SlotTimes[t], TimeSpan.Zero),
                        TotalCapacity = Capacities[t],
                        RemainingCapacity = Capacities[t],
                        RowVersion = [],
                    });
                }
            }
        }

        // Seed one zero-capacity slot for testing the availability filter
        slots[0] = slots[0] with { RemainingCapacity = 0 };

        await db.TimeSlots.AddRangeAsync(slots, ct);
        await db.SaveChangesAsync(ct);
    }
}
```

## Acceptance Criteria

- [ ] Each seeded restaurant has time slots for the next 30 days with 5 slots per day.
- [ ] Slot times are 12:00, 13:00, 18:00, 19:00, 20:00 UTC.
- [ ] At least one slot has `RemainingCapacity = 0`.
- [ ] All slots have `RemainingCapacity == TotalCapacity` except the zero-capacity test slot.
- [ ] Running the seeder twice does not create duplicate time slots.
- [ ] The total slot count equals `(number of restaurants) × 30 × 5` minus the one zeroed slot.

## Notes

- `slots[0] with { RemainingCapacity = 0 }` uses a positional record `with` expression — this only works if `TimeSlot` is a `record`. If it's a `class`, assign the property directly instead.
- The `RowVersion` initialized to `[]` (empty byte array) is intentional — EF Core will manage the row version on first save (SQL Server) or via a manual update hook (SQLite).
- If the number of restaurants changes from task-01, the total slot count changes proportionally — that is expected and acceptable.
