# Requirements: Slot Availability — Backend

## Summary

Diners need to see only the time slots at a restaurant where their party actually fits. Without server-side capacity filtering, the frontend would have to either load every slot and discard most of them or show unavailable slots and let the booking fail later. This feature adds a dedicated read endpoint that takes a date and party size and returns only qualifying slots from the database, keeping the payload small and the query fast.

The endpoint sits on the existing Restaurants module (`GET /api/restaurants/{id}/slots?date=&partySize=`) and requires no authentication — any visitor can check availability before deciding to register. Validation is inline: a missing or non-positive `partySize`, or a missing `date`, returns a 400 with structured problem details.

## Goals

- Return only `TimeSlot` rows where `RemainingCapacity >= partySize` for the requested date, filtered entirely in the database via EF LINQ.
- Respond in under 300 ms on the happy path.
- Return 400 with structured validation errors when `date` or `partySize` are missing or invalid.
- Expose each slot as `{ slotId, time, remainingCapacity }`.

## Non-Goals

- Pagination — all qualifying slots for a single date are returned in one response.
- Authentication — this endpoint is intentionally public.
- Slot creation, update, or deletion — read-only.
- Timezone conversion — `StartTime` is stored and returned as UTC `DateTimeOffset`; clients handle display formatting.

## Acceptance Criteria

- [ ] `GET /api/restaurants/{id}/slots?date=2026-06-20&partySize=4` returns 200 with only slots where `RemainingCapacity >= 4` on that date.
- [ ] A slot with `RemainingCapacity = 0` is excluded regardless of the requested `partySize`.
- [ ] Missing or invalid `date` or `partySize` returns 400 with structured validation problem details.
- [ ] Each slot in the response includes `slotId`, `time` (DateTimeOffset), and `remainingCapacity`.
- [ ] The EF query filters in SQL — no full table load and in-memory filter.
- [ ] BDD unit tests cover `when_party_size_exceeds_remaining_capacity` and `when_slot_has_sufficient_capacity`.

## Assumptions

- STORY-003 has created the `TimeSlot` EF model with `Id`, `RestaurantId`, `StartTime`, `RemainingCapacity`, and `RowVersion`.
- STORY-004 has seeded time slots with varied `RemainingCapacity` values (including 0).
- The `RestaurantsDbContext` exposes a `TimeSlots` `DbSet<TimeSlot>`.
- The `TimeSlotResponse` contract record (`SlotId`, `Time`, `RemainingCapacity`) already exists in `CM.TableNow.Contracts`.

## Technical Constraints

- No repository pattern — `DbContext` used directly in Data handlers (per CLAUDE.md).
- No AutoMapper — static `RestaurantMapper` in the Api project.
- All handlers return `Result<T>` from `CM.TableNow.Shared.Results`; endpoints call `TypedResultHelper.ToHttpResult`.
- File-scoped namespaces, primary constructors for DI, `CancellationToken` on all async methods, nullable enabled.
- EF filter applied via LINQ `.Where()` — not `.ToList()` followed by in-memory filter.
