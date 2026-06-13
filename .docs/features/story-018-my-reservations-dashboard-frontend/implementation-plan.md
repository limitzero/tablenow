# Implementation Plan: My Reservations Dashboard — Frontend

## Phase 1 — Store and Models

- [ ] **task-01-reservations-store** — Define `Reservation` TypeScript model, create `ReservationsService` (GET and DELETE via `HttpClient`), create `ReservationsStore` Signal Store with `reservations` state and `cancelReservation(id)` method.

### Technical Details

`Reservation` model: `{ reservationId: string; restaurantName: string; date: string; time: string; partySize: number; status: 'Confirmed' | 'Cancelled' }`.

## Phase 2 — List Component

- [ ] **task-02-reservations-list-component** — `ReservationListComponent` that fetches reservations on init, renders them in a Material list/table with a status chip, and shows a "Cancel" button for confirmed reservations.

### Technical Details

- Status chip color: `Confirmed` → green, `Cancelled` → grey/red.
- Cancel button only rendered for `status === 'Confirmed'`.

## Phase 3 — Cancellation Flow and Routes

- [ ] **task-03-cancellation-routes** — Confirmation dialog (Angular Material Dialog) on cancel button click, DELETE request on confirm, route file with `AuthGuard`, and app-level route registration.

### Technical Details

```typescript
export const reservationRoutes: Routes = [
  { path: '', component: ReservationListComponent, canActivate: [authGuard] },
];
```
