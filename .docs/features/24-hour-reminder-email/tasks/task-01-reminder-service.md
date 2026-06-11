# Task 01: BackgroundService & Query Logic

## Status

pending

## Wave

1

## Description

Creates a `ReminderBackgroundService` extending `BackgroundService` that runs every 15 minutes. Queries confirmed reservations in the 23–25h window. Adds a `ReminderSentAt` column to `Reservation` to track dedup.

## Dependencies

**Depends on:** STORY-021 task-02-handler-integration.md (IEmailService must be available)
**Blocks:** task-02-reminder-template.md

**Context from dependencies:** `IEmailService` registered in DI. `Reservation` entity has `Status`, `SlotDateTime` (via join with `TimeSlot`). Dedup requires a new nullable `ReminderSentAt (DateTime?)` on Reservation.

## Files to Create

- `server/src/Api/Services/ReminderBackgroundService.cs`

## Files to Modify

- `server/src/Domain/CM.TableNow.Reservations.Domain/Reservation.cs` — Add `ReminderSentAt (DateTime?)`
- `server/src/Data/CM.TableNow.Reservations.Data/Configurations/ReservationConfiguration.cs` — Map new column
- Add a new EF migration: `dotnet ef migrations add AddReminderSentAt ...`

## Technical Details

### Code Snippets

```csharp
// ReminderBackgroundService.cs
public class ReminderBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<ReminderBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await SendRemindersAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    private async Task SendRemindersAsync(CancellationToken ct)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ReservationsDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var now = DateTime.UtcNow;
        var windowStart = now.AddHours(23);
        var windowEnd = now.AddHours(25);

        var dueReservations = await db.Reservations
            .Where(r =>
                r.Status == "Confirmed" &&
                r.ReminderSentAt == null)
            .Join(db.TimeSlots,
                r => r.TimeSlotId, s => s.Id,
                (r, s) => new { Reservation = r, Slot = s })
            .Where(x => x.Slot.SlotDateTime >= windowStart && x.Slot.SlotDateTime <= windowEnd)
            .ToListAsync(ct);

        foreach (var item in dueReservations)
        {
            try
            {
                // Build and send reminder (email builder created in task-02)
                // await emailService.SendAsync(...);
                item.Reservation.ReminderSentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send reminder for reservation {Id}", item.Reservation.Id);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddHostedService<ReminderBackgroundService>();
```

## Acceptance Criteria

- [ ] `ReminderBackgroundService` runs every 15 minutes
- [ ] Queries reservations in 23–25h window with `ReminderSentAt == null`
- [ ] Sets `ReminderSentAt` after sending to prevent duplicate sends
- [ ] `dotnet build` exits with code 0
