# Task 02: Reminder Template & Registration

## Status

pending

## Wave

1

## Description

Creates the HTML reminder email template and registers `ReminderBackgroundService` in `NotificationsModuleRegistration`. Parallel to task-01 — different files.

## Dependencies

**Depends on:** None (Wave 1 — parallel with task-01)
**Blocks:** Nothing

**Context from dependencies:** STORY-020 task-02 created `NotificationsModuleRegistration.AddNotificationsModule()`. This task adds the hosted service registration to that method and creates the template file. Does not overlap with task-01.

## Files to Create

- `server/src/Infrastructure/Notifications/Templates/Reminder.html`

## Files to Modify

- `server/src/Infrastructure/Notifications/NotificationsModuleRegistration.cs` — add `AddHostedService<ReminderBackgroundService>()`

## Technical Details

### Code Snippets

```html
<!-- Templates/Reminder.html -->
<!DOCTYPE html>
<html>
<head><meta charset="utf-8"><title>Reservation Reminder</title></head>
<body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
  <h1 style="color: #333;">Reminder: Your reservation is tomorrow!</h1>
  <p>Just a friendly reminder about your upcoming reservation:</p>
  <table>
    <tr><td><strong>Restaurant:</strong></td><td>{{RestaurantName}}</td></tr>
    <tr><td><strong>Date:</strong></td><td>{{Date}}</td></tr>
    <tr><td><strong>Time:</strong></td><td>{{Time}}</td></tr>
    <tr><td><strong>Party size:</strong></td><td>{{PartySize}}</td></tr>
    <tr><td><strong>Address:</strong></td><td>{{Address}}</td></tr>
  </table>
  <p><a href="{{GoogleMapsUrl}}">Get directions on Google Maps</a></p>
  <p style="color: #666;">We look forward to seeing you!</p>
</body>
</html>
```

```csharp
// Update NotificationsModuleRegistration.cs:
public static IServiceCollection AddNotificationsModule(
    this IServiceCollection services, IConfiguration configuration)
{
    services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
    services.AddSingleton<IEmailService, SendGridEmailService>();
    services.AddSingleton<EmailTemplateRenderer>();
    services.AddHostedService<ReminderBackgroundService>(); // NEW
    return services;
}
```

## Acceptance Criteria

- [ ] `Reminder.html` template exists with `{{RestaurantName}}`, `{{Date}}`, `{{Time}}`, `{{PartySize}}`, `{{Address}}`, `{{GoogleMapsUrl}}` placeholders
- [ ] `ReminderBackgroundService` registered via `AddHostedService` in `AddNotificationsModule`
