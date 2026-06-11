# Action Required: Email Service Integration

## Before Implementation

- [ ] **Create a SendGrid or Mailgun account** — Free tiers are available. Obtain an API key.
- [ ] **Add API key to environment config** — Add to `appsettings.Development.json` (git-ignored):
  ```json
  {
    "Email": {
      "Provider": "SendGrid",
      "ApiKey": "SG.your-api-key-here",
      "FromAddress": "noreply@tablenow.dev",
      "FromName": "TableNow"
    }
  }
  ```

---

> Referenced in task-02-email-provider.md.
