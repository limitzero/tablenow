# Requirements: 24-Hour Reminder Email

## Summary

Diners sometimes forget upcoming reservations. A background job checks every 15 minutes for confirmed reservations in the 23h–25h window and sends a reminder. The `ReminderSent` flag prevents the same reservation from receiving multiple reminders.

## Goals

- `BackgroundService` runs every 15 minutes
- Sends reminder to confirmed reservations with DateTime in `[now+23h, now+25h]` and `ReminderSent=false`
- Sets `ReminderSent=true` after successful send
- Failed sends logged, retried on next run (not marked as sent)

## Acceptance Criteria

- [ ] Background service runs on schedule (every 15 min)
- [ ] Reminder sent only for Confirmed, ReminderSent=false, DateTime in window
- [ ] `ReminderSent` set to true after successful email
- [ ] Failed send logged but service continues (not marked sent — retried next cycle)

## Technical Constraints

- `Reservation.ReminderSent` column required — add to entity + new EF migration
- Use `IServiceScopeFactory` for DbContext access (hosted services are singletons)
