# Task 02: Booking Confirmation Dialog

## Status

pending

## Wave

1

## Description

Create `BookingConfirmationComponent`, an Angular Material dialog that lets a diner review their booking before committing. It displays the restaurant name, date, time, and party size for the selected slot. A "Confirm Booking" button issues the reservation request; while the request is in flight, the button is disabled and a loading indicator is shown so the diner cannot double-submit. On success the diner is navigated to `/reservations` ("My Reservations") and a success toast is shown. On a 409 Conflict (the slot sold out mid-flow) an inline error message is shown and the dialog signals that the slot list should be refreshed. This component consumes the `ReservationsStore` from task 01 for state and the booking action.

## Dependencies

**Depends on:** None for file overlap (Wave 1, runs in parallel with task 01), but consumes the store contract from task-01-reservations-store.md.
**Blocks:** task-03-reservation-routes.md

**Context from dependencies:** task-01-reservations-store.md creates `ReservationsStore` (an NgRx Signal Store provided in root) with state `selectedSlot` (`{ slotId, time, date, partySize, restaurantId, restaurantName }`), `bookingStatus` (`'idle' | 'pending' | 'success' | 'error'`), `error`, and a computed `isBooking`. It exposes `createReservation()` (returns the created `ReservationResponse` or `null`, sets `bookingStatus` to `'pending'` then `'success'`/`'error'`, and on 409 clears `selectedSlot` and sets `error` to "This time slot is no longer available.") and `clearSelectedSlot()`. This component reads `selectedSlot`, `isBooking`, and `error` from the store and calls `createReservation()` on confirm. The dialog is opened by task 03 (which passes/sets the selected slot beforehand). Foundations: STORY-002 provides the Angular 21 standalone-component app with Angular Material and the custom theme; routing via `@angular/router` `Router`.

## Files to Create

- `client/src/app/features/reservations/components/booking-confirmation/booking-confirmation.component.ts` — standalone dialog component (logic).
- `client/src/app/features/reservations/components/booking-confirmation/booking-confirmation.component.html` — dialog template.
- `client/src/app/features/reservations/components/booking-confirmation/booking-confirmation.component.scss` — component styles (may be empty).

## Files to Modify

None.

## Technical Details

### Implementation Steps

1. Create a standalone component with `changeDetection: ChangeDetectionStrategy.OnPush`.
2. Inject dependencies with `inject()`: `ReservationsStore`, `MatDialogRef<BookingConfirmationComponent>`, `Router`, `MatSnackBar`. Read the selected slot from the store (`store.selectedSlot`).
3. Import the Material modules the template uses: `MatDialogModule`, `MatButtonModule`, `MatProgressSpinnerModule` (or `MatProgressBarModule`). `MatSnackBar` is provided via DI, not imported into `imports`.
4. Template (`@if` / `@for` only):
   - Title: "Confirm your booking".
   - Body rows showing `slot.restaurantName`, formatted `slot.date`, `slot.time`, and `slot.partySize` (use Angular `DatePipe`/`titlecase` as helpful; import `DatePipe` via `CommonModule` or use the standalone `DatePipe`).
   - If `store.error()` is set, show it in an error styled block.
   - Actions row: a "Cancel" button (closes the dialog) and a "Confirm Booking" button.
5. The confirm button binds `[disabled]="store.isBooking()"` and shows a `mat-progress-spinner` (small, `diameter="20"`) inside or beside the button while `store.isBooking()` is true.
6. `onConfirm()`:
   - `const created = await this.store.createReservation();`
   - If `created` is truthy: open a success snackbar ("Reservation confirmed!"), close the dialog with a success result (`this.dialogRef.close({ outcome: 'success' })`), and navigate: `await this.router.navigate(['/reservations'])`.
   - If `created` is null and the store error is the 409 message: keep the dialog open showing the error, then close with a result indicating a refresh is needed (`this.dialogRef.close({ outcome: 'conflict' })`) OR keep open and let task 03's caller react to the `selectedSlot` having been cleared. Prefer closing with `{ outcome: 'conflict' }` so the slot-selection flow (task 03) can refresh the slot list. Show the conflict message via snackbar as well.
   - For other errors, keep the dialog open and display `store.error()` inline.
