# Task 02: Email Provider Implementation

## Status

pending

## Wave

2

## Description

Implements `IEmailService` using SendGrid (primary) with Polly retry logic for transient failures. Logs delivery outcomes. Registers the concrete implementation against `IEmailService` in DI.

## Dependencies

**Depends on:** task-01-email-interface.md
**Blocks:** STORY-021

**Context from dependencies:** task-01 created `IEmailService`, `EmailMessage`, `EmailOptions`. `EmailOptions.ApiKey` comes from `appsettings.json` `Email:ApiKey`. `EmailOptions.Provider` is "SendGrid" by default.

## Files to Create

- `server/src/Infrastructure/CM.TableNow.Auth.Infrastructure/Notifications/SendGridEmailService.cs`

## Files to Modify

- `server/src/Infrastructure/CM.TableNow.Auth.Infrastructure/Extensions/ServiceCollectionExtensions.cs` — Register `SendGridEmailService` as `IEmailService`

## Technical Details

### Code Snippets

```powershell
dotnet add server/src/Infrastructure/CM.TableNow.Auth.Infrastructure package SendGrid
dotnet add server/src/Infrastructure/CM.TableNow.Auth.Infrastructure package Polly
```

```csharp
// SendGridEmailService.cs
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Polly;

namespace CM.TableNow.Auth.Infrastructure.Notifications;

public class SendGridEmailService(
    IOptions<EmailOptions> options,
    ILogger<SendGridEmailService> logger) : IEmailService
{
    private static readonly ResiliencePipeline _retry = new ResiliencePipelineBuilder()
        .AddRetry(new Polly.Retry.RetryStrategyOptions { MaxRetryAttempts = 3, Delay = TimeSpan.FromSeconds(2) })
        .Build();

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var opts = options.Value;
        try
        {
            await _retry.ExecuteAsync(async token =>
            {
                var client = new SendGridClient(opts.ApiKey);
                var msg = new SendGridMessage
                {
                    From = new EmailAddress(opts.FromAddress, opts.FromName),
                    Subject = message.Subject,
                    HtmlContent = message.HtmlBody,
                };
                msg.AddTo(new EmailAddress(message.ToAddress, message.ToName));

                foreach (var attachment in message.Attachments ?? [])
                {
                    msg.AddAttachment(attachment.FileName,
                        Convert.ToBase64String(attachment.Content),
                        attachment.MimeType);
                }

                var response = await client.SendEmailAsync(msg, token);
                logger.LogInformation("Email to {To} delivered: {StatusCode}", message.ToAddress, response.StatusCode);
            }, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deliver email to {To}", message.ToAddress);
            // Do not rethrow — email failure is non-fatal
        }
    }
}
```

Register in service collection:
```csharp
services.AddSingleton<IEmailService, SendGridEmailService>();
```

## Acceptance Criteria

- [ ] `SendGridEmailService` implements `IEmailService`
- [ ] Polly retry: 3 attempts, 2-second delay
- [ ] Delivery failure logs error but does not throw
- [ ] `IEmailService` registered in DI
- [ ] `dotnet build` exits with code 0

## Notes

API key must be in `appsettings.Development.json` (git-ignored). See action-required.md for setup steps.
