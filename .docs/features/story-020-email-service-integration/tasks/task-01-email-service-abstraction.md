# Task 01: Email Service Abstraction

## Status

pending

## Wave

1

## Description

Creates `IEmailService` interface, `EmailAttachment` record, and `SendGridEmailService` implementation with Polly retry. The interface is provider-agnostic — callers only know `SendAsync`.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-001 Infrastructure/Notifications project with SendGrid + Polly packages)
**Blocks:** task-02-email-registration.md, STORY-021

**Context from dependencies:** STORY-001 added `SendGrid` and `Polly` NuGet packages to `Infrastructure.Notifications.csproj`. This task creates the implementation files in that project.

## Files to Create

- `server/src/Infrastructure/Notifications/IEmailService.cs`
- `server/src/Infrastructure/Notifications/EmailAttachment.cs`
- `server/src/Infrastructure/Notifications/SendGridEmailService.cs`

## Technical Details

### Code Snippets

```csharp
// IEmailService.cs
namespace TableNow.Infrastructure.Notifications;

public interface IEmailService
{
    Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        IEnumerable<EmailAttachment>? attachments = null,
        CancellationToken cancellationToken = default);
}
```

```csharp
// EmailAttachment.cs
namespace TableNow.Infrastructure.Notifications;
public record EmailAttachment(string Filename, byte[] Content, string ContentType);
```

```csharp
// SendGridEmailService.cs
namespace TableNow.Infrastructure.Notifications;

public class SendGridEmailService(
    IOptions<EmailOptions> options,
    ILogger<SendGridEmailService> logger)
    : IEmailService
{
    private readonly EmailOptions _options = options.Value;

    private static readonly AsyncRetryPolicy RetryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(3,
            attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (ex, delay, attempt, _) =>
                Console.WriteLine($"SendGrid retry {attempt} after {delay}: {ex.Message}"));

    public async Task SendAsync(
        string to, string subject, string htmlBody,
        IEnumerable<EmailAttachment>? attachments = null,
        CancellationToken cancellationToken = default)
    {
        await RetryPolicy.ExecuteAsync(async () =>
        {
            var client = new SendGridClient(_options.ApiKey);
            var from = new EmailAddress(_options.FromAddress, _options.FromName);
            var msg = MailHelper.CreateSingleEmail(
                from, new EmailAddress(to), subject, plainTextContent: null, htmlContent: htmlBody);

            foreach (var att in attachments ?? [])
            {
                msg.AddAttachment(att.Filename,
                    Convert.ToBase64String(att.Content), att.ContentType);
            }

            var response = await client.SendEmailAsync(msg, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                logger.LogError("SendGrid delivery failed: {Status} {Body}", response.StatusCode, body);
                throw new InvalidOperationException($"SendGrid returned {response.StatusCode}");
            }
            logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        });
    }
}
```

## Acceptance Criteria

- [ ] `IEmailService.SendAsync` signature: to, subject, htmlBody, optional attachments, CancellationToken
- [ ] `SendGridEmailService` uses Polly retry (3 attempts, exponential backoff)
- [ ] Failed delivery logged with `logger.LogError`
- [ ] Successful delivery logged with `logger.LogInformation`
