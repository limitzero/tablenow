# Requirements: Email Service Integration

## Summary

Provides a provider-agnostic `IEmailService` abstraction in `Infrastructure/Notifications/`. Backed by SendGrid or Mailgun. Retry logic on transient failures, logs delivery outcomes. API keys come from environment config.

## Goals

- `IEmailService` sends HTML emails reliably
- Delivery failures do not crash the calling flow
- API keys from environment variables only
- Provider swappable via config

## Non-Goals

- No email templates here (STORY-021)
- No email tracking/webhooks

## Acceptance Criteria

- [ ] `IEmailService.SendAsync(EmailMessage)` interface exists
- [ ] Concrete implementation uses SendGrid or Mailgun
- [ ] Transient failures are retried; errors logged
- [ ] `dotnet build` exits with code 0

## Technical Constraints

- `IEmailService` in `Infrastructure/Notifications/`
- API key from `IOptions<EmailOptions>` bound from `appsettings.json`
