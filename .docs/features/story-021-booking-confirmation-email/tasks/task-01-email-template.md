# Task 01: Email Template & ICS Generator

## Status

pending

## Wave

1

## Description

Creates the HTML email template, an `IcsGenerator` static class for RFC 5545 `.ics` files, and an `EmailTemplateRenderer` that replaces `{{Key}}` placeholders with provided values.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-020 IEmailService to exist)
**Blocks:** task-02-handler-integration.md

**Context from dependencies:** STORY-020 task-01 created `IEmailService` and `EmailAttachment`. This task creates the template files and utilities that callers pass to `IEmailService.SendAsync`.

## Files to Create

- `server/src/Infrastructure/Notifications/Templates/BookingConfirmation.html`
- `server/src/Infrastructure/Notifications/IcsGenerator.cs`
- `server/src/Infrastructure/Notifications/EmailTemplateRenderer.cs`

## Technical Details

### Code Snippets

```html
<!-- Templates/BookingConfirmation.html -->
<!DOCTYPE html>
<html>
<head><meta charset="utf-8"><title>Reservation Confirmed</title></head>
<body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
  <h1 style="color: #333;">Your reservation is confirmed!</h1>
  <table>
    <tr><td><strong>Restaurant:</strong></td><td>{{RestaurantName}}</td></tr>
    <tr><td><strong>Date:</strong></td><td>{{Date}}</td></tr>
    <tr><td><strong>Time:</strong></td><td>{{Time}}</td></tr>
    <tr><td><strong>Party size:</strong></td><td>{{PartySize}}</td></tr>
    <tr><td><strong>Address:</strong></td><td>{{Address}}</td></tr>
  </table>
  <p><a href="{{GoogleMapsUrl}}">Get directions on Google Maps</a></p>
  <p style="color: #666; font-size: 12px;">A calendar invite is attached. See you soon!</p>
</body>
</html>
```

```csharp
// IcsGenerator.cs
namespace TableNow.Infrastructure.Notifications;

public static class IcsGenerator
{
    public static byte[] Generate(
        string summary, DateTimeOffset start, DateTimeOffset end, string location)
    {
        var ics = $"""
BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//TableNow//EN
BEGIN:VEVENT
UID:{Guid.NewGuid()}@tablenow.com
DTSTART:{start.UtcDateTime:yyyyMMddTHHmmssZ}
DTEND:{end.UtcDateTime:yyyyMMddTHHmmssZ}
SUMMARY:{summary}
LOCATION:{location}
END:VEVENT
END:VCALENDAR
""";
        return Encoding.UTF8.GetBytes(ics);
    }
}
```

```csharp
// EmailTemplateRenderer.cs
namespace TableNow.Infrastructure.Notifications;

public class EmailTemplateRenderer
{
    public string Render(string templateName, Dictionary<string, string> values)
    {
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, "Templates", $"{templateName}.html");
        var html = File.ReadAllText(templatePath);
        return values.Aggregate(html,
            (current, kv) => current.Replace($"{{{{{kv.Key}}}}}", kv.Value));
    }
}
```

Also register `EmailTemplateRenderer` as a singleton in `NotificationsModuleRegistration`.

## Acceptance Criteria

- [ ] `BookingConfirmation.html` has `{{RestaurantName}}`, `{{Date}}`, `{{Time}}`, `{{PartySize}}`, `{{Address}}`, `{{GoogleMapsUrl}}` placeholders
- [ ] `IcsGenerator.Generate` returns valid RFC 5545 bytes with DTSTART, DTEND, SUMMARY, LOCATION
- [ ] `EmailTemplateRenderer.Render` replaces all `{{Key}}` placeholders
