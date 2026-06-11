# Requirements: 24-Hour Reminder Email

## Summary

A `BackgroundService` polls every 15 minutes for confirmed reservations with `SlotDateTime` between `now + 23h` and `now + 25h`. Sends reminder emails; marks sent to prevent duplicates.

## Goals

- Reminder sent 24h before confirmed reservations
- No duplicate sends
- Cancelled reservations skipped
- Email failure logged, retried next run

## Acceptance Criteria

- [ ] BackgroundService runs every 15 minutes
- [ ] Sends reminders for reservations in 23–25h window
- [ ] Dedup tracking prevents duplicate emails
- [ ] Cancelled reservations excluded

## Technical Constraints

- `BackgroundService` or `IHostedService`
- Add `ReminderSentAt` nullable DateTime to `Reservation` entity (or separate tracking table)
