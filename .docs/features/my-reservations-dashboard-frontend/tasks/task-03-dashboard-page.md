# Task 03: My Reservations Dashboard Page

## Status

pending

## Wave

2

## Description

Creates the My Reservations page at `/reservations`. Fetches reservations via `httpResource()`, renders `ReservationListItemComponent` for each, and handles cancellation with a confirmation prompt (Angular Material `Dialog` or `confirm()`). Route is guarded by `authGuard`.

## Dependencies

**Depends on:** task-01-reservations-store.md, task-02-reservation-list.md
**Blocks:** Nothing

**Context from dependencies:**
- task-01 created `ReservationDashboardService` with `getMyReservations()` and `cancel(id)` observables
- task-02 created `ReservationListItemComponent` with `reservation` input and `cancelRequested` output
- `authGuard` is in `client/src/app/core/guards/auth.guard.ts` (STORY-009)

## Files to Create

- `client/src/app/features/reservations/components/my-reservations/my-reservations.component.ts`
- `client/src/app/features/reservations/components/my-reservations/my-reservations.component.html`
- `client/src/app/features/reservations/routes.ts` — final routes for reservations feature
- `client/src/app/features/reservations/index.ts` — barrel export

## Files to Modify

- `client/src/app/app.routes.ts` — Apply `authGuard` to reservations route (if not already applied in STORY-009 task-02)

## Technical Details

### Code Snippets

```typescript
// my-reservations.component.ts
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { ReservationDashboardService } from '../../services/reservation-dashboard.service';
import { ReservationListItemComponent } from '../reservation-list-item/reservation-list-item.component';
import { MyReservation } from '../../models/reservation.model';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [ReservationListItemComponent],
  templateUrl: './my-reservations.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyReservationsComponent {
  private readonly reservationService = inject(ReservationDashboardService);

  readonly reservations = httpResource<MyReservation[]>(
    `${environment.apiBaseUrl}/reservations/my`
  );

  cancellingId = signal<string | null>(null);

  onCancelRequested(reservationId: string): void {
    if (!confirm('Are you sure you want to cancel this reservation?')) return;
    this.cancellingId.set(reservationId);
    this.reservationService.cancel(reservationId).subscribe({
      next: () => {
        this.cancellingId.set(null);
        this.reservations.reload();
      },
      error: () => this.cancellingId.set(null),
    });
  }
}
```

```html
<!-- my-reservations.component.html -->
<h1>My Reservations</h1>
@if (reservations.isLoading()) {
  <p>Loading reservations...</p>
} @else if (!reservations.value()?.length) {
  <p>You have no reservations yet. <a routerLink="/restaurants">Browse restaurants</a></p>
} @else {
  @for (res of reservations.value()!; track res.reservationId) {
    <app-reservation-list-item
      [reservation]="res"
      (cancelRequested)="onCancelRequested($event)" />
  }
}
```

```typescript
// routes.ts
export const RESERVATION_ROUTES: Routes = [
  { path: '', component: MyReservationsComponent },
];
```

## Acceptance Criteria

- [ ] `/reservations` renders user's reservations via `httpResource()`
- [ ] "No reservations yet" message shown for empty list
- [ ] Cancel button triggers `confirm()` dialog before calling service
- [ ] After cancel, list refreshes (`.reload()`)
- [ ] Route guarded by `authGuard`
- [ ] `npm run build` exits with code 0
