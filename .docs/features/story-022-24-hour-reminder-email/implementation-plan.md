# Implementation Plan: 24-Hour Reminder Email

This plan adds reminder tracking to the schema (task-01) and a scheduled background service that sends reminders idempotently (task-02), reusing STORY-020's `IEmailService` and STORY-021's email-content patterns.

## Phase 1 — Tracking Column and Background Service

### task-01-reminder-tracking-migration
- [ ] Add `ReminderSentAt` (nullable `DateTime`/`DateTimeOffset`) to the `Reservation` domain entity and EF model.
- [ ] Update the Fluent API configuration if needed.
- [ ] Create and verify the EF migration.

### task-02-reminder-background-service
- [ ] Implement `ReminderBackgroundService : BackgroundService` in `Infrastructure/Notifications/`.
- [ ] Loop every 15 minutes; on each tick create a DI scope and resolve a `DbContext`.
- [ ] Query `Status = Confirmed AND ReminderSentAt IS NULL AND SlotDateTime BETWEEN now+23h AND now+25h`.
- [ ] For each, send the reminder email via `IEmailService`; on success set `ReminderSentAt = now` and save.
- [ ] Log failures; leave `ReminderSentAt` null so the next tick retries.
- [ ] Register the hosted service in the Notifications module registration.

## Verification

- [ ] `dotnet build` succeeds; migration applies cleanly.
- [ ] BDD tests: reminder sent for an eligible reservation; no reminder for cancelled; no duplicate when already stamped; failure leaves the row eligible.
