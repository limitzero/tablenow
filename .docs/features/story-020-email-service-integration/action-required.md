# Action Required: Email Service Integration

Manual steps that must be completed by a human. These cannot be automated.

## Before Implementation

- [ ] **Create an email provider account** — Sign up for SendGrid or Mailgun (whichever the team standardizes on). Required to obtain sending credentials.
- [ ] **Verify a sender identity / domain** — Configure a verified single-sender address or authenticated domain in the provider dashboard. Required for the provider to accept outbound mail.
- [ ] **Generate an API key** — Create a transactional-send API key in the provider dashboard. This is the secret the service authenticates with.

## During Implementation

- [ ] **Set the API key in environment configuration** — Provide the key via environment variable / user secrets (e.g. `Email__ApiKey`), never committed to source. Also set `Email__FromAddress` and `Email__FromName`. Required so the bound `EmailOptions` resolves at runtime.

## After Implementation

- [ ] **Send a test email** — Trigger a send (via a temporary endpoint or integration test) to confirm the API key, sender identity, and provider configuration are correct end-to-end.

---

> These tasks are also referenced in context within the relevant task files.
