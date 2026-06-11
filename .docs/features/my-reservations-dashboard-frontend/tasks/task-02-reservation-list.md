# Task 02: Reservation List Component

## Status

pending

## Wave

1

## Description

Creates a `ReservationListItemComponent` that renders a single reservation row with status badge (colored Angular Material chip) and a Cancel button visible only for Confirmed reservations. Emits a `cancelRequested` output event.

## Dependencies

**Depends on:** STORY-002 task-03-environment-config.md (Angular Material)
**Blocks:** task-03-dashboard-page.md

**Context from dependencies:** `MyReservation` model (parallel task-01): `{ reservationId, restaurantName, date, time, partySize, status: 'Confirmed' | 'Cancelled' }`.

## Files to Create

- `client/src/app/features/reservations/components/reservation-list-item/reservation-list-item.component.ts`
- `client/src/app/features/reservations/components/reservation-list-item/reservation-list-item.component.html`
- `client/src/app/features/reservations/components/reservation-list-item/reservation-list-item.component.scss`

## Technical Details

### Code Snippets

```typescript
// reservation-list-item.component.ts
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MyReservation } from '../../models/reservation.model';

@Component({
  selector: 'app-reservation-list-item',
  standalone: true,
  imports: [MatCardModule, MatChipsModule, MatButtonModule],
  templateUrl: './reservation-list-item.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReservationListItemComponent {
  reservation = input.required<MyReservation>();
  cancelRequested = output<string>(); // reservationId

  onCancel(): void {
    this.cancelRequested.emit(this.reservation().reservationId);
  }
}
```

```html
<!-- reservation-list-item.component.html -->
<mat-card class="reservation-item">
  <mat-card-header>
    <mat-card-title>{{ reservation().restaurantName }}</mat-card-title>
    <mat-card-subtitle>{{ reservation().date }} at {{ reservation().time }}</mat-card-subtitle>
  </mat-card-header>
  <mat-card-content>
    <p>Party of {{ reservation().partySize }}</p>
    <mat-chip-set>
      <mat-chip [color]="reservation().status === 'Confirmed' ? 'primary' : 'warn'" selected>
        {{ reservation().status }}
      </mat-chip>
    </mat-chip-set>
  </mat-card-content>
  @if (reservation().status === 'Confirmed') {
    <mat-card-actions>
      <button mat-button color="warn" (click)="onCancel()">Cancel Reservation</button>
    </mat-card-actions>
  }
</mat-card>
```

## Acceptance Criteria

- [ ] Component renders restaurant name, date, time, party size, status badge
- [ ] Status chip is green/primary for Confirmed, warn/red for Cancelled
- [ ] Cancel button only visible on Confirmed reservations (`@if`)
- [ ] `cancelRequested` emits the `reservationId`
- [ ] `changeDetection: ChangeDetectionStrategy.OnPush`
- [ ] `npm run build` exits with code 0
