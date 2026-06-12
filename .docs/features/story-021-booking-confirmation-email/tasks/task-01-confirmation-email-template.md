# Task 01: Confirmation Email Template + .ics Attachment

## Status

pending

## Wave

1

## Description

Produce the content side of the booking confirmation email: a reusable HTML template, a generator for a valid `.ics` iCalendar attachment, and a Google Maps directions link helper. This task also assembles these into a factory that, given reservation details, returns the email subject, populated HTML body, and the `.ics` `EmailAttachment`. The trigger task (task-02) consumes this factory to actually send the email.

## Dependencies

**Depends on:** None within this story (depends on STORY-020 being complete)
**Blocks:** task-02-confirmation-email-trigger

**Context from dependencies:** STORY-020 created the `Infrastructure/Notifications/` module exposing `IEmailService.SendAsync(string to, string subject, string htmlBody, IReadOnlyCollection<EmailAttachment>? attachments, CancellationToken)` and the `EmailAttachment` record (`FileName`, `ContentType`, `Content` as `byte[]`). This task produces the html body string and an `EmailAttachment` instance shaped for that contract.

## Files to Create

- `server/src/Infrastructure/Notifications/Templates/booking-confirmation.html` — HTML email template with placeholder tokens.
- `server/src/Infrastructure/Notifications/IcsBuilder.cs` — builds an iCalendar `.ics` byte payload.
- `server/src/Infrastructure/Notifications/MapsLinkBuilder.cs` — builds a Google Maps directions URL.
- `server/src/Infrastructure/Notifications/BookingConfirmationEmailFactory.cs` — fills the template, builds the `.ics`, and returns subject/body/attachment.
- `server/src/Infrastructure/Notifications/BookingConfirmationDetails.cs` — record carrying the data needed to build the email.

## Files to Modify

- `server/src/Infrastructure/Notifications/<project>.csproj` — mark the template as an embedded resource (or copy-to-output content file).

## Technical Details

### Implementation Steps

1. Create `BookingConfirmationDetails` record with the fields needed to render: `RecipientEmail`, `RestaurantName`, `RestaurantAddress`, `StartUtc` (DateTimeOffset), `DurationMinutes` (default 120), `PartySize`.
2. Create the HTML template with simple `{{Token}}` placeholders for restaurant name, formatted date, formatted time, party size, and the Maps link. Embed it as a resource or content file.
3. Create `MapsLinkBuilder.BuildDirectionsUrl(address)` returning `https://www.google.com/maps/dir/?api=1&destination=<Uri.EscapeDataString(address)>`.
4. Create `IcsBuilder.Build(...)` producing a valid `VCALENDAR`/`VEVENT` with `UID`, `DTSTAMP`, `DTSTART`, `DTEND` (start + duration), `SUMMARY` ("Reservation at {RestaurantName}"), and `LOCATION` (restaurant address). Times in UTC (`yyyyMMddTHHmmssZ`). Return UTF-8 bytes.
5. Create `BookingConfirmationEmailFactory.Create(BookingConfirmationDetails)` that loads the template, replaces tokens, builds the Maps link, builds the `.ics`, and returns a small result (`Subject`, `HtmlBody`, `EmailAttachment`).

### Code Snippets

`MapsLinkBuilder.cs`:

```csharp
namespace CM.OpenTable.Infrastructure.Notifications;

public static class MapsLinkBuilder
{
    public static string BuildDirectionsUrl(string destinationAddress) =>
        $"https://www.google.com/maps/dir/?api=1&destination={Uri.EscapeDataString(destinationAddress)}";
}
```

`IcsBuilder.cs` (hand-rolled iCalendar):

```csharp
using System.Text;

namespace CM.OpenTable.Infrastructure.Notifications;

public static class IcsBuilder
{
    public static EmailAttachment Build(
        string restaurantName,
        string location,
        DateTimeOffset startUtc,
        int durationMinutes)
    {
        var endUtc = startUtc.AddMinutes(durationMinutes);
        const string fmt = "yyyyMMddTHHmmssZ";

        var ics = new StringBuilder()
            .AppendLine("BEGIN:VCALENDAR")
            .AppendLine("VERSION:2.0")
            .AppendLine("PRODID:-//TableNow//Reservation//EN")
            .AppendLine("BEGIN:VEVENT")
            .AppendLine($"UID:{Guid.NewGuid():N}@tablenow")
            .AppendLine($"DTSTAMP:{DateTimeOffset.UtcNow.ToString(fmt)}")
            .AppendLine($"DTSTART:{startUtc.UtcDateTime.ToString(fmt)}")
            .AppendLine($"DTEND:{endUtc.UtcDateTime.ToString(fmt)}")
            .AppendLine($"SUMMARY:Reservation at {restaurantName}")
            .AppendLine($"LOCATION:{location}")
            .AppendLine("END:VEVENT")
            .AppendLine("END:VCALENDAR");

        return new EmailAttachment("reservation.ics", "text/calendar", Encoding.UTF8.GetBytes(ics.ToString()));
    }
}
```

`booking-confirmation.html` (token-based):

```html
<!DOCTYPE html>
<html>
  <body style="font-family: Arial, sans-serif; color: #222;">
    <h1>Your reservation is confirmed</h1>
    <p>You're booked at <strong>{{RestaurantName}}</strong>.</p>
    <ul>
      <li><strong>Date:</strong> {{Date}}</li>
      <li><strong>Time:</strong> {{Time}}</li>
      <li><strong>Party size:</strong> {{PartySize}}</li>
    </ul>
    <p><a href="{{MapsUrl}}">Get directions on Google Maps</a></p>
    <p>A calendar invite is attached to this email.</p>
  </body>
</html>
```

## Acceptance Criteria

- [ ] `booking-confirmation.html` exists in `Infrastructure/Notifications/Templates/` and is embedded/copied so it can be loaded at runtime.
- [ ] `IcsBuilder` produces a valid `.ics` with `DTSTART`, `DTEND`, `SUMMARY`, `LOCATION`, returned as an `EmailAttachment` (`text/calendar`).
- [ ] `MapsLinkBuilder` produces `https://www.google.com/maps/dir/?api=1&destination=<encoded address>`.
- [ ] `BookingConfirmationEmailFactory.Create(...)` returns a subject, a token-filled HTML body, and the `.ics` attachment.
- [ ] `dotnet build` succeeds.

## Notes

- Default reservation duration of 120 minutes is acceptable for `DTEND` unless the slot stores an explicit end time.
- iCalendar lines technically should fold at 75 octets; for short reservation fields this is not a practical concern, but escaping commas/semicolons in `LOCATION`/`SUMMARY` is recommended if addresses contain them.
- Prefer loading the template once (cache the string) rather than re-reading the resource on every send.
