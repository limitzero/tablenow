# Requirements: Email Service Integration

## Summary

TableNow needs reliable transactional email delivery. The `IEmailService` abstraction allows swapping providers without changing callers. SendGrid is the initial implementation. Polly provides retry logic on transient failures so booking confirmation doesn't fail due to a network hiccup.

## Goals

- `IEmailService` interface in `Infrastructure/Notifications/`
- `SendGridEmailService` with Polly retry (3 attempts, exponential backoff)
- API key from environment config (never source code)
- Delivery outcomes logged

## Acceptance Criteria

- [ ] `IEmailService.SendAsync` delivers an HTML email
- [ ] On transient failure, Polly retries up to 3 times
- [ ] Delivery success/failure logged via ILogger
- [ ] API key from `Email__ApiKey` environment variable

## Technical Constraints

- Abstraction in `Infrastructure/Notifications/` — no email code in Application layer
- Provider-agnostic interface (SendGrid is an implementation detail)
