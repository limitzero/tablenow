# Requirements: 24-Hour Reminder Email

## Summary

Diners benefit from a nudge before their reservation. This feature sends a reminder email approximately 24 hours before a confirmed reservation's time slot. A scheduled `BackgroundService` in the `Infrastructure/Notifications/` module runs every 15 minutes, finds confirmed reservations whose slot time is within a window around 24 hours from now and that have not yet been reminded, and sends the reminder through the `IEmailService` from STORY-020.

To prevent duplicate reminders (the job runs frequently and windows overlap), the `Reservation` entity gains a nullable `ReminderSentAt` timestamp. A reservation is only eligible if `ReminderSentAt IS NULL`, and the column is stamped immediately after a successful send. Cancelled reservations are never reminded.

The outcome is a reliable, idempotent reminder mechanism that reuses the existing email infrastructure and confirmation email patterns from STORY-021.

## Goals

- Add a nullable `ReminderSentAt` column to the `Reservation` entity/EF model with a migration.
- Implement a `BackgroundService` that runs every 15 minutes.
- Query reservations where `Status = Confirmed`, `ReminderSentAt IS NULL`, and slot time is between `now + 23h` and `now + 25h`.
- Send a reminder email via `IEmailService` and stamp `ReminderSentAt` after a successful send.
- Ensure cancelled reservations receive no reminder and no reservation is reminded twice.

## Non-Goals

- The email transport/provider (STORY-020) and the confirmation email content/template (STORY-021).
- SMS or push notifications.
- Configurable per-user reminder lead times.
- A distributed scheduler / multi-instance coordination (single-instance `BackgroundService` is acceptable for MVP).

## Acceptance Criteria

- [ ] Given a confirmed reservation 24 hours away, when the background job runs, then a reminder email is sent to the diner.
- [ ] Given a cancelled reservation, when the job runs, then no reminder is sent.
- [ ] Given the background job, when inspected, then it runs on a schedule (every 15 minutes) and queries upcoming reservations.
- [ ] Given email failure, when it occurs, then the failure is logged and retried on the next run (because `ReminderSentAt` is only set after a successful send).
- [ ] Given a reservation already reminded, when the job runs again, then no duplicate email is sent.

## Assumptions

- STORY-020 is complete: `IEmailService` is registered and injectable.
- STORY-021 is complete: confirmation email patterns and the `Infrastructure/Notifications/` module exist; reminder content can reuse the template/factory approach.
- The `Reservation` entity exposes the slot date/time, status, party size, restaurant name, and the diner's email (directly or via joins).
- The application runs as a single instance for the scheduled job (no need for distributed locking in MVP).

## Technical Constraints

- `BackgroundService`/`IHostedService` lives in `Infrastructure/Notifications/` and is registered via the Notifications module registration.
- The service resolves a scoped `DbContext` per tick by creating a DI scope (`IServiceScopeFactory`) — never inject a scoped `DbContext` into a singleton hosted service directly.
- The eligibility query must filter in the database (EF LINQ), not in memory.
- `ReminderSentAt` is set only after a confirmed successful send so failures are naturally retried next tick.
- File-scoped namespaces, nullable enabled, `CancellationToken` honored on the execute loop, primary constructors for DI.
- Migration created in `server/src/Migrations/`.
