# Requirements: Database Schema & EF Core Migrations

## Summary

Defines the four core domain entities — User, Restaurant, TimeSlot, and Reservation — and the corresponding EF Core models with Fluent API configurations. Domain entities are plain C# classes with no EF annotations; all database mapping is done via Fluent API in separate configuration classes.

The `TimeSlot` entity requires a concurrency token (`[Timestamp]` or `IsConcurrencyToken`) to support optimistic concurrency for the double-booking prevention feature in STORY-014. This is a critical schema detail that must be in place before the reservation creation logic is built.

After this story, `dotnet ef database update` applies the full schema and `dotnet build` still passes across the solution.

## Goals

- Four core tables created with correct columns and foreign keys
- Domain entities are EF-annotation free (Fluent API only)
- `TimeSlot.RemainingCapacity` and a concurrency token are present on the TimeSlot table
- `OnModelCreating` uses `ApplyConfigurationsFromAssembly` — no inline mappings
- Migrations project exists and the initial migration is created

## Non-Goals

- No seed data (STORY-004)
- No Review or Photo entities (STORY-023, STORY-025)
- No UserFavorite entity (STORY-028)
- No stored procedures or views

## Acceptance Criteria

- [ ] `User`, `Restaurant`, `TimeSlot`, `Reservation` tables exist with correct columns and foreign keys after `dotnet ef database update`
- [ ] `TimeSlot` has `RemainingCapacity` (int) and a concurrency token column
- [ ] Domain entities in `Domain/<Context>/` have no EF attributes
- [ ] Fluent API configurations live in `Data/<Context>/Configurations/`
- [ ] `OnModelCreating` calls `ApplyConfigurationsFromAssembly` for each context's DbContext
- [ ] `dotnet build` still exits with code 0 after migrations are added

## Assumptions

- SQLite is used for local development (`Microsoft.EntityFrameworkCore.Sqlite`)
- SQL Server is used for production (`Microsoft.EntityFrameworkCore.SqlServer`)
- Concurrency token approach: `[Timestamp]` byte array row version (compatible with both SQLite and SQL Server)
- One DbContext per business context (Auth, Restaurants, Reservations)

## Technical Constraints

- EF Core 10 — use latest API conventions
- No EF annotations on domain entities — Fluent API only
- No split entity model (one EF model per entity, lives in `Data/<Context>/Models/`)
- No repository pattern — DbContext used directly in Data handlers
- File-scoped namespaces, nullable enabled
