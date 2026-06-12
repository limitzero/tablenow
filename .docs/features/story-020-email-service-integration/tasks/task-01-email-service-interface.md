# Task 01: Email Service Interface

## Status

pending

## Wave

1

## Description

Define the provider-agnostic email contract for TableNow. This task creates the `IEmailService` interface, the `EmailAttachment` value type, and the `EmailOptions` configuration class inside the `Infrastructure/Notifications/` module. Every downstream notification feature (booking confirmation, reminder emails) depends only on this interface, so it must be free of any provider-specific types. This is the foundational contract that the concrete provider implementation (task-02) fulfills.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-02-email-provider-implementation

**Context from dependencies:** This is a foundational task. STORY-007 has already established the `Api` project, `appsettings.json`, and `ServiceCollectionExtensions.RegisterServices()` wiring. No other notification code exists yet — this task introduces the `Infrastructure/Notifications/` module's public surface.

## Files to Create

- `server/src/Infrastructure/Notifications/IEmailService.cs` — the email-sending contract.
- `server/src/Infrastructure/Notifications/EmailAttachment.cs` — record describing a single email attachment (used later for `.ics` files).
- `server/src/Infrastructure/Notifications/EmailOptions.cs` — strongly-typed configuration bound from the `Email` section.

## Files to Modify

- `server/src/Api/appsettings.json` — add an `Email` configuration section with non-secret defaults (no API key value committed).

## Technical Details

### Implementation Steps

1. If the `Infrastructure/Notifications/` project/folder does not yet exist, create it as a class library project under `server/src/Infrastructure/` following the modular monolith layout. Reference it from the `Api` project.
2. Create the `IEmailService` interface with a single async method. All parameters are primitives or framework types — no provider types leak through.
3. Create the `EmailAttachment` record so callers (STORY-021) can attach `.ics` files.
4. Create the `EmailOptions` class bound from the `Email` section.
5. Add a non-secret `Email` section to `appsettings.json`. The `ApiKey` is left empty and supplied via environment configuration at runtime.

### Code Snippets

`IEmailService.cs`:

```csharp
namespace CM.OpenTable.Infrastructure.Notifications;

public interface IEmailService
{
    Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        IReadOnlyCollection<EmailAttachment>? attachments = null,
        CancellationToken cancellationToken = default);
}
```

`EmailAttachment.cs`:

```csharp
namespace CM.OpenTable.Infrastructure.Notifications;

public sealed record EmailAttachment(
    string FileName,
    string ContentType,
    byte[] Content);
```

`EmailOptions.cs`:

```csharp
namespace CM.OpenTable.Infrastructure.Notifications;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Provider { get; set; } = "SendGrid";
    public string ApiKey { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "TableNow";
}
```

`appsettings.json` addition (no secret committed):

```json
"Email": {
  "Provider": "SendGrid",
  "ApiKey": "",
  "FromAddress": "no-reply@tablenow.example",
  "FromName": "TableNow"
}
```

## Acceptance Criteria

- [ ] `IEmailService` exists in `Infrastructure/Notifications/` with the `SendAsync(to, subject, htmlBody, attachments?, CancellationToken)` signature.
- [ ] `EmailAttachment` record exists with `FileName`, `ContentType`, and `Content` (`byte[]`).
- [ ] `EmailOptions` exists with `Provider`, `ApiKey`, `FromAddress`, `FromName` and a `SectionName` constant.
- [ ] `appsettings.json` contains an `Email` section with an empty `ApiKey` (no secret committed).
- [ ] The interface exposes no provider-specific types; `dotnet build` succeeds.

## Notes

- The actual `ApiKey` value is provided via environment configuration (e.g. `Email__ApiKey`) — see action-required.md. Do not commit a real key.
- Confirm the root namespace prefix matches the solution's convention (`CM.OpenTable.*` per CLAUDE.md). Adjust if the scaffolded solution uses a different `Company.Product` prefix.
