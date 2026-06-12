# Task 02: Reminder Background Service

## Status

pending

## Wave

1

## Description

Implement a scheduled `BackgroundService` that sends 24-hour reminder emails. Every 15 minutes it queries confirmed reservations whose slot time falls within a window around 24 hours from now and that have not yet been reminded, sends each diner a reminder via `IEmailService`, and stamps `ReminderSentAt` after a successful send so reminders are idempotent and retried on failure. Cancelled reservations are excluded.

## Dependencies

**Depends on:** task-01-reminder-tracking-migration
**Blocks:** None

**Context from dependencies:** task-01 added a nullable `ReminderSentAt` column to `Reservation`; `NULL` means "not yet reminded". STORY-020 provides `IEmailService.SendAsync(to, subject, htmlBody, attachments?, CancellationToken)` (registered in the Notifications module, which already logs/swallows delivery failures). STORY-021 established the email template/factory pattern in `Infrastructure/Notifications/` that can be reused to build the reminder body.

## Files to Create

- `server/src/Infrastructure/Notifications/ReminderBackgroundService.cs` — the scheduled hosted service.
- `server/src/Infrastructure/Notifications/Templates/booking-reminder.html` — reminder email template (token-based, mirroring the confirmation template).
- (Optional) `server/src/Infrastructure/Notifications/ReminderEmailFactory.cs` — builds the reminder subject/body, analogous to the confirmation factory.

## Files to Modify

- `server/src/Infrastructure/Notifications/NotificationsServiceCollectionExtensions.cs` — register the hosted service via `services.AddHostedService<ReminderBackgroundService>()`.

## Technical Details

### Implementation Steps

1. Create `ReminderBackgroundService : BackgroundService`. Inject `IServiceScopeFactory` and `ILogger<ReminderBackgroundService>` via the primary constructor. Do **not** inject the scoped `DbContext` or scoped `IEmailService` directly into this singleton.
2. In `ExecuteAsync`, loop with a 15-minute period using a `PeriodicTimer`, honoring the `stoppingToken`.
3. On each tick, create a DI scope (`scopeFactory.CreateScope()`) and resolve the `DbContext` and `IEmailService` (and the reminder factory) from it.
4. Query eligible reservations in the database (EF LINQ — no in-memory filtering):
   - `Status == Confirmed`
   - `ReminderSentAt == null`
   - slot date/time between `now + 23h` and `now + 25h`
   Include the joins needed for recipient email, restaurant name, slot time, party size.
5. For each eligible reservation: build the reminder email (template + factory), call `IEmailService.SendAsync(...)`. If the send completes without throwing, set `ReminderSentAt = DateTimeOffset.UtcNow`.
6. After processing the batch, `SaveChangesAsync` to persist `ReminderSentAt` for the successfully-sent reservations.
7. Wrap per-tick work in try/catch so one failing tick never tears down the service; log errors.

### Code Snippets

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CM.OpenTable.Infrastructure.Notifications;

public sealed class ReminderBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReminderBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Reminder tick failed; will retry next interval");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessRemindersAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReservationsDbContext>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var now = DateTimeOffset.UtcNow;
        var windowStart = now.AddHours(23);
        var windowEnd = now.AddHours(25);

        var due = await db.Reservations
            .Where(r => r.Status == ReservationStatus.Confirmed
                        && r.ReminderSentAt == null
                        && r.Slot.StartUtc >= windowStart
                        && r.Slot.StartUtc <= windowEnd)
            .Select(r => new { Reservation = r, r.User.Email, r.Restaurant.Name, r.Slot.StartUtc, r.PartySize })
            .ToListAsync(ct);

        foreach (var item in due)
        {
            // build + send reminder email
            await email.SendAsync(item.Email, $"Reminder: your reservation at {item.Name}", /* html */ "...", null, ct);
            item.Reservation.ReminderSentAt = DateTimeOffset.UtcNow;
        }

        if (due.Count > 0)
        {
            await db.SaveChangesAsync(ct);
        }
    }
}
```

Registration (in the Notifications module extension from STORY-020):

```csharp
services.AddHostedService<ReminderBackgroundService>();
```

## Acceptance Criteria

- [ ] A `BackgroundService` runs every 15 minutes and is registered in the Notifications module.
- [ ] The eligibility query filters in the database for `Status = Confirmed`, `ReminderSentAt IS NULL`, and slot time within `now+23h`..`now+25h`.
- [ ] A reminder email is sent for each eligible reservation, and `ReminderSentAt` is set only after a successful send.
- [ ] Cancelled reservations are never reminded.
- [ ] A reservation already reminded is not reminded again (no duplicates).
- [ ] A send failure leaves `ReminderSentAt` null so the reservation is retried next tick; the failure is logged.
- [ ] `dotnet build` succeeds.

## Notes

- Adjust the exact navigation property names (`r.Slot.StartUtc`, `r.User.Email`, `r.Restaurant.Name`, `ReservationStatus.Confirmed`, `ReservationsDbContext`) to match the actual entity/DbContext names from STORY-003/014.
- The 23h–25h window (rather than exactly 24h) tolerates the 15-minute tick cadence so no reservation is missed and `ReminderSentAt` prevents duplicates within the overlapping window.
- BDD tests (`describe_reminder_service` → `when_reservation_is_due` / `when_reservation_is_cancelled` / `when_already_reminded` / `when_send_fails`). Do not create a separate testing task file.
- For multi-instance deployments, a distributed lock or `SELECT ... FOR UPDATE`-style claim would be needed; out of scope for MVP (single instance).
