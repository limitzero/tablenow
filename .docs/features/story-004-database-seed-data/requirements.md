# Requirements: Database Seed Data

## Summary

TableNow needs realistic demo data in the database from the moment migrations are applied, so the MVP can be demonstrated to stakeholders without any manual setup. This story adds an idempotent seed mechanism that populates users, restaurants, and time slots as part of the application startup or migration step.

The seed data must cover enough variety to exercise the full reservation flow: multiple cuisine types, restaurants with differing capacities, future time slots spread across 30 days, and at least one zero-capacity slot to verify the availability filter. Test user accounts with known credentials let developers and testers sign in immediately.

The expected outcome is that running `dotnet ef database update` (or starting the app for the first time against a fresh database) produces a fully populated, demo-ready environment.

## Goals

- At least 15 restaurants seeded across 3–5 cuisine types (e.g., Italian, Japanese, Mexican, American, Indian).
- Each restaurant has 30 days of future time slots with multiple slots per day (e.g., 12:00, 13:00, 18:00, 19:00, 20:00).
- At least 2 test user accounts with known email/password combinations (hashed with BCrypt).
- At least one seeded slot with `RemainingCapacity = 0` to verify the availability filter excludes it.
- Seed is idempotent — running it twice does not duplicate records.

## Non-Goals

- No production data — seed is development and demo only.
- No admin UI for managing seed data.
- No reviews, photos, or favorites seed data (Phase 3/4 stories).
- No complex scheduling patterns — simple fixed daily slot times are sufficient.

## Acceptance Criteria

- [ ] Running `dotnet ef database update` results in ≥ 15 restaurants with ≥ 3 cuisine types present.
- [ ] Each restaurant has time slots for the next 30 days (multiple slots per day).
- [ ] At least 2 test users exist with known email/password credentials.
- [ ] At least one slot with `RemainingCapacity = 0` exists and is excluded by the slot availability query.
- [ ] Running seed logic twice does not create duplicate records (idempotent).

## Assumptions

- STORY-003 has created all required EF models (`User`, `Restaurant`, `TimeSlot`) and migrations.
- BCrypt password hashing from STORY-005's `BCrypt.Net-Next` package is available for hashing test user passwords.
- The seed runs before any feature tests to guarantee a populated database.
- Slot times are stored in UTC.

## Technical Constraints

- Seed implementation lives in `Data/<Context>/` — either via `HasData` in EF configurations or a dedicated startup `IHostedService` / extension method.
- Use `BCrypt.Net-Next` (work factor ≥ 12) for test user password hashes.
- Restaurant fields: `Name`, `Cuisine`, `Address` (Street, City, Region, PostalCode), `Description`, `ThumbnailUrl`.
- Time slots use fixed times per day; `TotalCapacity` and initial `RemainingCapacity` may vary (e.g., 20 for lunch, 30 for dinner).
- Seed must not throw if data already exists — check before inserting or use `HasData` with stable GUIDs.
