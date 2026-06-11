# Task 01: Reservation Service & Dialog

## Status

pending

## Wave

1

## Description

Creates `ReservationService` for the booking POST call and `BookingConfirmationDialogComponent` — the Material Dialog that shows booking details before the diner confirms.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-002 Angular scaffold)
**Blocks:** task-02-booking-confirmation-flow.md

**Context from dependencies:** STORY-014 created `POST /api/reservations` accepting `{slotId, partySize}`. `environment.ts` has `apiBaseUrl`. The dialog receives data via `MAT_DIALOG_DATA` injection token and returns a boolean via `MatDialogRef`.

## Files to Create

- `client/src/app/features/reservations/services/reservation.service.ts`
- `client/src/app/features/reservations/components/booking-confirmation-dialog.component.ts`
- `client/src/app/features/reservations/models/reservation.model.ts`

## Technical Details

### Code Snippets

```typescript
// models/reservation.model.ts
export interface CreateReservationResponse {
  reservationId: string;
  slotId: string;
  restaurantName: string;
  dateTime: string;
  partySize: number;
  status: string;
}

export interface BookingDialogData {
  restaurantName: string;
  date: string;
  time: string;
  partySize: number;
  slotId: string;
}
```

```typescript
// services/reservation.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { CreateReservationResponse } from '../models/reservation.model';

@Injectable({ providedIn: 'root' })
export class ReservationService {
  private readonly http = inject(HttpClient);

  createReservation(slotId: string, partySize: number) {
    return this.http.post<CreateReservationResponse>(
      `${environment.apiBaseUrl}/reservations`,
      { slotId, partySize }
    );
  }
}
```

```typescript
// components/booking-confirmation-dialog.component.ts
import { Component, inject, ChangeDetectionStrategy, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BookingDialogData } from '../models/reservation.model';

@Component({
  selector: 'app-booking-confirmation-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatProgressSpinnerModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2 mat-dialog-title>Confirm Reservation</h2>
    <mat-dialog-content>
      <p><strong>{{ data.restaurantName }}</strong></p>
      <p>{{ data.date }} at {{ data.time }}</p>
      <p>Party of {{ data.partySize }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="cancel()">Cancel</button>
      <button mat-raised-button color="primary"
              [disabled]="loading()"
              (click)="confirm()">
        @if (loading()) { <mat-spinner diameter="20" /> } @else { Confirm Booking }
      </button>
    </mat-dialog-actions>
  `,
})
export class BookingConfirmationDialogComponent {
  protected readonly data = inject<BookingDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<BookingConfirmationDialogComponent>);
  readonly loading = signal(false);

  confirm() {
    this.loading.set(true);
    this.dialogRef.close(true);
  }

  cancel() {
    this.dialogRef.close(false);
  }
}
```

## Acceptance Criteria

- [ ] `ReservationService.createReservation(slotId, partySize)` POSTs to `/api/reservations`
- [ ] `BookingConfirmationDialogComponent` shows restaurant, date, time, partySize
- [ ] Dialog returns `true` on confirm, `false` on cancel
- [ ] Dialog accepts `BookingDialogData` via `MAT_DIALOG_DATA`
- [ ] Both files use OnPush and inject()
