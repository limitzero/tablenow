# Requirements: Restaurant Detail & Availability — Frontend

## Summary

Diners select a date and party size to see available time slots before booking. The detail page fetches the restaurant by ID and displays full info. The availability form re-fetches slots on every change — date defaults to tomorrow and party size defaults to 2.

## Goals

- Restaurant name, cuisine, address, description displayed
- Date picker (default: tomorrow), party size selector (1–20, default: 2)
- Slot list refreshes automatically on form change via httpResource
- "No availability" message when slot list is empty
- Clicking a slot is wired to the booking flow (STORY-015 adds confirmation dialog)

## Acceptance Criteria

- [ ] Full restaurant detail displayed on load
- [ ] Date defaults to today + 1; party size defaults to 2
- [ ] Changing date or partySize triggers GET /api/restaurants/{id}/slots re-fetch
- [ ] Each slot shows time and remaining seats
- [ ] Empty slot list shows "No availability for this date and party size."
- [ ] Loading state shown while fetching

## Technical Constraints

- One component file extended across the two tasks
- `httpResource()` keyed on `{date, partySize}` signals for reactive slot fetching
