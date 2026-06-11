# Action Required: User Sign-In — Backend

## Before Implementation

- [ ] **Generate a JWT secret** — A random 256-bit (32-byte) base64 string for signing tokens. Do not commit this value. Set it via the environment variable `Jwt__Secret` in your local development environment or secrets manager.

## After Implementation

- [ ] **Configure production secret** — Set the `Jwt__Secret` environment variable in the production host before deploying. The placeholder in `appsettings.json` must never be used in production.

---

> The JWT secret must be set externally; the `appsettings.json` file must only contain the placeholder `"SET_VIA_ENVIRONMENT_VARIABLE"`.
