# Implementation Plan: Restaurant Detail & Availability — Frontend

## Phase 1 — Detail Component

- [ ] **task-01-restaurant-detail-component** — Create `RestaurantDetailComponent` that reads `:id` from the route, fetches the restaurant from `RestaurantsStore` (or via `GET /api/restaurants/{id}`), renders the full restaurant profile, and adds the `:id` route to `restaurantRoutes`.

### Technical Details

- Read `restaurantId = inject(ActivatedRoute).snapshot.params['id']`.
- If the store already has the restaurant (user navigated from the list), use `store.restaurants().find(r => r.id === restaurantId)`. Otherwise fetch via `httpResource`.

## Phase 2 — Slot Availability Form

- [ ] **task-02-slot-availability-integration** — Add date picker (default today + 1), party size selector (1–20, default 2), reactive slot query via `httpResource` keyed on `{ restaurantId, date, partySize }`, and the slot list / empty state rendering. Define `TimeSlot` TypeScript model.

### Technical Details

`TimeSlot` model: `{ slotId: string; time: string; remainingCapacity: number }`.

```typescript
// Reactive slot query
const date = signal(defaultDate);
const partySize = signal(2);
const slotsResource = httpResource<TimeSlot[]>(() =>
  `${environment.apiBaseUrl}/restaurants/${restaurantId}/slots?date=${date()}&partySize=${partySize()}`
);
```

Template: `@for (slot of slotsResource.value(); track slot.slotId)` with time and remaining capacity shown; `@if (!slotsResource.isLoading() && slotsResource.value()?.length === 0)` for empty state.
