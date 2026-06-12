# Requirements: Email Service Integration

## Summary

TableNow needs to send transactional emails — booking confirmations and 24-hour reminders — to diners. Rather than coupling notification features directly to a specific email vendor, this feature introduces a provider-agnostic `IEmailService` abstraction in the `Infrastructure/Notifications/` module. Notification features depend only on this interface, so the underlying provider (SendGrid or Mailgun) can be swapped without touching feature code.

The implementation must be resilient: transient provider failures (timeouts, 5xx responses, rate limiting) are retried automatically, and any unrecoverable delivery failure is logged but never propagated as an exception that could crash a booking. This guarantees that the in-app reservation confirmation always succeeds regardless of email delivery status.

The expected outcome is a registered, injectable `IEmailService` with a single `SendAsync` method that downstream stories (021 Booking Confirmation Email, 022 Reminder Email) can call.

## Goals

- Define a provider-agnostic `IEmailService` abstraction with a `SendAsync(to, subject, htmlBody, attachments?)` contract.
- Bind provider configuration (API key, from-address, from-name) from `appsettings.json` via the Options pattern, with secrets supplied through environment configuration.
- Provide a concrete provider implementation (SendGrid or Mailgun) with retry on transient failures using Polly.
- Log delivery outcomes (success and failure) without throwing on delivery failure.
- Self-register the module via `RegisterNotificationsModule()` called from `ServiceCollectionExtensions.RegisterServices()`.

## Non-Goals

- Email template authoring and `.ics` calendar attachment generation (handled in STORY-021).
- The 24-hour reminder background service (handled in STORY-022).
- In-app notification UI or notification persistence/history.
- Inbound email handling or webhooks for bounce/complaint tracking.

## Acceptance Criteria

- [ ] Given a booking confirmation, when the email service is called, then an HTML email is delivered (target: within 30 seconds).
- [ ] Given a delivery failure, when it occurs, then the error is logged and does not crash the calling flow.
- [ ] Given the in-app confirmation, when shown, then it appears regardless of email delivery status.
- [ ] Given the email provider config, when inspected, then API keys are sourced from environment configuration (never source code).
- [ ] Given `IEmailService`, when resolved from DI, then a concrete provider implementation is returned.

## Assumptions

- An email provider account (SendGrid or Mailgun) is available and an API key has been provisioned (see [Action Required](./action-required.md)).
- A verified sender address/domain exists for the chosen provider.
- STORY-007 (JWT middleware) is complete, so the `Api` startup and `ServiceCollectionExtensions.RegisterServices()` wiring already exist.

## Technical Constraints

- Module lives in `server/src/Infrastructure/Notifications/` following the modular monolith layout.
- Use the Options pattern (`IOptions<EmailOptions>`) for configuration binding; nullable enabled; file-scoped namespaces.
- Use primary constructors for DI and `CancellationToken` on the async method.
- Retry/resilience implemented with Polly (`Microsoft.Extensions.Http.Resilience` or `Polly` directly).
- The interface must not expose any provider-specific types — only primitive/abstract parameters.
- Delivery failures must be swallowed (logged) by callers' contract; the service itself returns a result/bool or void and logs internally rather than throwing for transient/permanent send failures.
