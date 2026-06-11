# Task 15: My Reservations Dashboard Frontend

## Status

pending

## Phase

8

## Description

Build the authenticated `/reservations` dashboard page. It lists all the current user's reservations (restaurant name, date, time, party size, status). Confirmed reservations show a "Cancel" button; clicking it asks for confirmation then calls `DELETE /api/v1/reservations/{id}`. On success the reservation's status badge updates to "Cancelled" and the button disappears. The route is protected by `authGuard`.

## Dependencies

**Depends on:** task-10-jwt-interceptor-guard-frontend, task-11-reservation-management-backend  
**Blocks:** task-16-reservation-booking-frontend

**Context from dependencies:** task-10 created `authGuard` in `client/src/app/core/guards/auth.guard.ts`. task-11 implemented `GET /api/v1/reservations/my` (returns `[{reservationId, restaurantName, date, time, partySize, status}]`) and `DELETE /api/v1/reservations/{id}` (returns 200, 403, or 409). The JWT interceptor (task-10) automatically attaches the Bearer token.

## Files to Create

- `client/src/app/features/reservations/models/reservation.model.ts`
- `client/src/app/features/reservations/services/reservations.service.ts`
- `client/src/app/features/reservations/store/reservations.store.ts`
- `client/src/app/features/reservations/components/reservations-dashboard/reservations-dashboard.component.ts`
- `client/src/app/features/reservations/components/reservations-dashboard/reservations-dashboard.component.html`
- `client/src/app/features/reservations/components/reservations-dashboard/reservations-dashboard.component.scss`
- `client/src/app/features/reservations/routes/reservations.routes.ts`
- `client/src/app/features/reservations/index.ts`

## Files to Modify

- `client/src/app/app.routes.ts` — add guarded lazy-loaded reservations routes

## Technical Details

### Implementation Steps

1. **Define `Reservation` model interface.**
2. **Write `ReservationsService`** — wraps `GET /reservations/my` and `DELETE /reservations/{id}`.
3. **Write `reservations.store.ts`** — NgRx Signal Store with `reservations` list and `cancel` method.
4. **Write `ReservationsDashboardComponent`** — reads from store, renders table, handles cancel.
5. **Write `reservations.routes.ts`** and add to `app.routes.ts` with `authGuard`.

### Code Snippets

**`reservation.model.ts`:**
```typescript
export interface Reservation {
  reservationId: string;
  restaurantName: string;
  date: string;
  time: string;
  partySize: number;
  status: 'Confirmed' | 'Cancelled';
}
```

**`reservations.service.ts`:**
```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Reservation } from '../models/reservation.model';

@Injectable({ providedIn: 'root' })
export class ReservationsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/v1`;

  getMyReservations() {
    return this.http.get<Reservation[]>(`${this.base}/reservations/my`);
  }

  cancelReservation(id: string) {
    return this.http.delete<{ data: string }>(`${this.base}/reservations/${id}`);
  }
}
```

**`reservations.store.ts`:**
```typescript
import { signalStore, withState, withMethods } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { inject } from '@angular/core';
import { pipe, switchMap, tap } from 'rxjs';
import { ReservationsService } from '../services/reservations.service';
import { Reservation } from '../models/reservation.model';

interface ReservationsState {
  reservations: Reservation[];
  loading: boolean;
  cancellingId: string | null;
}

export const ReservationsStore = signalStore(
  { providedIn: 'root' },
  withState<ReservationsState>({
    reservations: [],
    loading: false,
    cancellingId: null,
  }),
  withMethods((store, service = inject(ReservationsService)) => ({
    loadReservations: rxMethod<void>(
      pipe(
        tap(() => patchState(store, { loading: true })),
        switchMap(() => service.getMyReservations()),
        tap(reservations => patchState(store, { reservations, loading: false }))
      )
    ),
    cancelReservation: rxMethod<string>(
      pipe(
        tap(id => patchState(store, { cancellingId: id })),
        switchMap(id => service.cancelReservation(id).pipe(
          tap(() => patchState(store, state => ({
            reservations: state.reservations.map(r =>
              r.reservationId === id ? { ...r, status: 'Cancelled' as const } : r
            ),
            cancellingId: null,
          })))
        ))
      )
    ),
  }))
);
```

**`reservations-dashboard.component.ts`:**
```typescript
import { Component, ChangeDetectionStrategy, OnInit, inject } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { ReservationsStore } from '../../store/reservations.store';
import { Reservation } from '../../models/reservation.model';

