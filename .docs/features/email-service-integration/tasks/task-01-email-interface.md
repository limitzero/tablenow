# Task 01: IEmailService Interface & Registration

## Status

pending

## Wave

1

## Description

Defines `IEmailService` and `EmailMessage` in `Infrastructure/Notifications/` and registers the abstraction in DI. This task creates the interface only — the concrete provider implementation is task-02.

## Dependencies

**Depends on:** STORY-001 task-01-solution-projects.md
**Blocks:** task-02-email-provider.md, STORY-021

**Context from dependencies:** `CM.TableNow.Auth.Infrastructure.csproj` or a new `CM.TableNow.Notifications.Infrastructure.csproj` is the home for this code. STORY-001 created `Infrastructure/` projects per context.

## Files to Create

- `server/src/Infrastructure/CM.TableNow.Auth.Infrastructure/Notifications/IEmailService.cs`
- `server/src/Infrastructure/CM.TableNow.Auth.Infrastructure/Notifications/EmailMessage.cs`
- `server/src/Infrastructure/CM.TableNow.Auth.Infrastructure/Notifications/EmailOptions.cs`

## Technical Details

### Code Snippets

```csharp
// EmailMessage.cs
namespace CM.TableNow.Auth.Infrastructure.Notifications;

public record EmailMessage(
    string ToAddress,
    string ToName,
    string Subject,
    string HtmlBody,
    IReadOnlyList<EmailAttachment>? Attachments = null);

public record EmailAttachment(string FileName, byte[] Content, string MimeType);
```

```csharp
// IEmailService.cs
namespace CM.TableNow.Auth.Infrastructure.Notifications;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
```

```csharp
// EmailOptions.cs
namespace CM.TableNow.Auth.Infrastructure.Notifications;

public class EmailOptions
{
    public const string SectionName = "Email";
    public string Provider { get; set; } = "SendGrid";
    public string ApiKey { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "TableNow";
}
```

Register in the Auth Infrastructure `ServiceCollectionExtensions`:
```csharp
services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
// Concrete implementation registered in task-02
```

## Acceptance Criteria

- [ ] `IEmailService`, `EmailMessage`, `EmailOptions` exist
- [ ] `EmailAttachment` for `.ics` and other attachments
- [ ] `dotnet build` exits with code 0
