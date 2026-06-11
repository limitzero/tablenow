# Requirements: Booking Confirmation Email

## Summary

After a successful booking, the diner receives an HTML confirmation email with restaurant details, a Google Maps link, and a `.ics` calendar file so the reservation appears on their calendar. Email delivery is best-effort — if it fails, the reservation is still confirmed.

## Goals

- HTML email: restaurant name, date, time, party size, address, Google Maps link
- `.ics` calendar file attachment
- Email sent after 201 reservation created
- Email failure does NOT fail the reservation

## Acceptance Criteria

- [ ] Confirmation email sent after successful booking
- [ ] Email contains restaurant name, date, time, party size
- [ ] Email contains Google Maps link to restaurant address
- [ ] Email has `.ics` attachment (VCALENDAR format)
- [ ] Email failure logged but reservation unaffected

## Technical Constraints

- Google Maps URL: `https://www.google.com/maps/dir/?api=1&destination=<URI-encoded address>`
- `.ics` format: RFC 5545 with DTSTART, DTEND, SUMMARY, LOCATION
- Email failure: catch exception, log, return reservation result normally
