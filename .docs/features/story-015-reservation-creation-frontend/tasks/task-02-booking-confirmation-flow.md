# Task 02: Booking Confirmation Flow

## Status

pending

## Wave

2

## Description

Extends `RestaurantDetailComponent` (from STORY-013) with the full booking flow: slot click → open dialog → API call → success toast and navigation OR error handling and slot refresh.

## Dependencies

**Depends on:** task-01-reservation-service.md
**Blocks:** STORY-018 (after booking succeeds, user is navigated to /reservations)

**Context from dependencies:** task-01 created `ReservationService` and `BookingConfirmationDialogComponent`. STORY-013 task-02 created `RestaurantDetailComponent.selectSlot()` method and `slotsResource` for slot fetching. This task extends the detail component by injecting new services and completing the `selectSlot()` method implementation.

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-detail.component.ts`

## Technical Details

### Implementation Steps

Add to `RestaurantDetailComponent`:

```typescript
// New imports
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ReservationService } from '../../reservations/services/reservation.service';
import { BookingConfirmationDialogComponent } from '../../reservations/components/booking-confirmation-dialog.component';

// New injections in class
private readonly dialog = inject(MatDialog);
private readonly snackBar = inject(MatSnackBar);
private readonly reservationService = inject(ReservationService);

// Replace stub selectSlot() with full implementation:
selectSlot(slot: SlotDto): void {
  const restaurant = this.restaurantResource.value();
  const { date, partySize } = this.availabilityForm.value;
  if (!restaurant || !date || !partySize) return;

  const dialogRef = this.dialog.open(BookingConfirmationDialogComponent, {
    data: {
      restaurantName: restaurant.name,
      date,
      time: slot.time.slice(0, 5),
      partySize,
      slotId: slot.slotId,
    },
    width: '400px',
  });

  dialogRef.afterClosed().subscribe((confirmed: boolean) => {
    if (!confirmed) return;

    this.reservationService.createReservation(slot.slotId, partySize).subscribe({
      next: () => {
        this.snackBar.open('Reservation confirmed!', 'Close', { duration: 3000 });
        this.router.navigate(['/reservations']);
      },
      error: (err) => {
        if (err.status === 409) {
          this.slotError.set('Sorry, that time slot was just taken. Please choose another.');
          this.slotsResource.reload(); // trigger re-fetch
        } else {
          this.slotError.set('Booking failed. Please try again.');
        }
      },
    });
  });
}
```

Add `readonly slotError = signal<string | null>(null);` to the component.

Add `@if (slotError()) { <p class="error">{{ slotError() }}</p> }` to the template above the slot list.

Import `MatSnackBarModule`, `MatDialogModule` in the component's `imports` array.

## Acceptance Criteria

- [ ] Clicking a slot opens `BookingConfirmationDialogComponent` with correct data
- [ ] Confirming calls `reservationService.createReservation(slotId, partySize)`
- [ ] On 201: snackbar opens with "Reservation confirmed!" and user navigates to /reservations
- [ ] On 409: `slotError` signal set, `slotsResource.reload()` called to refresh slots
- [ ] Dialog is dismissed on both success and error paths
