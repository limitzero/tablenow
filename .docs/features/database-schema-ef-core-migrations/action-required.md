# Action Required: Database Schema & EF Core Migrations

## Before Implementation

- [ ] **Complete STORY-001 (Backend Scaffolding)** — The solution and project files must exist before EF entities and migrations can be added.

## After Implementation

- [ ] **Run `dotnet ef database update`** — Apply the initial migration to create the local SQLite database. This is a manual step as the command requires a running connection string. Run from `./server`:
  ```powershell
  dotnet ef database update --project src/Migrations/CM.TableNow.Auth.Migrations --startup-project src/Api
  dotnet ef database update --project src/Migrations/CM.TableNow.Restaurants.Migrations --startup-project src/Api
  dotnet ef database update --project src/Migrations/CM.TableNow.Reservations.Migrations --startup-project src/Api
  ```

---

> These tasks are also referenced in context within the relevant task files.