@Component({
  selector: 'app-reservations-dashboard',
  standalone: true,
  imports: [MatTableModule, MatButtonModule, MatChipsModule,
            MatProgressSpinnerModule, MatDialogModule],
  templateUrl: './reservations-dashboard.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReservationsDashboardComponent implements OnInit {
  protected readonly store = inject(ReservationsStore);
  private readonly dialog = inject(MatDialog);

  readonly displayedColumns = ['restaurant', 'date', 'time', 'partySize', 'status', 'actions'];

  ngOnInit(): void {
    this.store.loadReservations();
  }

  confirmCancel(reservation: Reservation): void {
    const confirmed = window.confirm(
      `Cancel your reservation at ${reservation.restaurantName} on ${reservation.date}?`
    );
    if (confirmed) {
      this.store.cancelReservation(reservation.reservationId);
    }
  }
}
```

**`reservations-dashboard.component.html`:**
```html
<div class="dashboard-container">
  <h1>My Reservations</h1>

  @if (store.loading()) {
    <mat-spinner />
  } @else if (store.reservations().length === 0) {
    <p>You have no reservations yet. <a routerLink="/restaurants">Find a restaurant</a></p>
  } @else {
    <table mat-table [dataSource]="store.reservations()" class="full-width">
      <ng-container matColumnDef="restaurant">
        <th mat-header-cell *matHeaderCellDef>Restaurant</th>
        <td mat-cell *matCellDef="let r">{{ r.restaurantName }}</td>
      </ng-container>

      <ng-container matColumnDef="date">
        <th mat-header-cell *matHeaderCellDef>Date</th>
        <td mat-cell *matCellDef="let r">{{ r.date }}</td>
      </ng-container>

      <ng-container matColumnDef="time">
        <th mat-header-cell *matHeaderCellDef>Time</th>
        <td mat-cell *matCellDef="let r">{{ r.time }}</td>
      </ng-container>

      <ng-container matColumnDef="partySize">
        <th mat-header-cell *matHeaderCellDef>Party</th>
        <td mat-cell *matCellDef="let r">{{ r.partySize }}</td>
      </ng-container>

      <ng-container matColumnDef="status">
        <th mat-header-cell *matHeaderCellDef>Status</th>
        <td mat-cell *matCellDef="let r">
          <mat-chip [color]="r.status === 'Confirmed' ? 'primary' : 'warn'" highlighted>
            {{ r.status }}
          </mat-chip>
        </td>
      </ng-container>

      <ng-container matColumnDef="actions">
        <th mat-header-cell *matHeaderCellDef></th>
        <td mat-cell *matCellDef="let r">
          @if (r.status === 'Confirmed') {
            <button
              mat-stroked-button color="warn"
              [disabled]="store.cancellingId() === r.reservationId"
              (click)="confirmCancel(r)">
              Cancel
            </button>
          }
        </td>
      </ng-container>

      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
    </table>
  }
</div>
```

**`reservations.routes.ts`:**
```typescript
import { Routes } from '@angular/router';

export const RESERVATION_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../components/reservations-dashboard/reservations-dashboard.component')
        .then(m => m.ReservationsDashboardComponent),
  },
];
```

**`app.routes.ts` addition (guarded):**
```typescript
import { authGuard } from './core/guards/auth.guard';

{
  path: 'reservations',
  canActivate: [authGuard],
  loadChildren: () =>
    import('./features/reservations/routes/reservations.routes')
      .then(m => m.RESERVATION_ROUTES),
},
```

**`index.ts`:**
```typescript
export { ReservationsStore } from './store/reservations.store';
export { ReservationsService } from './services/reservations.service';
```

## Acceptance Criteria

- [ ] `/reservations` requires authentication — unauthenticated navigation redirects to `/login`
- [ ] The table lists all reservations with restaurant name, date, time, party size, and status chip
- [ ] Confirmed reservations show a "Cancel" button; Cancelled ones do not
- [ ] Clicking "Cancel" shows a confirmation prompt before calling the API
- [ ] After successful cancellation, the status chip updates to "Cancelled" in place (no page reload)
- [ ] `ChangeDetectionStrategy.OnPush`; no `*ngIf`/`*ngFor`; no constructor injection
- [ ] `authGuard` applied to the reservations route in `app.routes.ts`

## Notes

- Angular Material's `mat-table` uses `*matCellDef` and `*matHeaderCellDef` — these are Material-specific structural directives, not `*ngFor`. This is intentional and acceptable.
- `window.confirm()` is used for the cancel confirmation in the MVP — an Angular Material dialog is preferred for production but is heavier to set up.
- The `cancellingId` state prevents double-clicks by disabling the button while a cancel is in flight.
