# Requirements: Booking Confirmation Email

## Summary

After a diner successfully books a reservation, TableNow should send them an HTML confirmation email so they have a reliable, durable record of the booking. The email contains the restaurant name, reservation date, time, and party size; an attached `.ics` calendar file so the booking can be added to any calendar app; and a Google Maps directions link generated from the restaurant's address.

This feature builds on the `IEmailService` abstraction delivered in STORY-020 and hooks into the reservation creation flow from STORY-014. Critically, email sending must be decoupled from the booking transaction: the reservation is already committed and the in-app success response is returned regardless of whether the email is delivered. Any email error is logged, never surfaced as a booking failure.

The outcome is a templated, calendar-enabled confirmation email that fires automatically on every successful reservation.

## Goals

- Author a reusable HTML confirmation email template populated with restaurant name, date, time, and party size.
- Generate a valid `.ics` iCalendar attachment with `DTSTART`, `DTEND`, `SUMMARY`, and `LOCATION`.
- Include a Google Maps directions link built from the restaurant's encoded address.
- Trigger `IEmailService.SendAsync()` after a successful reservation creation in `CreateReservationRequestHandler`.
- Ensure email failure does not fail the booking — send fire-and-forget / as a background task with error logging.

## Non-Goals

- The underlying email transport/provider (delivered in STORY-020).
- The 24-hour reminder email (STORY-022).
- Localization/translation of the email body.
- Rich marketing content, images, or unsubscribe management.

## Acceptance Criteria

- [ ] Given a confirmed reservation, when created, then a confirmation email is sent with restaurant name, date, time, and party size.
- [ ] Given the email, when received, then it includes a `.ics` calendar file attachment.
- [ ] Given the email body, when inspected, then it contains a Google Maps directions link constructed from the restaurant's address.
- [ ] Given email failure, when it occurs, then the reservation is still confirmed and the user sees an in-app success message.
- [ ] Given the `.ics` file, when opened in a calendar app, then it creates an event matching the reservation date/time and restaurant location.

## Assumptions

- STORY-020 is complete: `IEmailService.SendAsync(to, subject, htmlBody, attachments?, CancellationToken)` and the `EmailAttachment` record are available and registered in DI.
- STORY-014 is complete: `CreateReservationRequestHandler` exists and returns a successful `Result<T>` after committing the reservation transaction.
- The reservation/restaurant data available at confirmation time includes the diner's email, restaurant name, full address, slot date/time, and party size.
- A reservation default duration (e.g. 2 hours) can be assumed for `DTEND` if no explicit end time is stored.

## Technical Constraints

- Template stored in `server/src/Infrastructure/Notifications/Templates/booking-confirmation.html` and loaded as an embedded resource or content file.
- `.ics` generation lives in `Infrastructure/Notifications/` (a small `IcsBuilder`/helper). May use a library (e.g. `Ical.Net`) or hand-roll the iCalendar text.
- Google Maps URL format: `https://www.google.com/maps/dir/?api=1&destination=<url-encoded address>`.
- Email send must not be awaited inside the booking transaction in a way that can fail the booking; use fire-and-forget or a queued background task, and log errors.
- File-scoped namespaces, nullable enabled, `CancellationToken` on async methods, primary constructors for DI.