7. `onCancel()`: `this.dialogRef.close({ outcome: 'cancelled' })`.

### Code Snippets

`booking-confirmation.component.ts`:

```typescript
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ReservationsStore } from '../../store/reservations.store';

export interface BookingConfirmationResult {
  outcome: 'success' | 'conflict' | 'cancelled';
}

@Component({
  selector: 'app-booking-confirmation',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, MatDialogModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './booking-confirmation.component.html',
  styleUrl: './booking-confirmation.component.scss',
})
export class BookingConfirmationComponent {
  protected readonly store = inject(ReservationsStore);
  private readonly dialogRef = inject(MatDialogRef<BookingConfirmationComponent, BookingConfirmationResult>);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  protected async onConfirm(): Promise<void> {
    const created = await this.store.createReservation();
    if (created) {
      this.snackBar.open('Reservation confirmed!', 'Dismiss', { duration: 4000 });
      this.dialogRef.close({ outcome: 'success' });
      await this.router.navigate(['/reservations']);
      return;
    }
    const error = this.store.error();
    if (error === 'This time slot is no longer available.') {
      this.snackBar.open(error, 'Dismiss', { duration: 5000 });
      this.dialogRef.close({ outcome: 'conflict' });
    }
    // For other errors the dialog stays open showing store.error() inline.
  }

  protected onCancel(): void {
    this.dialogRef.close({ outcome: 'cancelled' });
  }
}
```

`booking-confirmation.component.html`:

```html
<h2 mat-dialog-title>Confirm your booking</h2>

@if (store.selectedSlot(); as slot) {
  <mat-dialog-content>
    <dl class="booking-summary">
      <dt>Restaurant</dt>
      <dd>{{ slot.restaurantName }}</dd>
      <dt>Date</dt>
      <dd>{{ slot.date | date: 'fullDate' }}</dd>
      <dt>Time</dt>
      <dd>{{ slot.time }}</dd>
      <dt>Party size</dt>
      <dd>{{ slot.partySize }}</dd>
    </dl>

    @if (store.error()) {
      <p class="booking-error" role="alert">{{ store.error() }}</p>
    }
  </mat-dialog-content>

  <mat-dialog-actions align="end">
    <button mat-button type="button" (click)="onCancel()" [disabled]="store.isBooking()">
      Cancel
    </button>
    <button mat-flat-button color="primary" type="button" (click)="onConfirm()" [disabled]="store.isBooking()">
      @if (store.isBooking()) {
        <mat-progress-spinner diameter="20" mode="indeterminate" />
      } @else {
        Confirm Booking
      }
    </button>
  </mat-dialog-actions>
} @else {
  <mat-dialog-content>
    <p>No slot selected.</p>
  </mat-dialog-content>
}
```

## Acceptance Criteria

- [ ] The dialog displays restaurant name, date, time, and party size from the store's `selectedSlot`.
- [ ] Clicking "Confirm Booking" calls `store.createReservation()` (which issues `POST /api/reservations`).
- [ ] While the request is in flight, the confirm button is disabled and a loading indicator (spinner) is shown.
- [ ] On success, a success toast is shown, the dialog closes with `{ outcome: 'success' }`, and the app navigates to `/reservations`.
- [ ] On a 409, the conflict error message is shown (snackbar) and the dialog closes with `{ outcome: 'conflict' }` so the caller can refresh the slot list.
- [ ] The component uses `OnPush`, `inject()`, standalone imports, and `@if` (no `*ngIf`).

## Notes

- The result object `{ outcome: 'success' | 'conflict' | 'cancelled' }` is the contract task 03 uses to decide whether to refresh the slot list (on `'conflict'`).
- Do not perform the slot-list refresh inside this dialog — that belongs to the slot-selection flow in task 03, which owns the slots `httpResource()`. This dialog only signals the outcome.
- `MatSnackBar` requires `provideAnimations()` (or `provideAnimationsAsync()`) to be present in the app config; this was set up in STORY-002 for Angular Material. If animations are not yet configured, the snackbar still renders without transitions.
