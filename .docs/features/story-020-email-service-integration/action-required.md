# Action Required: Email Service Integration

## Before Implementation

- [ ] **Create a SendGrid account** — Sign up at https://sendgrid.com. Free tier is sufficient for dev.
- [ ] **Generate an API key** — In SendGrid dashboard: Settings → API Keys → Create API Key (Full Access or Restricted with Mail Send permission).
- [ ] **Verify sender** — In SendGrid: Settings → Sender Authentication. Verify `noreply@tablenow.com` or use your own domain.

## After Implementation

- [ ] **Set the API key** — In your local environment set `Email__ApiKey=<your-key>`. For production, set it in the host's environment variable store (never commit the key).

---

> The `appsettings.json` placeholder `"SET_VIA_ENVIRONMENT_VARIABLE"` must never be replaced with a real key in source code.
