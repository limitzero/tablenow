# Implementation Plan: Database Schema & EF Core Migrations

## Phase 1 — Model definition (parallel)

task-01 defines the pure domain entities; task-02 defines the EF mapping. They touch different projects (`Domain/*` vs `Data/*`). task-02 references the domain types task-01 produces, so if both are dispatched in parallel, task-02 must align its EF models to the domain shapes specified in task-01's file (the shapes are fully documented in both tasks to keep them self-contained).

- [ ] **task-01-domain-entities** — Create `User` (Auth), `Restaurant` + `TimeSlot` (Restaurants), and `Reservation` (Reservations) domain entities with no EF annotations. Use value objects where natural (e.g., `Address`, `Email`). Define the `ReservationStatus` enum (`Confirmed`, `Cancelled`).
- [ ] **task-02-ef-models-configs** — Create EF models in `Data/<Context>/Models/`, Fluent API `IEntityTypeConfiguration<>` classes in `Data/<Context>/Configurations/`, and one `DbContext` per context. Each `DbContext.OnModelCreating` calls `ApplyConfigurationsFromAssembly`. Configure foreign keys, indexes, and the `TimeSlot.RemainingCapacity` column plus a cross-provider optimistic-concurrency token (`RowVersion`).

## Phase 2 — Migrations

Runs after Phase 1. Requires the DbContexts and configurations from task-02 and the domain entities from task-01.

- [ ] **task-03-migrations-project** — Configure the migrations project(s) under `server/src/Migrations/`, register the EF design-time factory and provider selection (SQLite for dev, SQL Server for prod), generate the initial migration covering all four entities, and verify `dotnet ef database update` applies the schema cleanly against both providers.
