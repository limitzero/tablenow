# Task 03: Cancellation Flow & Routes

## Status

pending

## Wave

3

## Description

Wire up the cancel confirmation dialog using Angular Material Dialog, connect it to the `onCancel` stub in `ReservationListComponent`, and register the `/reservations` route with `AuthGuard` in the app router. On confirmation, the dialog calls `store.cancelReservation(id)` and the status chip updates reactively from the store. On cancellation (dialog dismissed), no action is taken.

## Dependencies

**Depends on:** task-02-reservations-list-component.md
**Blocks:** None

**Context from dependencies:** task-02 created `ReservationListComponent` with an `onCancel(id)` stub method. task-01 created `ReservationsStore.cancelReservation(id)` which calls the DELETE API and patches local state. The `authGuard` function guard from STORY-009 is in `client/src/app/core/guards/auth.guard.ts`.

## Files to Create

- `client/src/app/features/reservations/routes/reservation.routes.ts` — Feature routes with `canActivate: [authGuard]`.

## Files to Modify

- `client/src/app/features/reservations/components/reservation-list/reservation-list.component.ts` — Implement `onCancel(id)` to open `MatDialog` confirmation, then call `store.cancelReservation(id)`.
- `client/src/app/app.routes.ts` — Add lazy-loaded `reservations` route.

## Technical Details

### Implementation Steps

1. In `ReservationListComponent.onCancel(id)`:
   ```typescript
   private readonly dialog = inject(MatDialog);
   
   onCancel(id: string): void {
     const ref = this.dialog.open(ConfirmDialogComponent, {
       data: { message: 'Are you sure you want to cancel this reservation?' },
     });
     ref.afterClosed().subscribe(confirmed => {
       if (confirmed) this.store.cancelReservation(id);
     });
   }
   ```
   Note: This is the one acceptable `.subscribe()` — on a finite dialog observable that completes after one emission.
2. Create (or reuse from `shared/`) a `ConfirmDialogComponent` with Yes/No buttons.
3. Create `reservation.routes.ts`:
   ```typescript
   export const reservationRoutes: Routes = [
     { path: '', component: ReservationListComponent, canActivate: [authGuard] },
   ];
   ```
4. In `app.routes.ts`:
   ```typescript
   { path: 'reservations', loadChildren: () => import('./features/reservations/routes/reservation.routes').then(m => m.reservationRoutes) }
   ```

## Acceptance Criteria

- [ ] Clicking "Cancel" opens a Material Dialog confirmation prompt.
- [ ] Confirming in the dialog calls `store.cancelReservation(id)` and the status badge updates to "Cancelled".
- [ ] Dismissing the dialog takes no action.
- [ ] `/reservations` route requires authentication (redirects to `/login` when unauthenticated).
- [ ] The feature is lazy-loaded in `app.routes.ts`.
