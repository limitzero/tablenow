# Task 01: HTML Email Template & .ics Generator

## Status

pending

## Wave

1

## Description

Creates an HTML email template for booking confirmations and a static `.ics` iCalendar file generator. Produces a Google Maps directions URL from the restaurant address.

## Dependencies

**Depends on:** STORY-020 task-01-email-interface.md (EmailMessage and EmailAttachment types)
**Blocks:** task-02-handler-integration.md

**Context from dependencies:** `EmailMessage` takes `HtmlBody (string)` and `Attachments (EmailAttachment[])`. `EmailAttachment`: `FileName, Content (byte[]), MimeType`.

## Files to Create

- `server/src/Infrastructure/CM.TableNow.Auth.Infrastructure/Notifications/Templates/ConfirmationEmailBuilder.cs`

## Technical Details

### Code Snippets

```csharp
// ConfirmationEmailBuilder.cs
namespace CM.TableNow.Auth.Infrastructure.Notifications.Templates;

public static class ConfirmationEmailBuilder
{
    public static EmailMessage Build(
        string toEmail, string toName,
        string restaurantName, string restaurantAddress,
        DateTime slotDateTime, int partySize)
    {
        var mapsUrl = $"https://www.google.com/maps/dir/?api=1&destination={Uri.EscapeDataString(restaurantAddress)}";
        var dateStr = slotDateTime.ToString("dddd, MMMM d, yyyy");
        var timeStr = slotDateTime.ToString("h:mm tt");

        var html = $"""
            <h2>Your TableNow Reservation is Confirmed!</h2>
            <p><strong>Restaurant:</strong> {restaurantName}</p>
            <p><strong>Date:</strong> {dateStr}</p>
            <p><strong>Time:</strong> {timeStr}</p>
            <p><strong>Party Size:</strong> {partySize}</p>
            <p><a href="{mapsUrl}">Get Directions</a></p>
            <p>See you soon!</p>
            """;

        var icsContent = GenerateIcs(restaurantName, restaurantAddress, slotDateTime, partySize);

        return new EmailMessage(
            toEmail, toName,
            $"Reservation Confirmed: {restaurantName} on {dateStr}",
            html,
            [new EmailAttachment("reservation.ics", System.Text.Encoding.UTF8.GetBytes(icsContent), "text/calendar")]);
    }

    private static string GenerateIcs(
        string summary, string location, DateTime start, int partySize)
    {
        var end = start.AddHours(2);
        return $"""
            BEGIN:VCALENDAR
            VERSION:2.0
            PRODID:-//TableNow//EN
            BEGIN:VEVENT
            UID:{Guid.NewGuid()}@tablenow
            DTSTART:{start:yyyyMMddTHHmmssZ}
            DTEND:{end:yyyyMMddTHHmmssZ}
            SUMMARY:Dinner at {summary} (Party of {partySize})
            LOCATION:{location}
            END:VEVENT
            END:VCALENDAR
            """;
    }
}
```

## Acceptance Criteria

- [ ] `ConfirmationEmailBuilder.Build()` returns an `EmailMessage` with HTML body and `.ics` attachment
- [ ] Google Maps URL uses `uri.EscapeDataString` for the address
- [ ] `.ics` contains DTSTART, DTEND, SUMMARY, LOCATION
- [ ] `dotnet build` exits with code 0
