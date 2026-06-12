# Task 02: Email Provider Implementation

## Status

pending

## Wave

1

## Description

Provide the concrete email provider implementation behind the `IEmailService` abstraction. This task implements `EmailService` against a real provider (SendGrid or Mailgun), wraps sends in a Polly retry policy for transient failures, logs every delivery outcome, and never throws on send failure. It also adds the `RegisterNotificationsModule()` extension that binds configuration and registers the service, then wires it into `ServiceCollectionExtensions.RegisterServices()`.

## Dependencies

**Depends on:** task-01-email-service-interface
**Blocks:** None

**Context from dependencies:** task-01 defines `IEmailService` (`SendAsync(to, subject, htmlBody, attachments?, CancellationToken)`), the `EmailAttachment` record, and the `EmailOptions` class (`Provider`, `ApiKey`, `FromAddress`, `FromName`, `SectionName = "Email"`) bound from the `Email` configuration section. This task supplies the concrete class fulfilling that contract and registers it in DI.

## Files to Create

- `server/src/Infrastructure/Notifications/EmailService.cs` — concrete provider implementation with retry + logging.
- `server/src/Infrastructure/Notifications/NotificationsServiceCollectionExtensions.cs` — `RegisterNotificationsModule()` extension.

## Files to Modify

- `server/src/Api/ServiceCollectionExtensions.cs` (or the equivalent file containing `RegisterServices()`) — call `RegisterNotificationsModule(configuration)`.
- `server/src/Infrastructure/Notifications/<project>.csproj` — add the provider SDK and Polly package references.

## Technical Details

### Implementation Steps

1. Add NuGet packages to the Notifications project:
   - Provider SDK — `SendGrid` (recommended) or a Mailgun client.
   - Resilience — `Polly` or `Microsoft.Extensions.Http.Resilience`.
2. Implement `EmailService` using a primary constructor injecting `IOptions<EmailOptions>`, `ILogger<EmailService>`, and the provider client (or an `HttpClient` from `IHttpClientFactory`).
3. Build a Polly async retry policy: retry transient failures (HTTP timeout, 5xx, 429) with exponential backoff (e.g. 3 attempts: 1s, 2s, 4s).
4. In `SendAsync`, construct the provider message from the `to`, `subject`, `htmlBody`, and optional `attachments`, then execute the send inside the retry policy.
5. Wrap the whole operation in try/catch. On success, log information. On exhausted retries or permanent failure, log an error and return normally — **never throw**. This guarantees callers' booking flows are unaffected.
6. Create `RegisterNotificationsModule()` that binds `EmailOptions`, registers the provider client, and registers `IEmailService` → `EmailService` (scoped or transient).
7. Call `services.RegisterNotificationsModule(configuration)` from `RegisterServices()`.

### Code Snippets

`EmailService.cs` (SendGrid illustration — retry + swallow failures):

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace CM.OpenTable.Infrastructure.Notifications;

public sealed class EmailService(
    IOptions<EmailOptions> options,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailOptions _options = options.Value;

    private static readonly AsyncRetryPolicy RetryPolicy = Policy
        .Handle<Exception>(IsTransient)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)));

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        IReadOnlyCollection<EmailAttachment>? attachments = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await RetryPolicy.ExecuteAsync(async ct =>
            {
                // Build provider message from _options.FromAddress/_options.FromName,
                // to, subject, htmlBody, attachments — then send.
                // var response = await client.SendEmailAsync(message, ct);
                // if (!IsSuccess(response)) throw new EmailDeliveryException(...);
            }, cancellationToken);

            logger.LogInformation("Email sent to {Recipient} with subject {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            // Never rethrow — email failure must not break the calling (booking) flow.
            logger.LogError(ex, "Failed to send email to {Recipient} with subject {Subject}", to, subject);
        }
    }

    private static bool IsTransient(Exception ex) =>
        ex is TimeoutException or HttpRequestException; // extend with provider-specific transient codes (429, 5xx)
}
```

`NotificationsServiceCollectionExtensions.cs`:

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CM.OpenTable.Infrastructure.Notifications;

public static class NotificationsServiceCollectionExtensions
{
    public static IServiceCollection RegisterNotificationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }
}
```

Wire-up in `RegisterServices()`:

```csharp
services.RegisterNotificationsModule(configuration);
```

### Environment Variables

- `Email__ApiKey` — provider API key, supplied at runtime (never committed). See action-required.md.
- `Email__FromAddress` / `Email__FromName` — optional overrides of the `appsettings.json` defaults.

## Acceptance Criteria

- [ ] `EmailService` implements `IEmailService` and sends an HTML email via the chosen provider.
- [ ] Transient failures (timeouts, 5xx, 429) are retried with backoff via Polly.
- [ ] Delivery success and failure are logged; a failure never throws out of `SendAsync`.
- [ ] `RegisterNotificationsModule(configuration)` binds `EmailOptions` and registers `IEmailService`.
- [ ] `RegisterServices()` calls `RegisterNotificationsModule(configuration)`.
- [ ] `dotnet build` succeeds; `IEmailService` resolves to `EmailService` from DI.

## Notes

- A BDD unit test (`describe_email_service` → `when_provider_fails_transiently` / `when_delivery_fails_permanently`) should verify retry occurs and that a permanent failure is logged without throwing. Do not create a separate testing task file — include the test alongside this implementation.
- If using `IHttpClientFactory` with `Microsoft.Extensions.Http.Resilience`, prefer `AddStandardResilienceHandler()` over a hand-rolled Polly policy.
- Keep the provider choice behind the implementation; downstream code must never reference provider types.
