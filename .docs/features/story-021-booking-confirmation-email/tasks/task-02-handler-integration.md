# Task 02: Handler Integration

## Status

pending

## Wave

2

## Description

Modifies `CreateReservationRequestHandler` to send the confirmation email after a successful booking. Email failure is caught, logged, and does NOT propagate — the reservation result is returned normally.

## Dependencies

**Depends on:** task-01-email-template.md
**Blocks:** Nothing

**Context from dependencies:** task-01 created `IcsGenerator`, `EmailTemplateRenderer`, and the HTML template. STORY-014 task-01 created `CreateReservationRequestHandler` in `Application/Reservations/Features/CreateReservation/`. STORY-020 created `IEmailService`. This task modifies the handler to inject the email service and template renderer.

## Files to Modify

- `server/src/Application/Reservations/Features/CreateReservation/CreateReservationRequestHandler.cs`

## Technical Details

### Implementation Steps

Add `IEmailService` and `EmailTemplateRenderer` to the handler's primary constructor. After getting a successful result from the Data layer:

```csharp
// After successful reservation creation — BEFORE returning the result:
try
{
    var googleMapsUrl = $"https://www.google.com/maps/dir/?api=1&destination={Uri.EscapeDataString(restaurantAddress)}";
    var html = _templateRenderer.Render("BookingConfirmation", new Dictionary<string, string>
    {
        ["RestaurantName"] = restaurantName,
        ["Date"] = slotDateTime.ToString("dddd, MMMM d, yyyy"),
        ["Time"] = slotDateTime.ToString("h:mm tt"),
        ["PartySize"] = command.PartySize.ToString(),
        ["Address"] = restaurantAddress,
        ["GoogleMapsUrl"] = googleMapsUrl,
    });

    var ics = IcsGenerator.Generate(
        $"Dinner at {restaurantName}",
        slotDateTime,
        slotDateTime.AddHours(2),
        restaurantAddress);

    await _emailService.SendAsync(
        userEmail,
        $"Your reservation at {restaurantName} is confirmed",
        html,
        [new EmailAttachment("reservation.ics", ics, "text/calendar")],
        cancellationToken);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to send confirmation email for reservation {ReservationId}", reservationId);
    // IMPORTANT: do NOT rethrow — reservation succeeded regardless of email failure
}
```

Note: The handler needs to retrieve `userEmail` and `restaurantAddress` — include them in the `ReservationCreatedDto` returned by the Data command, or make a separate query.

## Acceptance Criteria

- [ ] Confirmation email sent after successful 201 reservation
- [ ] Email includes `.ics` attachment
- [ ] Email failure caught with `LogError` and does NOT throw
- [ ] Reservation result returned normally even when email fails
