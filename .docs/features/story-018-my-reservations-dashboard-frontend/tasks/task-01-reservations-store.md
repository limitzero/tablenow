# Task 01: Reservations Store & Models

## Status

pending

## Wave

1

## Description

Create the `Reservation` TypeScript model, `ReservationsService` with GET and DELETE methods, and the `ReservationsStore` NgRx Signal Store slice. The store holds the user's reservations list and exposes a `cancelReservation(id)` method that calls the service and updates the local state on success without a full re-fetch.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-02-reservations-list-component.md

**Context from dependencies:** Assumes STORY-009 JWT interceptor automatically attaches the `Authorization` header. `GET /api/reservations/my` returns an array of `{ reservationId, restaurantName, date, time, partySize, status }`. `DELETE /api/reservations/{id}` returns 200 on success.

## Files to Create

- `client/src/app/features/reservations/models/reservation.model.ts` — `Reservation` interface.
- `client/src/app/features/reservations/services/reservations.service.ts` — `ReservationsService`.
- `client/src/app/features/reservations/store/reservations.store.ts` — NgRx Signal Store slice.
- `client/src/app/features/reservations/index.ts` — Barrel export.

## Files to Modify

- None (new feature folder).

## Technical Details

### Implementation Steps

1. Define `Reservation` interface: `reservationId`, `restaurantName`, `date`, `time`, `partySize`, `status` (`'Confirmed' | 'Cancelled'`).
2. Create `ReservationsService` with `getMyReservations()` (returns `Observable<Reservation[]>`) and `cancel(id: string)` (returns `Observable<void>`).
3. Create `ReservationsStore` with `withState<{ reservations: Reservation[] }>`, `withMethods` including `loadReservations()` that calls the service and `cancelReservation(id)` that calls the service then updates local state by setting status to `'Cancelled'`.

### Code Snippets

```typescript
// reservations.store.ts
export const ReservationsStore = signalStore(
  { providedIn: 'root' },
  withState<{ reservations: Reservation[] }>({ reservations: [] }),
  withMethods((store, service = inject(ReservationsService)) => ({
    async loadReservations() {
      const list = await firstValueFrom(service.getMyReservations());
      patchState(store, { reservations: list });
    },
    async cancelReservation(id: string) {
      await firstValueFrom(service.cancel(id));
      patchState(store, {
        reservations: store.reservations().map(r =>
          r.reservationId === id ? { ...r, status: 'Cancelled' as const } : r
        ),
      });
    },
  })),
);
```

## Acceptance Criteria

- [ ] `Reservation` interface has `reservationId`, `restaurantName`, `date`, `time`, `partySize`, `status`.
- [ ] `loadReservations()` fetches from `GET /api/reservations/my` and populates the store.
- [ ] `cancelReservation(id)` calls `DELETE /api/reservations/{id}` and updates the local status to `'Cancelled'`.
- [ ] `index.ts` exports the store, service, and model.
