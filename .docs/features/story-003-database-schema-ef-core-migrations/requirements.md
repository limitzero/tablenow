# Requirements: Database Schema & EF Core Migrations

## Summary

TableNow needs a persistent data model for users, restaurants, time slots, and reservations before any feature can store or query data. This story defines those four core entities as code-first EF Core models and produces an initial migration that creates the schema repeatably.

Following the project's architecture, domain entities live in each context's `Domain/<Context>/` project and carry **no** EF annotations — they are pure business types. The EF mapping lives separately in each context's `Data/<Context>/Models/` and `Configurations/` folders using the Fluent API, registered via `ApplyConfigurationsFromAssembly` in each `DbContext`. The `TimeSlot` entity gets a `RemainingCapacity` column plus an optimistic-concurrency token (row version) so the later reservation-creation story can decrement capacity atomically and prevent double-booking.

The expected outcome is that `dotnet ef database update` applies the schema without errors against SQLite (local dev) and SQL Server (prod), producing `Users`, `Restaurants`, `TimeSlots`, and `Reservations` tables with correct columns and foreign keys.

## Goals

- Domain entities `User`, `Restaurant`, `TimeSlot`, and `Reservation` in the appropriate `Domain/<Context>/` projects, with value objects where natural and no EF attributes.
- EF models in `Data/<Context>/Models/` and Fluent API configurations in `Data/<Context>/Configurations/`.
- A `DbContext` per business context whose `OnModelCreating` uses `ApplyConfigurationsFromAssembly`.
- A `RemainingCapacity` column on `TimeSlot` plus an optimistic-concurrency token (row version / timestamp).
- A migrations project under `server/src/Migrations/` and an initial migration covering all entities.
- `dotnet ef database update` applies the schema cleanly against SQLite (dev) and SQL Server (prod).

## Non-Goals

- No seed data (covered by STORY-004).
- No query/command handlers or endpoints (covered by STORY-005, 010, 011, 014+).
- No reviews, photos, favorites entities (Phase 3/4 stories).
- No business logic for capacity decrement — only the schema and concurrency token are established here.

## Acceptance Criteria

- [ ] `User`, `Restaurant`, `TimeSlot`, and `Reservation` tables exist with correct columns and foreign keys.
- [ ] `dotnet ef database update` applies the schema without errors against SQLite (dev) and SQL Server (prod).
- [ ] `TimeSlot` has a `RemainingCapacity` column and a concurrency token (row version or timestamp).
- [ ] `OnModelCreating` uses `ApplyConfigurationsFromAssembly` with Fluent API configurations in `Configurations/`.
- [ ] Domain entities carry no EF annotations.

## Assumptions

- STORY-001 has produced the solution, the per-context `Domain` and `Data` projects, and the `Migrations` project shells.
- Contexts: `User` → Auth context; `Restaurant` and `TimeSlot` → Restaurants context; `Reservation` → Reservations context.
- The dev provider is SQLite and the prod provider is SQL Server; the concurrency token must work on both (SQLite uses a manually-managed row-version column; SQL Server can use `rowversion`).
- Connection strings come from configuration established in STORY-001 (`ConnectionStrings:Default`).

## Technical Constraints

- One EF model per entity in `Data/<Context>/Models/`; Fluent API only in `Configurations/`; no `[Attribute]` mappings on domain entities.
- Do not split an entity into separate Persistence + Domain models with divergent shapes — the EF model maps the domain entity; keep them aligned per CLAUDE.md ("Do not split EF entity into Persistence + Domain models").
- No repository pattern — DbContext is used directly by Data handlers (later stories).
- The concurrency token must be cross-provider: prefer a `byte[] RowVersion` configured `.IsRowVersion()` for SQL Server and `.IsConcurrencyToken()` with a value generator/converter strategy that also functions on SQLite, OR a `Guid`/`long` version column updated on save. Document the chosen approach in the migration.
- File-scoped namespaces, nullable enabled, records for value objects where appropriate.
- `Reservation.Status` is an enum with values `Confirmed` and `Cancelled`.
