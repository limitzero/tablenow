# Task 16: Reservation Booking Frontend — Confirmation Flow

## Status

pending

## Phase

9

## Description

Complete the end-to-end booking flow by adding a confirmation dialog to the restaurant detail page. When a diner clicks "Book" on a time slot (task-14), an Angular Material dialog opens showing restaurant name, date, time, and party size. Clicking "Confirm Booking" posts to `POST /api/v1/reservations`. On 201 success the dialog closes, a success snackbar appears, and the user is navigated to `/reservations`. On 409 (slot no longer available), the dialog shows an error and refreshes the slot list. The `confirmBooking` method lives in the `ReservationsStore`.

## Dependencies

**Depends on:** task-14-restaurant-detail-frontend, task-11-reservation-management-backend  
**Blocks:** (none — this is the final task, completing the MVP)

**Context from dependencies:** task-14 created `RestaurantDetailComponent` with a `selectedSlot` signal that is set when the user clicks "Book". The component knows the `restaurantId` and current form values (`date`, `partySize`). task-11 ensured `POST /api/v1/reservations` accepts `{slotId, partySize}` and returns 201 `{reservationId, restaurantName, date, time, partySize, status}` or 409 `{errors: [...]}`. `ReservationsService` in `features/reservations/services/` already has `cancelReservation` — add `createReservation` here. The `authInterceptor` (task-10) attaches the JWT automatically.

## Files to Create

- `client/src/app/features/reservations/components/booking-confirmation-dialog/booking-confirmation-dialog.component.ts`
- `client/src/app/features/reservations/components/booking-confirmation-dialog/booking-confirmation-dialog.component.html`

## Files to Modify

- `client/src/app/features/reservations/services/reservations.service.ts` — add `createReservation(slotId, partySize)` method
- `client/src/app/features/reservations/store/reservations.store.ts` — add `createReservation` method
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.ts` — open dialog when `selectedSlot` is set
- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.html` — no template changes needed (dialog opens programmatically)

## Technical Details

### Implementation Steps

1. **Add `createReservation` to `ReservationsService`**.
2. **Add `createReservation` method to `ReservationsStore`** (handles success navigation + error state).
3. **Create `BookingConfirmationDialogComponent`** — receives slot/restaurant data as dialog data.
4. **Update `RestaurantDetailComponent.selectSlot()`** — open the dialog, handle its result.

### Code Snippets

**`ReservationsService` addition:**
```typescript
createReservation(slotId: string, partySize: number) {
  return this.http.post<{
    reservationId: string;
    restaurantName: string;
    date: string;
    time: string;
    partySize: number;
    status: string;
  }>(`${this.base}/reservations`, { slotId, partySize });
}
```

**`booking-confirmation-dialog.component.ts`:**
```typescript
import {
  Component, ChangeDetectionStrategy, inject, signal, Inject
} from '@angular/core';
import {
  MAT_DIALOG_DATA, MatDialogRef, MatDialogModule
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { ReservationsService } from '../../../reservations/services/reservations.service';
import { MatSnackBar } from '@angular/material/snack-bar';

export interface BookingDialogData {
  restaurantName: string;
  date: string;      // "yyyy-MM-dd"
  time: string;      // "HH:mm"
  partySize: number;
  slotId: string;
}

@Component({
  selector: 'app-booking-confirmation-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './booking-confirmation-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookingConfirmationDialogComponent {
  private readonly service = inject(ReservationsService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialogRef = inject(MatDialogRef<BookingConfirmationDialogComponent>);

  readonly data = inject<BookingDialogData>(MAT_DIALOG_DATA);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  confirm(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.service.createReservation(this.data.slotId, this.data.partySize).subscribe({
      next: () => {
        this.dialogRef.close('confirmed');
        this.snackBar.open('Reservation confirmed!', 'Close', { duration: 4000 });
        this.router.navigate(['/reservations']);
      },
      error: (err) => {
        this.isLoading.set(false);
        if (err.status === 409) {
          this.errorMessage.set('This time slot is no longer available. Please choose another.');
          this.dialogRef.close('slot-unavailable');
        } else {
          this.errorMessage.set('Something went wrong. Please try again.');
        }
      },
    });
  }
}
```

