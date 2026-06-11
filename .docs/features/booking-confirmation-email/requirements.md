# Requirements: Booking Confirmation Email

## Summary

After a successful reservation, sends an HTML email with restaurant name, date, time, party size. Includes a `.ics` iCalendar attachment and a Google Maps directions link. Email failure does not fail the booking response.

## Goals

- Confirmation email sent after successful booking
- `.ics` calendar attachment included
- Google Maps link from restaurant address
- Email failure is fire-and-forget

## Acceptance Criteria

- [ ] Email sent after `POST /api/reservations` 201
- [ ] Email contains restaurant name, date, time, party size
- [ ] `.ics` attachment included with correct DTSTART, DTEND, SUMMARY, LOCATION
- [ ] Google Maps link: `https://www.google.com/maps/dir/?api=1&destination=<encoded address>`
- [ ] Booking still succeeds even if email fails

## Technical Constraints

- Trigger in `CreateReservationRequestHandler`
- `.ics` generation manual (no NuGet dependency)
- HTML template in `Infrastructure/Notifications/Templates/`
