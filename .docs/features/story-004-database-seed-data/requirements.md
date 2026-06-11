# Requirements: Database Seed Data

## Summary

The MVP must be demonstrable immediately after setup — no manual data entry. Seed data covers restaurants with realistic names and descriptions, 30 days of future time slots at standard meal times, and two test accounts (diner and operator roles). A slot with zero remaining capacity is seeded to enable testing of the booking conflict flow.

## Goals

- 15+ restaurants across 5 cuisine types (Italian, Mexican, Japanese, American, French)
- Each restaurant has 30 days of future time slots at 7 times per day
- At least one slot per restaurant with `RemainingCapacity = 0`
- Two test users: `diner@test.com` and `operator@test.com` (both with `P@ssw0rd123`)
- Passwords hashed with BCrypt work factor ≥ 12

## Non-Goals

- Production-quality data (this is dev/demo seed only)
- Seeding reviews, photos, or favorites (later stories)

## Acceptance Criteria

- [ ] At least 15 restaurants across 3–5 cuisine types after `dotnet ef database update`
- [ ] Each restaurant has 30 days of future time slots (multiple per day)
- [ ] `diner@test.com` and `operator@test.com` exist with hashed passwords
- [ ] At least one slot exists with `RemainingCapacity = 0`
- [ ] Seed runs idempotently (re-running doesn't duplicate data)

## Technical Constraints

- Seed logic runs via `IHostedService` on startup (check-and-skip if data already exists)
- Use `Bogus` NuGet package for realistic fake data generation
- BCrypt work factor must be 12 for all password hashes
