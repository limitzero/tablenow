# Task 02: Booking Flow Integration

## Status

pending

## Wave

2

## Description

Updates `RestaurantDetailComponent` to make time slots selectable, opens the confirmation dialog on slot click, issues `POST /api/reservations` on confirmation, navigates to `/reservations` on success, and refreshes the slot list on 409.

## Dependencies

**Depends on:** task-01-confirmation-dialog.md, STORY-013 task-03-detail-page.md (RestaurantDetailComponent exists)
**Blocks:** Nothing

**Context from dependencies:**
- task-01 created `BookingConfirmationDialogComponent` with `BookingDialogData` input and closes with `true/false`
- STORY-013 task-03 created `RestaurantDetailComponent` with slot list rendered via `@for`
- `POST /api/reservations` body: `{ timeSlotId: string, partySize: number }`. Returns 201 `{ reservationId, status }` or 409

## Files to Create

- `client/src/app/features/reservations/services/reservation.service.ts`

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-detail/restaurant-detail.component.ts` — Add slot selection and dialog logic

## Technical Details

### Code Snippets

```typescript
// reservation.service.ts
import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

export interface CreateReservationRequest {
  timeSlotId: string;
  partySize: number;
}

export interface CreateReservationResponse {
  reservationId: string;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class ReservationService {
  private readonly http = inject(HttpClient);

  create(request: CreateReservationRequest) {
    return this.http.post<CreateReservationResponse>(
      `${environment.apiBaseUrl}/reservations`, request
    );
  }
}
```

Add to `RestaurantDetailComponent`:
```typescript
// Inject MatDialog and ReservationService
private readonly dialog = inject(MatDialog);
private readonly reservationService = inject(ReservationService);
private readonly router = inject(Router);
private readonly snackBar = inject(MatSnackBar);
bookingError = signal<string | null>(null);

async onSlotSelected(slot: TimeSlot): Promise<void> {
  const dialogRef = this.dialog.open(BookingConfirmationDialogComponent, {
    data: {
      restaurantName: this.restaurant.value()?.name ?? '',
      date: this.availabilityQuery()?.date ?? '',
      time: slot.time,
      partySize: this.availabilityQuery()?.partySize ?? 1,
      slotId: slot.slotId,
    } as BookingDialogData,
  });

  const confirmed = await firstValueFrom(dialogRef.afterClosed());
  if (!confirmed) return;

  this.reservationService.create({
    timeSlotId: slot.slotId,
    partySize: this.availabilityQuery()?.partySize ?? 1,
  }).subscribe({
    next: () => {
      this.snackBar.open('Booking confirmed!', 'Close', { duration: 3000 });
      this.router.navigateByUrl('/reservations');
    },
    error: (err) => {
      if (err.status === 409) {
        this.bookingError.set('This slot is no longer available. Please select another time.');
        // Trigger slot refresh by re-emitting current query
        this.availabilityQuery.update(q => q ? { ...q } : q);
      }
    },
  });
}
```

In the HTML, make each slot list item a button:
```html
@for (slot of slots.value()!; track slot.slotId) {
  <button mat-stroked-button (click)="onSlotSelected(slot)">
    {{ slot.time }} — {{ slot.remainingCapacity }} seats
  </button>
}
@if (bookingError()) {
  <p class="error-message">{{ bookingError() }}</p>
}
```

## Acceptance Criteria

- [ ] Clicking a slot opens `BookingConfirmationDialogComponent`
- [ ] Confirming the dialog issues `POST /api/reservations`
- [ ] Success navigates to `/reservations` and shows a snackbar toast
- [ ] 409 shows error message and refreshes slot list
- [ ] Loading state during confirmation request
- [ ] `npm run build` exits with code 0