**`booking-confirmation-dialog.component.html`:**
```html
<h2 mat-dialog-title>Confirm Your Booking</h2>

<mat-dialog-content>
  @if (errorMessage()) {
    <p class="error-message">{{ errorMessage() }}</p>
  }
  <dl class="booking-summary">
    <dt>Restaurant</dt><dd>{{ data.restaurantName }}</dd>
    <dt>Date</dt>      <dd>{{ data.date }}</dd>
    <dt>Time</dt>      <dd>{{ data.time }}</dd>
    <dt>Party size</dt><dd>{{ data.partySize }}</dd>
  </dl>
</mat-dialog-content>

<mat-dialog-actions align="end">
  <button mat-button mat-dialog-close [disabled]="isLoading()">Cancel</button>
  <button
    mat-flat-button color="primary"
    [disabled]="isLoading()"
    (click)="confirm()">
    @if (isLoading()) { Booking… } @else { Confirm Booking }
  </button>
</mat-dialog-actions>
```

**`RestaurantDetailComponent.selectSlot()` update:**

Add to imports in `restaurant-detail.component.ts`:
```typescript
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { BookingConfirmationDialogComponent, BookingDialogData }
  from '../../../reservations/components/booking-confirmation-dialog/booking-confirmation-dialog.component';
```

Add `MatDialogModule` to `imports` array and `MatDialog` injection:
```typescript
private readonly dialog = inject(MatDialog);
```

Replace `selectSlot()` method:
```typescript
selectSlot(slot: Slot): void {
  const { date, partySize } = this.form.getRawValue();
  const dateStr = date!.toISOString().split('T')[0];
  const restaurant = this.restaurant();
  if (!restaurant) return;

  const dialogData: BookingDialogData = {
    restaurantName: restaurant.name,
    date: dateStr,
    time: slot.time,
    partySize: partySize!,
    slotId: slot.slotId,
  };

  const dialogRef = this.dialog.open(BookingConfirmationDialogComponent, {
    data: dialogData,
    width: '400px',
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result === 'slot-unavailable') {
      // Refresh slot list so the unavailable slot is removed
      this.loadSlots();
      this.selectedSlot.set(null);
    }
  });
}
```

**`app.routes.ts` — verify `MatSnackBar` is available:**
Add `provideAnimations()` or `provideAnimationsAsync()` to `app.config.ts` if not already present (it was added in task-02).

### API Contracts Used

| Endpoint | Request | Success | Error |
|----------|---------|---------|-------|
| `POST /api/v1/reservations` | `{slotId: string, partySize: number}` | 201 `{reservationId, restaurantName, date, time, partySize, status}` | 409, 401 |

## Acceptance Criteria

- [ ] Clicking "Book" on a slot opens a confirmation dialog showing restaurant, date, time, party size
- [ ] Clicking "Confirm Booking" calls `POST /api/v1/reservations` with `{slotId, partySize}`
- [ ] On 201 success: dialog closes, success snackbar appears, user navigates to `/reservations`
- [ ] On 409 (slot unavailable): dialog shows error message; closing the dialog refreshes the slot list
- [ ] "Confirm Booking" button shows loading state and is disabled while the API call is in flight
- [ ] The entire MVP E2E flow works: register → browse → select date/party → book → view dashboard → cancel

## Notes

- `MAT_DIALOG_DATA` must be injected using `inject(MAT_DIALOG_DATA)` — not constructor injection.
- `MatSnackBarModule` is included in Angular Material by default when `provideAnimationsAsync()` is used.
- The `dialogRef.afterClosed()` subscription is in the parent component — this is acceptable because `afterClosed()` completes after one emission, so there is no leak.
- After this task is complete, the full MVP E2E flow is functional. Verify in browser: register → browse restaurants → select date + party size → click Book → confirm → see dashboard → cancel reservation.
