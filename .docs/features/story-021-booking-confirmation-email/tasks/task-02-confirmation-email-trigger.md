# Task 02: Confirmation Email Trigger

## Status

pending

## Wave

1

## Description

Trigger the booking confirmation email after a reservation is successfully created. This task injects the confirmation email factory and `IEmailService` into `CreateReservationRequestHandler` and dispatches the email once the reservation transaction has committed. The send is fire-and-forget (or queued as a background task) so that an email failure is logged but never causes the booking to fail — the in-app success response is returned regardless of email delivery.

## Dependencies

**Depends on:** task-01-confirmation-email-template
**Blocks:** None

**Context from dependencies:** task-01 produced `BookingConfirmationEmailFactory.Create(BookingConfirmationDetails)` returning a subject, token-filled HTML body, and a `.ics` `EmailAttachment`, plus the `BookingConfirmationDetails` record (`RecipientEmail`, `RestaurantName`, `RestaurantAddress`, `StartUtc`, `DurationMinutes`, `PartySize`). STORY-020 provides `IEmailService.SendAsync(to, subject, htmlBody, attachments?, CancellationToken)` which already logs and swallows its own delivery failures. STORY-014 provides `CreateReservationRequestHandler`, which commits the reservation in a transaction and returns a successful `Result<T>` with reservation details.

## Files to Modify

- `server/src/Application/Reservations/Features/CreateReservation/CreateReservationRequestHandler.cs` — inject dependencies and send the email after a successful commit.

## Files to Create

- (Optional) `server/src/Infrastructure/Notifications/IBookingConfirmationSender.cs` + implementation — a thin sender that wraps factory + `IEmailService` so the Application handler depends on one abstraction rather than two Infrastructure types. Create if it keeps the handler clean.

## Technical Details

### Implementation Steps

1. Add the confirmation sender dependency to `CreateReservationRequestHandler`'s primary constructor: either inject `BookingConfirmationEmailFactory` + `IEmailService`, or a single `IBookingConfirmationSender`.
2. After the existing reservation creation succeeds (the `Result` is success and the transaction is committed), assemble a `BookingConfirmationDetails` from the reservation/restaurant/user data already loaded in the handler.
3. Dispatch the email **without coupling it to the booking result**. Options:
   - Fire-and-forget with `_ = Task.Run(async () => { ... })` wrapped in try/catch logging, or
   - Enqueue onto a background queue / `IHostedService` channel (preferred for testability).
4. Wrap the send in try/catch and log errors. Because `IEmailService.SendAsync` already swallows transient/permanent delivery failures, this catch is a defensive backstop for unexpected exceptions (e.g. template assembly).
5. Return the existing successful `Result<CreateReservationResponse>` unchanged.

### Code Snippets

Inside `CreateReservationRequestHandler` after a successful commit:

```csharp
// Reservation already committed; result is success.
var details = new BookingConfirmationDetails(
    RecipientEmail: user.Email,
    RestaurantName: restaurant.Name,
    RestaurantAddress: restaurant.Address,
    StartUtc: slot.StartUtc,
    DurationMinutes: 120,
    PartySize: request.PartySize);

// Fire-and-forget: email failure must never fail the booking.
_ = SendConfirmationSafelyAsync(details, cancellationToken);

return Result<CreateReservationResponse>.Success(response);
```

```csharp
private async Task SendConfirmationSafelyAsync(BookingConfirmationDetails details, CancellationToken ct)
{
    try
    {
        var email = _confirmationFactory.Create(details);
        await _emailService.SendAsync(
            details.RecipientEmail, email.Subject, email.HtmlBody, new[] { email.Attachment }, ct);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to dispatch booking confirmation email for {Recipient}", details.RecipientEmail);
    }
}
```

## Acceptance Criteria

- [ ] On a successful reservation, a confirmation email is dispatched with restaurant name, date, time, party size, the Maps link, and the `.ics` attachment.
- [ ] An email failure (exception or delivery failure) does not change the handler's returned `Result<T>` — the booking still succeeds.
- [ ] The email send happens after the reservation transaction is committed.
- [ ] Email errors are logged.
- [ ] `dotnet build` succeeds.

## Notes

- Prefer a background-queue approach over raw `Task.Run` if the solution already has an `IHostedService`/channel pattern — it makes the send observable in tests and avoids losing work on shutdown.
- BDD test (`describe_create_reservation` → `when_reservation_is_created` → `it_should_dispatch_a_confirmation_email`, and `when_email_send_fails` → `it_should_still_return_success`). Do not create a separate testing task file.
- Recipient email and restaurant address must come from server-side data (loaded in the handler), not from the request body.
