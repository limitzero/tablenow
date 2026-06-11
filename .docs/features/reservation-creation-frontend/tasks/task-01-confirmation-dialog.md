# Task 01: Booking Confirmation Dialog

## Status

pending

## Wave

1

## Description

Creates a standalone Angular Material dialog component that shows booking summary (restaurant name, date, time, party size) and has "Confirm Booking" and "Cancel" buttons. The dialog is opened from the detail page and returns the user's decision via `MatDialogRef`.

## Dependencies

**Depends on:** STORY-002 task-03-environment-config.md (Angular Material), STORY-013 task-01-slot-service.md (TimeSlot model)
**Blocks:** task-02-booking-flow.md

**Context from dependencies:** `TimeSlot`: `{ slotId, time, remainingCapacity }`. Angular Material Dialog is available. The dialog receives data via `MAT_DIALOG_DATA`.

## Files to Create

- `client/src/app/features/reservations/components/booking-confirmation-dialog/booking-confirmation-dialog.component.ts`
- `client/src/app/features/reservations/components/booking-confirmation-dialog/booking-confirmation-dialog.component.html`

## Technical Details

### Code Snippets

```typescript
// booking-confirmation-dialog.component.ts
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

export interface BookingDialogData {
  restaurantName: string;
  date: string;
  time: string;
  partySize: number;
  slotId: string;
}

@Component({
  selector: 'app-booking-confirmation-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule],
  templateUrl: './booking-confirmation-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookingConfirmationDialogComponent {
  readonly data = inject<BookingDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<BookingConfirmationDialogComponent>);

  confirm(): void { this.dialogRef.close(true); }
  cancel(): void { this.dialogRef.close(false); }
}
```

```html
<!-- booking-confirmation-dialog.component.html -->
<h2 mat-dialog-title>Confirm Your Booking</h2>
<mat-dialog-content>
  <p><strong>Restaurant:</strong> {{ data.restaurantName }}</p>
  <p><strong>Date:</strong> {{ data.date }}</p>
  <p><strong>Time:</strong> {{ data.time }}</p>
  <p><strong>Party Size:</strong> {{ data.partySize }}</p>
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button mat-button (click)="cancel()">Cancel</button>
  <button mat-raised-button color="primary" (click)="confirm()">Confirm Booking</button>
</mat-dialog-actions>
```

## Acceptance Criteria

- [ ] Dialog component exists with correct data bindings
- [ ] `confirm()` closes dialog with `true`
- [ ] `cancel()` closes dialog with `false`
- [ ] `changeDetection: ChangeDetectionStrategy.OnPush`
- [ ] `npm run build` exits with code 0
