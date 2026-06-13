# Implementation Plan: Database Seed Data

## Phase 1 — Users and Restaurants

Seeds the foundational records that time slots depend on.

- [ ] **task-01-seed-users-restaurants** — Hash test user passwords with BCrypt and insert `User` records; insert ≥ 15 `Restaurant` records across ≥ 3 cuisine types using stable `Guid` identifiers.

### Technical Details

- Use `BCrypt.Net-Next` with work factor 12 to hash test passwords before inserting.
- Use stable, hard-coded GUIDs for seed records to support idempotent `HasData` or existence checks.
- Restaurant fields: `Name`, `Cuisine`, `Address`, `Description`, `ThumbnailUrl`.
- Cuisine types to cover: Italian, Japanese, Mexican, American, Indian (minimum 3).

## Phase 2 — Time Slots

Runs after Phase 1; requires restaurant GUIDs from the previous phase.

- [ ] **task-02-seed-time-slots** — For each seeded restaurant, generate time slots for the next 30 days with multiple times per day (12:00, 13:00, 18:00, 19:00, 20:00 UTC). Seed at least one slot with `RemainingCapacity = 0`.

### Technical Details

- Loop from `DateTimeOffset.UtcNow.Date` through `+30 days`, creating slots at the fixed UTC times.
- `TotalCapacity` and `RemainingCapacity` initialized equally (e.g., 20 for lunch, 30 for dinner); one slot gets `RemainingCapacity = 0`.
- Use stable GUIDs or existence checks to keep the seed idempotent.
