# Task 03: Reservation Routes & Slot-Selection Integration

## Status

pending

## Wave

2

## Description

Wire the reservation feature together. Add the feature's route definitions, integrate `BookingConfirmationComponent` into the restaurant detail slot-selection flow (STORY-013) so that tapping a slot opens the confirmation dialog populated with the chosen slot, and add the required barrel export `index.ts` for the feature. On a 409 outcome from the dialog, re-fetch the slot list so the diner sees refreshed availability. This task makes the feature usable end-to-end.

## Dependencies

**Depends on:** task-01-reservations-store.md, task-02-booking-confirmation-dialog.md
**Blocks:** None

**Context from dependencies:**

- task-01-reservations-store.md created `ReservationsStore` (root-provided NgRx Signal Store) with `setSelectedSlot(slot)`, `clearSelectedSlot()`, `createReservation()`, and state `selectedSlot`, `bookingStatus`, `error`. `SelectedSlot` shape is `{ slotId, time, date, partySize, restaurantId, restaurantName }`.
- task-02-booking-confirmation-dialog.md created `BookingConfirmationComponent` (a Material dialog). When opened it reads `selectedSlot` from the store, shows the confirmation, and on confirm calls `createReservation()`. It closes with a result `{ outcome: 'success' | 'conflict' | 'cancelled' }` (exported type `BookingConfirmationResult`). On success it already navigates to `/reservations` and shows a toast; on conflict it shows a toast and closes with `{ outcome: 'conflict' }`.
- STORY-013 produced the restaurant detail component under `client/src/app/features/restaurants/` with a slot list rendered from a slots `httpResource()` keyed on `{date, partySize}`, and a way to react to a slot being chosen. This task adds the "open confirmation dialog" behavior to that flow and triggers a slot refresh on conflict.
- STORY-018 will register the `/reservations` route for the dashboard; this task only navigates to it (already handled inside the dialog) and defines the reservations feature's own route entry that can be lazy-loaded into the app router.

## Files to Create

- `client/src/app/features/reservations/routes/reservations.routes.ts` — route definitions for the reservations feature.
- `client/src/app/features/reservations/index.ts` — barrel export for the feature (store, service, models, routes, components).

## Files to Modify

- `client/src/app/features/restaurants/<detail component>.ts` — when a slot is selected, call `ReservationsStore.setSelectedSlot(...)` and open `BookingConfirmationComponent` via `MatDialog`; on `{ outcome: 'conflict' }` trigger a slot-list refresh (re-run the slots `httpResource()`/reload). (Exact filename per the STORY-013 detail component; locate the slot-selection handler.)
- `client/src/app/app.routes.ts` — register the reservations feature routes if a placeholder is not already present (only if `/reservations` is not yet wired; coordinate with STORY-018 to avoid duplicate registration).

## Technical Details

### Implementation Steps

1. Create `reservations.routes.ts` exporting a `Routes` array. The dashboard route (`/reservations`) is owned by STORY-018; define the feature route entry so it can be composed into the app router without duplicating the dashboard. If STORY-018 has not landed, export an empty-but-typed `RESERVATIONS_ROUTES: Routes = []` placeholder plus a comment noting STORY-018 adds the dashboard route here. Do not redefine `/reservations` if STORY-018 already registered it.
2. In the STORY-013 detail component, locate the slot-selection handler (where a slot is tapped). Inject `ReservationsStore` and `MatDialog` via `inject()`. On slot tap:
   - Build a `SelectedSlot` from the tapped slot plus the current restaurant name/id and the form's `date` and `partySize`.
   - `this.reservationsStore.setSelectedSlot(selected);`
   - `const ref = this.dialog.open(BookingConfirmationComponent);`
   - `const result = await firstValueFrom(ref.afterClosed());`
   - If `result?.outcome === 'conflict'`, refresh the slot list (e.g. call the slots `httpResource().reload()` or re-trigger the query keyed on `{date, partySize}`).
3. Create `index.ts` barrel exporting `ReservationsStore`, `ReservationsService`, the models, `BookingConfirmationComponent`, and `RESERVATIONS_ROUTES`.
4. Ensure all touched components keep `OnPush`, `inject()`, and `@if`/`@for`.

### Code Snippets

`routes/reservations.routes.ts`:

```typescript
import { Routes } from '@angular/router';

// The `/reservations` dashboard route is registered by STORY-018.
// This array composes the reservations feature into the app router.
export const RESERVATIONS_ROUTES: Routes = [
  // { path: 'reservations', loadComponent: () => import('../components/...').then(m => m.ReservationsDashboardComponent) }, // STORY-018
];
```

Slot-selection integration (inside the STORY-013 detail component):

```typescript
private readonly dialog = inject(MatDialog);
private readonly reservationsStore = inject(ReservationsStore);

protected async onSlotSelected(slot: { slotId: string; time: string }): Promise<void> {
  this.reservationsStore.setSelectedSlot({
    slotId: slot.slotId,
    time: slot.time,
    date: this.form.controls.date.value!,            // from the STORY-013 reactive form
    partySize: this.form.controls.partySize.value!,  // from the STORY-013 reactive form
    restaurantId: this.restaurant().id,
    restaurantName: this.restaurant().name,
  });

  const ref = this.dialog.open(BookingConfirmationComponent);
  const result = await firstValueFrom(ref.afterClosed());

  if (result?.outcome === 'conflict') {
    // Slot sold out mid-flow — refresh availability so the diner can re-pick.
    this.slotsResource.reload(); // re-run the slots httpResource() keyed on {date, partySize}
  }
}
```

`index.ts`:

```typescript
export * from './store/reservations.store';
export * from './services/reservations.service';
export * from './models/reservation.models';
export * from './components/booking-confirmation/booking-confirmation.component';
export * from './routes/reservations.routes';
```

## Acceptance Criteria

- [ ] Tapping an available slot on the restaurant detail page opens `BookingConfirmationComponent` populated with the restaurant name, selected date, slot time, and party size.
- [ ] The confirmation dialog is opened via `MatDialog` after `setSelectedSlot(...)` is called.
- [ ] When the dialog closes with `{ outcome: 'conflict' }`, the slot list is refreshed (the slots fetch re-runs).
- [ ] On a successful booking the user lands on `/reservations` (navigation performed by the dialog).
- [ ] The feature exposes a barrel `index.ts` exporting the store, service, models, dialog component, and routes.
- [ ] All modified/created components use `OnPush`, `inject()`, and `@if`/`@for`.

## Notes

- Coordinate route registration with STORY-018: the `/reservations` dashboard route should be defined once. If STORY-018 already added it to `app.routes.ts`, leave `RESERVATIONS_ROUTES` as a placeholder and do not duplicate.
- The actual slots-resource refresh mechanism depends on how STORY-013 implemented its `httpResource()` (a `.reload()` call, or re-setting the keyed signal). Use whichever the existing detail component exposes; the requirement is simply that availability re-renders after a conflict.
- `firstValueFrom(ref.afterClosed())` is acceptable here because it is in a component method awaiting a dialog result, not a long-lived stream subscription; it completes when the dialog closes.
