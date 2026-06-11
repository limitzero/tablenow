# Action Required: TableNow MVP

Manual steps that must be completed by a human. These cannot be automated by agents.

## Before Implementation

- [ ] **Install .NET 10 SDK** — Verify with `dotnet --version`; must be ≥ 10.0. Download from https://dotnet.microsoft.com/download.
- [ ] **Install Node.js 20+ and Angular CLI 21** — Verify with `node --version` and `ng version`. Install CLI: `npm install -g @angular/cli@21`.
- [ ] **Install EF Core CLI tools** — Run `dotnet tool install --global dotnet-ef` (or update: `dotnet tool update --global dotnet-ef`).
- [ ] **Set `JWT__Secret` environment variable** — Must be set before running the API. Minimum 32 characters. Example (PowerShell): `$env:JWT__Secret = "your-super-secret-key-at-least-32-chars"`. Add to `launchSettings.json` for local dev (never commit).
- [ ] **SQL Server or SQLite** — For local dev SQLite requires no setup. For SQL Server, ensure a local instance is running and update `ConnectionStrings__DefaultConnection` accordingly.

## During Implementation

- [ ] **Run initial EF migration** — After task-03 (database schema) is complete, run: `dotnet ef migrations add InitialCreate --project server/src/Migrations/TableNow.Migrations` to generate the migration file.
- [ ] **Apply migration** — Run `dotnet ef database update --project server/src/Migrations/TableNow.Migrations` to create the schema and trigger seed data.

## After Implementation

- [ ] **Verify E2E flow in browser** — Start both servers (`dotnet run` from `server/src/Api` + `ng serve` from `client/`) and manually complete the full flow: register → browse → book → view dashboard → cancel.
- [ ] **Run full test suite** — `dotnet test` from `server/` and `npm run test` from `client/` must both pass with zero failures.

---

> These tasks are also referenced in context within the relevant task files.
