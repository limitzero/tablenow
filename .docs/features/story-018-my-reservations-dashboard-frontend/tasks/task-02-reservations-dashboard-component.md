# Task 02: Dashboard Component & Routes

## Status

pending

## Wave

2

## Description

Creates `ReservationsDashboardComponent` with a Material list, status chips, and cancel flow. Creates the route guarded by `authGuard`. Wires the route into `app.routes.ts`.

## Dependencies

**Depends on:** task-01-reservations-store.md
**Blocks:** Nothing (this completes the Phase 1 MVP reservation management flow)

**Context from dependencies:** task-01 created `ReservationsStore` with `loadReservations()` and `cancelReservation(id)`. `authGuard` from STORY-009 is in `core/guards/auth.guard.ts`. `MatDialog` is used for the cancel confirmation prompt (same pattern as STORY-015 booking dialog).

## Files to Create

- `client/src/app/features/reservations/components/reservations-dashboard.component.ts`
- `client/src/app/features/reservations/routes/reservations.routes.ts`
- `client/src/app/features/reservations/index.ts`

## Files to Modify

- `client/src/app/app.routes.ts`

## Technical Details

### Code Snippets

```typescript
// components/reservations-dashboard.component.ts
@Component({
  selector: 'app-reservations-dashboard',
  standalone: true,
  imports: [MatListModule, MatChipsModule, MatButtonModule, MatProgressSpinnerModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-container">
      <h1>My Reservations</h1>
      @if (store.loading()) {
        <mat-spinner />
      } @else if (store.reservations().length === 0) {
        <p>You have no reservations yet.</p>
      } @else {
        <mat-list>
          @for (res of store.reservations(); track res.reservationId) {
            <mat-list-item>
              <span matListItemTitle>{{ res.restaurantName }}</span>
              <span matListItemLine>{{ res.date }} at {{ res.time | slice:0:5 }} — Party of {{ res.partySize }}</span>
              <div matListItemMeta>
                <mat-chip [color]="res.status === 'Confirmed' ? 'primary' : undefined">
                  {{ res.status }}
                </mat-chip>
                @if (res.status === 'Confirmed') {
                  <button mat-button color="warn" (click)="cancelReservation(res)">Cancel</button>
                }
              </div>
            </mat-list-item>
          }
        </mat-list>
      }
    </div>
  `,
})
export class ReservationsDashboardComponent implements OnInit {
  protected readonly store = inject(ReservationsStore);
  private readonly dialog = inject(MatDialog);

  ngOnInit() { this.store.loadReservations(); }

  cancelReservation(reservation: MyReservationDto): void {
    const ref = this.dialog.open(ConfirmCancelDialogComponent, {
      data: { restaurantName: reservation.restaurantName },
    });
    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) this.store.cancelReservation(reservation.reservationId);
    });
  }
}
```

```typescript
// routes/reservations.routes.ts
export const reservationRoutes: Routes = [{
  path: 'reservations',
  loadComponent: () =>
    import('../components/reservations-dashboard.component').then(m => m.ReservationsDashboardComponent),
  canActivate: [authGuard],
}];
```

Note: A simple inline `ConfirmCancelDialogComponent` can be created in the same file or as a separate minimal component that asks "Are you sure you want to cancel this reservation?"

## Acceptance Criteria

- [ ] Dashboard renders list of reservations with status chips
- [ ] Confirmed=primary chip color, Cancelled=default chip
- [ ] Cancel button only visible when status=Confirmed
- [ ] Cancel triggers confirmation dialog, then calls `store.cancelReservation(id)`
- [ ] Route `/reservations` has `canActivate: [authGuard]`
- [ ] Route added to `app.routes.ts`
- [ ] Empty state message shown when no reservations
