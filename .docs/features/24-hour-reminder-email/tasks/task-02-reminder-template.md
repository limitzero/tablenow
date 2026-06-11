# Task 02: Reminder Email Template & Dedup

## Status

pending

## Wave

2

## Description

Creates a `ReminderEmailBuilder` for the 24-hour reminder email template, and wires it into `ReminderBackgroundService`. The reminder email is simpler than the confirmation — no `.ics` attachment needed.

## Dependencies

**Depends on:** task-01-reminder-service.md
**Blocks:** Nothing

**Context from dependencies:** task-01 created `ReminderBackgroundService` with a stub `// await emailService.SendAsync(...)` that this task fills in. `IEmailService` and `EmailMessage` types are available.

## Files to Create

- `server/src/Infrastructure/CM.TableNow.Auth.Infrastructure/Notifications/Templates/ReminderEmailBuilder.cs`

## Files to Modify

- `server/src/Api/Services/ReminderBackgroundService.cs` — Replace stub with real email call

## Technical Details

### Code Snippets

```csharp
// ReminderEmailBuilder.cs
public static class ReminderEmailBuilder
{
    public static EmailMessage Build(
        string toEmail, string toName,
        string restaurantName, string restaurantAddress,
        DateTime slotDateTime, int partySize)
    {
        var mapsUrl = $"https://www.google.com/maps/dir/?api=1&destination={Uri.EscapeDataString(restaurantAddress)}";
        var dateStr = slotDateTime.ToString("dddd, MMMM d, yyyy 'at' h:mm tt");

        var html = $"""
            <h2>Reminder: Your TableNow Reservation Tomorrow</h2>
            <p>Just a reminder that you have a reservation at <strong>{restaurantName}</strong>.</p>
            <p><strong>Date & Time:</strong> {dateStr}</p>
            <p><strong>Party Size:</strong> {partySize}</p>
            <p><a href="{mapsUrl}">Get Directions</a></p>
            """;

        return new EmailMessage(
            toEmail, toName,
            $"Reminder: Reservation at {restaurantName} Tomorrow",
            html);
    }
}
```

## Acceptance Criteria

- [ ] `ReminderEmailBuilder.Build()` returns an `EmailMessage` with reminder content
- [ ] Background service sends reminder and marks `ReminderSentAt`
- [ ] `dotnet build` exits with code 0
