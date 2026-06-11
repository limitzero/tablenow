# Action Required: User Sign-In — Backend

## Before Implementation

- [ ] **Set JWT secret in development config** — Create or update `appsettings.Development.json` (git-ignored) with a JWT secret of at least 32 characters:
  ```json
  {
    "Jwt": {
      "Secret": "dev-secret-at-least-32-characters-long-for-hmac"
    }
  }
  ```
  This file is never committed. Each developer must create it manually.

---

> This step is also noted in task-01-jwt-infrastructure.md.
