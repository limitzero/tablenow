# Requirements: Database Schema & EF Core Migrations

## Summary

TableNow needs four core tables: `Users`, `Restaurants`, `TimeSlots`, and `Reservations`. The architecture separates domain entity classes (in Domain projects, no EF annotations) from EF Fluent API configurations (in Data projects). This prevents domain models from becoming entangled with persistence concerns.

The `TimeSlot` entity requires a row-version concurrency token (`RowVersion byte[]`) so that two simultaneous reservation attempts for the same slot trigger a `DbUpdateConcurrencyException` — the foundation for the double-booking prevention in STORY-014.

Migrations run via the `server/src/Migrations/` project. The initial migration produces the full schema and must apply cleanly on both SQLite (development) and SQL Server (production).

## Goals

- Four domain entity classes with no EF attributes
- Fluent API configurations for all four entities
- `AppDbContext` with `OnModelCreating` using `ApplyConfigurationsFromAssembly`
- `TimeSlot.RowVersion` configured as a concurrency token via `IsRowVersion()`
- `dotnet ef database update` runs without errors on SQLite dev

## Non-Goals

- Seed data (STORY-004)
- Additional entities for reviews, photos, favorites (STORY-023, 025, 028)
- EF migrations for those later entities

## Acceptance Criteria

- [ ] `User`, `Restaurant`, `TimeSlot`, `Reservation` domain entities exist with correct properties
- [ ] No EF attributes on domain entities (only Fluent API)
- [ ] `AppDbContext` uses `ApplyConfigurationsFromAssembly` in `OnModelCreating`
- [ ] `TimeSlot.RowVersion` is configured as `IsRowVersion()` (optimistic concurrency)
- [ ] `dotnet ef database update` from the Migrations project applies without errors

## Assumptions

- STORY-001 is complete (solution structure exists)
- SQL Server is available for production; SQLite used for local dev
- `ReminderSent` column added to Reservation in STORY-022 (not here)

## Technical Constraints

- Domain entities in `Domain/<Context>/` — pure C# classes
- EF model configurations in `Data/<Context>/Configurations/`
- `OnModelCreating` must use `ApplyConfigurationsFromAssembly` — do not configure entities inline
- Use Fluent API only — no `[Key]`, `[Required]`, `[MaxLength]` data annotations on domain entities
- Soft FK relationships via `HasOne`/`HasForeignKey` in configurations
