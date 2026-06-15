using CM.TableNow.Restaurants.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Restaurants.Data.Seed;

/// <summary>
/// Generates reservable time slots for every seeded restaurant across the next 30 days,
/// with five slots per day (lunch and dinner). Idempotent: skips seeding if any slot exists.
/// </summary>
/// <remarks>
/// Slots are keyed to the stable IDs in <see cref="RestaurantDataSeeder.RestaurantIds"/>.
/// The first generated slot is intentionally left with zero remaining capacity so the slot
/// availability endpoint (STORY-011) can be tested against a slot that must be excluded.
/// </remarks>
public static class TimeSlotDataSeeder
{
    // Daily slot start times (UTC): two lunch sittings followed by three dinner sittings.
    private static readonly TimeSpan[] SlotTimes =
    [
        TimeSpan.FromHours(12),
        TimeSpan.FromHours(13),
        TimeSpan.FromHours(18),
        TimeSpan.FromHours(19),
        TimeSpan.FromHours(20),
    ];

    // Per-slot capacities aligned by index with SlotTimes: lunch seats 20, dinner seats 30.
    private static readonly int[] Capacities = [20, 20, 30, 30, 30];

    // Number of days of slots to generate, starting from today (inclusive).
    private const int DaysToSeed = 30;

    public static async Task SeedAsync(RestaurantsDbContext db, CancellationToken ct = default)
    {
        // Idempotency guard: if any slot exists the seed has already run, so do nothing.
        if (await db.TimeSlots.AnyAsync(ct))
        {
            return;
        }

        var slots = new List<TimeSlot>();
        var today = DateTime.UtcNow.Date;

        foreach (var restaurantId in RestaurantDataSeeder.RestaurantIds)
        {
            for (var dayOffset = 0; dayOffset < DaysToSeed; dayOffset++)
            {
                var date = today.AddDays(dayOffset);

                for (var slotIndex = 0; slotIndex < SlotTimes.Length; slotIndex++)
                {
                    slots.Add(new TimeSlot
                    {
                        Id = Guid.NewGuid(),
                        RestaurantId = restaurantId,
                        StartTime = new DateTimeOffset(date + SlotTimes[slotIndex], TimeSpan.Zero),
                        TotalCapacity = Capacities[slotIndex],
                        RemainingCapacity = Capacities[slotIndex],
                    });
                }
            }
        }

        // Force a single fully-booked slot so STORY-011 can verify it is excluded from results.
        // TimeSlot is a class, so this mutates the existing instance in place.
        slots[0].RemainingCapacity = 0;

        await db.TimeSlots.AddRangeAsync(slots, ct);
        await db.SaveChangesAsync(ct);
    }
}
