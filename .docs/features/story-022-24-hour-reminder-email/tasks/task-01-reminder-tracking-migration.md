# Task 01: Reminder Tracking Migration

## Status

pending

## Wave

1

## Description

Add a nullable `ReminderSentAt` timestamp to the `Reservation` entity so the reminder background service can record which reservations have already been reminded and avoid sending duplicate emails. This task updates the domain entity, the EF model and Fluent configuration, and creates the EF migration that adds the column.

## Dependencies

**Depends on:** None within this story (depends on STORY-003 schema and STORY-014 reservation flow being in place)
**Blocks:** task-02-reminder-background-service

**Context from dependencies:** STORY-003 established the EF code-first schema, `OnModelCreating` with `ApplyConfigurationsFromAssembly`, and the `Migrations` project. The `Reservation` entity (domain in `Domain/Reservations/`, EF model in `Data/Reservations/Models/`, configuration in `Data/Reservations/Configurations/`) already has `Status`, `PartySize`, slot/restaurant relationships, etc. This task adds one nullable column used by the reminder job in task-02.

## Files to Modify

- `server/src/Domain/Reservations/Reservation.cs` — add `ReminderSentAt` (nullable).
- `server/src/Data/Reservations/Models/ReservationModel.cs` (or the EF model file for Reservation) — add the mapped property if the project keeps a separate EF model. Per CLAUDE.md, the project uses one model per entity in `Data/Models/` with Fluent API; add the property where the mapped Reservation type lives.
- `server/src/Data/Reservations/Configurations/ReservationConfiguration.cs` — configure the column if non-default mapping is desired (nullable is the default for `DateTime?`).

## Files to Create

- `server/src/Migrations/...` — the generated migration adding `ReminderSentAt` (created via the EF CLI, not hand-authored).

## Technical Details

### Implementation Steps

1. Add a nullable timestamp to the `Reservation` entity. Prefer `DateTimeOffset?` if the codebase stores times as `DateTimeOffset`; otherwise `DateTime?` (UTC).

   ```csharp
   public DateTimeOffset? ReminderSentAt { get; set; }
   ```

2. If the project maps a separate EF model type for `Reservation`, mirror the property there. (CLAUDE.md: do not split into Persistence + Domain models — one entity, Fluent API config. Follow whatever the existing Reservation type does.)
3. The default mapping for a nullable type produces a nullable column, so no explicit Fluent config is strictly required. Optionally add documentation in `ReservationConfiguration`:

   ```csharp
   builder.Property(r => r.ReminderSentAt); // nullable: null until a reminder email is successfully sent
   ```

4. Create the migration from the `server/` root:

   ```powershell
   dotnet ef migrations add AddReservationReminderSentAt --project server/src/Migrations/<MigrationsProject>.csproj --startup-project server/src/Api/<ApiProject>.csproj
   ```

5. Inspect the generated migration to confirm it only adds a nullable `ReminderSentAt` column to the `Reservations` table.
6. Apply it in dev to verify:

   ```powershell
   dotnet ef database update --project server/src/Migrations/<MigrationsProject>.csproj --startup-project server/src/Api/<ApiProject>.csproj
   ```

## Acceptance Criteria

- [ ] `Reservation` has a nullable `ReminderSentAt` property.
- [ ] A migration exists that adds a nullable `ReminderSentAt` column to the `Reservations` table.
- [ ] `dotnet ef database update` applies the migration without errors on the dev database.
- [ ] Existing reservations have `ReminderSentAt = NULL` after migration.
- [ ] `dotnet build` succeeds.

## Notes

- Keep the column nullable — `NULL` is the "not yet reminded" sentinel that the task-02 query filters on.
- Match the project's chosen time type (`DateTime` UTC vs `DateTimeOffset`) for consistency with the slot/created timestamps.
