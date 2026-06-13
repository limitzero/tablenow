# Task 02: Reservations List Component

## Status

pending

## Wave

2

## Description

Create `ReservationListComponent` that calls `store.loadReservations()` on init, renders reservations in an Angular Material list with a colored status chip for each item, and shows a "Cancel" button on rows with `status === 'Confirmed'`. The cancel button emits to task-03's dialog flow (hooked up in that task). Uses `OnPush` change detection.

## Dependencies

**Depends on:** task-01-reservations-store.md
**Blocks:** task-03-cancellation-routes.md

**Context from dependencies:** task-01 created `ReservationsStore` with `reservations()` signal, `loadReservations()`, and `cancelReservation(id)`. The `Reservation` model has `reservationId`, `restaurantName`, `date`, `time`, `partySize`, `status`.

## Files to Create

- `client/src/app/features/reservations/components/reservation-list/reservation-list.component.ts` — Component.
- `client/src/app/features/reservations/components/reservation-list/reservation-list.component.html` — Template.

## Files to Modify

- `client/src/app/features/reservations/index.ts` — Export `ReservationListComponent`.

## Technical Details

### Code Snippets

```html
<!-- reservation-list.component.html -->
<h2>My Reservations</h2>
@if (store.reservations().length === 0) {
  <p>You have no reservations yet.</p>
}
<mat-list>
  @for (r of store.reservations(); track r.reservationId) {
    <mat-list-item>
      <span matListItemTitle>{{ r.restaurantName }}</span>
      <span matListItemLine>{{ r.date }} at {{ r.time }} · {{ r.partySize }} people</span>
      <span matListItemMeta>
        <mat-chip [color]="r.status === 'Confirmed' ? 'primary' : 'warn'" highlighted>
          {{ r.status }}
        </mat-chip>
        @if (r.status === 'Confirmed') {
          <button mat-stroked-button color="warn" (click)="onCancel(r.reservationId)">
            Cancel
          </button>
        }
      </span>
    </mat-list-item>
  }
</mat-list>
```

```typescript
@Component({
  selector: 'app-reservation-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatListModule, MatChipsModule, MatButtonModule],
  templateUrl: './reservation-list.component.html',
})
export class ReservationListComponent {
  protected readonly store = inject(ReservationsStore);

  constructor() {
    this.store.loadReservations();
  }

  onCancel(id: string): void {
    // Dialog hookup added by task-03
  }
}
```

## Acceptance Criteria

- [ ] Component calls `store.loadReservations()` on construction.
- [ ] Each reservation shows `restaurantName`, `date`, `time`, `partySize`, and `status` chip.
- [ ] "Cancel" button is only shown for `status === 'Confirmed'` reservations.
- [ ] Component uses `OnPush` change detection.
- [ ] Empty state message shown when reservations array is empty.
