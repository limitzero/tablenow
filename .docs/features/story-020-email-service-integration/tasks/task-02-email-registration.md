# Task 02: Email Registration & Config

## Status

pending

## Wave

2

## Description

Creates `EmailOptions`, updates `NotificationsModuleRegistration` to register `IEmailService`, and adds the Email config section to `appsettings.json`.

## Dependencies

**Depends on:** task-01-email-service-abstraction.md
**Blocks:** STORY-021 (confirmation email), STORY-022 (reminder email)

**Context from dependencies:** task-01 created `SendGridEmailService` that depends on `IOptions<EmailOptions>`. This task creates `EmailOptions` and wires up DI so the options are injected.

## Files to Create

- `server/src/Infrastructure/Notifications/EmailOptions.cs`

## Files to Modify

- `server/src/Infrastructure/Notifications/NotificationsModuleRegistration.cs`
- `server/src/Api/appsettings.json`

## Technical Details

### Code Snippets

```csharp
// EmailOptions.cs
namespace TableNow.Infrastructure.Notifications;
public class EmailOptions
{
    public const string SectionName = "Email";
    public string ApiKey { get; init; } = string.Empty;
    public string FromAddress { get; init; } = "noreply@tablenow.com";
    public string FromName { get; init; } = "TableNow";
}
```

```csharp
// NotificationsModuleRegistration.cs
public static IServiceCollection AddNotificationsModule(
    this IServiceCollection services, IConfiguration configuration)
{
    services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
    services.AddSingleton<IEmailService, SendGridEmailService>();
    // ReminderBackgroundService added in STORY-022
    return services;
}
```

### Environment Variables

- `Email__ApiKey` — SendGrid API key (double-underscore = nested config section in .NET)

## Acceptance Criteria

- [ ] `EmailOptions` binds from `appsettings.json:Email` section
- [ ] `IEmailService` registered as singleton
- [ ] `Email:ApiKey` in `appsettings.json` has placeholder value only
