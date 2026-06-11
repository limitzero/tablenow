# Requirements: Database Seed Data

## Summary

Provides realistic demo data so the MVP is immediately usable after setup. The seed includes 15+ restaurants with varied cuisine types, 30 days of future time slots with multiple slots per day per restaurant, and 2 test user accounts with known credentials for login testing.

## Goals

- At least 15 restaurants across 3–5 cuisine types seeded after `dotnet ef database update`
- Each restaurant has 30 days of future time slots, multiple slots per day (e.g., 18:00, 18:30, 19:00, 19:30, 20:00)
- At least 2 test user accounts with known credentials
- One test slot per restaurant has `RemainingCapacity = 0` to test the filtering logic

## Non-Goals

- No UI or API for managing seed data
- No seed data for Reviews, Photos, or Favorites (Phase 3/4 features)

## Acceptance Criteria

- [ ] 15+ restaurants across 3–5 cuisines exist after seeding
- [ ] Each restaurant has 30 days × 5 slots = 150 slots per restaurant
- [ ] At least 2 test users exist with known passwords (see Technical Notes in task files)
- [ ] At least 1 slot per restaurant has `RemainingCapacity = 0`

## Assumptions

- Seed runs idempotently — re-running does not create duplicates
- Seed data uses deterministic GUIDs so it is repeatable across dev machines

## Technical Constraints

- Seed logic lives in the Data layer, not in the domain
- Uses `Bogus` NuGet package for realistic fake data generation (or custom static data)
- Runs at application startup via `IHostedService` or via `HasData` in EF migrations
