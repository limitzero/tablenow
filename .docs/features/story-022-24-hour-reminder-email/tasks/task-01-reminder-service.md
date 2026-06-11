# Task 01: Reminder Background Service

## Status

pending

## Wave

1

## Description

Creates `ReminderBackgroundService` that queries upcoming reservations in the 24-hour window and sends reminder emails. Also adds the `ReminderSent` column to the `Reservation` entity (and requires a new EF migration).

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-020 IEmailService, STORY-003 Reservation entity)
**Blocks:** Nothing directly (task-02 is parallel)

**Context from dependencies:** STORY-020 created `IEmailService`. STORY-003 created `Reservation` entity in `Domain/Reservations/Reservation.cs` — the `ReminderSent bool` property was stubbed there. If it wasn't added in STORY-003, add it now and create a new migration. `AppDbContext` joins Reservation → TimeSlot → User.

## Files to Modify

- `server/src/Domain/Reservations/Reservation.cs` — ensure `ReminderSent bool` property exists

## Files to Create

- `server/src/Infrastructure/Notifications/ReminderBackgroundService.cs`
- New EF migration `AddReminderSentToReservation` (run via CLI after)

## Technical Details

### Code Snippets

```csharp
// ReminderBackgroundService.cs
namespace TableNow.Infrastructure.Notifications;

public class ReminderBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReminderBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessRemindersAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    private async Task ProcessRemindersAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var now = DateTimeOffset.UtcNow;
        var windowStart = now.AddHours(23);
        var windowEnd = now.AddHours(25);

        var upcoming = await db.Reservations
            .Where(r => r.Status == "Confirmed"
                     && !r.ReminderSent
                     && r.TimeSlot.DateTime >= windowStart
                     && r.TimeSlot.DateTime <= windowEnd)
            .Include(r => r.TimeSlot).ThenInclude(ts => ts.Restaurant)
            .Join(db.Users, r => r.UserId, u => u.Id, (r, u) => new { r, u })
            .ToListAsync(ct);

        foreach (var item in upcoming)
        {
            try
            {
                // Email sent using Reminder template (created in task-02)
                await emailService.SendAsync(
                    item.u.Email,
                    $"Reminder: Your reservation at {item.r.TimeSlot.Restaurant.Name} is tomorrow",
                    $"<p>This is a reminder for your reservation at <strong>{item.r.TimeSlot.Restaurant.Name}</strong> on {item.r.TimeSlot.DateTime:f}. Party of {item.r.PartySize}.</p>",
                    cancellationToken: ct);

                item.r.ReminderSent = true;
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send reminder for reservation {Id}", item.r.Id);
                // Do NOT set ReminderSent=true — will retry next cycle
            }
        }
    }
}
```

**EF Migration needed:** After adding `ReminderSent` to `Reservation`, run:
```powershell
dotnet ef migrations add AddReminderSentToReservation --project server/src/Migrations --startup-project server/src/Migrations
dotnet ef database update --project server/src/Migrations --startup-project server/src/Migrations
```

## Acceptance Criteria

- [ ] `ReminderBackgroundService` extends `BackgroundService`
- [ ] Runs every 15 minutes via `Task.Delay`
- [ ] Queries reservations WHERE Status=Confirmed AND ReminderSent=false AND DateTime in [now+23h, now+25h]
- [ ] Sets `ReminderSent=true` after successful send
- [ ] Failed sends logged but NOT marked as sent
- [ ] Uses `IServiceScopeFactory` (not direct DbContext injection)
