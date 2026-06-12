# Implementation Plan: Booking Confirmation Email

This plan delivers the confirmation email content (task-01) and the trigger that sends it after a successful reservation (task-02), building on STORY-020's `IEmailService` and STORY-014's `CreateReservationRequestHandler`.

## Phase 1 — Template/Attachment and Trigger

### task-01-confirmation-email-template
- [ ] Create `Infrastructure/Notifications/Templates/booking-confirmation.html` with placeholders for restaurant name, date, time, party size, and the Maps link.
- [ ] Create an `IcsBuilder` that produces a valid iCalendar (`VCALENDAR`/`VEVENT`) with `DTSTART`, `DTEND`, `SUMMARY`, `LOCATION`, `UID`.
- [ ] Create a Maps-link helper producing `https://www.google.com/maps/dir/?api=1&destination=<encoded address>`.
- [ ] Create a `BookingConfirmationEmailFactory` that fills the template, builds the `.ics` `EmailAttachment`, and returns subject + html body + attachment.

### task-02-confirmation-email-trigger
- [ ] Inject the email factory + `IEmailService` into `CreateReservationRequestHandler`.
- [ ] After the reservation transaction commits successfully, dispatch the confirmation email fire-and-forget / via a background task.
- [ ] Catch and log any email error; never let it affect the returned `Result<T>`.

## Verification

- [ ] `dotnet build` succeeds.
- [ ] BDD test confirms a successful booking triggers a send and that an email exception does not change the booking result.
- [ ] Generated `.ics` opens correctly in a calendar app (manual check).
