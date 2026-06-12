# Implementation Plan: Email Service Integration

This plan delivers a provider-agnostic email service in the `Infrastructure/Notifications/` module. The interface (task-01) is the contract that downstream notification stories depend on; the provider implementation (task-02) supplies resilience and registration.

## Phase 1 — Abstraction and Provider

### task-01-email-service-interface
- [ ] Create `server/src/Infrastructure/Notifications/IEmailService.cs` with `Task SendAsync(string to, string subject, string htmlBody, IReadOnlyCollection<EmailAttachment>? attachments = null, CancellationToken cancellationToken = default)`.
- [ ] Create `EmailAttachment` record (`FileName`, `ContentType`, `Content` as `byte[]`).
- [ ] Create `EmailOptions` (`ApiKey`, `FromAddress`, `FromName`, `Provider`) bound from the `Email` configuration section.
- [ ] Keep the interface free of any provider-specific types.

### task-02-email-provider-implementation
- [ ] Add the chosen provider SDK package (e.g. `SendGrid` or `Mailgun` client) and `Polly` (or `Microsoft.Extensions.Http.Resilience`).
- [ ] Implement the concrete `EmailService` using a primary constructor (`IOptions<EmailOptions>`, `ILogger<EmailService>`, provider client/`HttpClient`).
- [ ] Wrap the send call in a Polly retry policy for transient failures (timeouts, 5xx, 429) with exponential backoff.
- [ ] Log success and failure outcomes; never throw on delivery failure — catch, log, and return.
- [ ] Add `RegisterNotificationsModule(this IServiceCollection services, IConfiguration configuration)` that binds `EmailOptions`, registers the provider client, and registers `IEmailService`.
- [ ] Call `RegisterNotificationsModule()` from `ServiceCollectionExtensions.RegisterServices()`.

## Verification

- [ ] `dotnet build` succeeds with zero errors.
- [ ] `IEmailService` resolves from DI to the concrete implementation.
- [ ] Unit test (`describe_email_service`) verifies a transient failure is retried and a permanent failure is logged without throwing.
