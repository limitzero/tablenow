# Task 01: Reservations Store & Service

## Status

pending

## Wave

1

## Description

Creates `ReservationsService` (HTTP calls) and `ReservationsStore` (NgRx Signal Store) for the dashboard. The store exposes `loadReservations()` and `cancelReservation(id)` methods. `ReservationService.cancelReservation` from STORY-015 handles the HTTP DELETE — this store wraps it.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-002 scaffold, STORY-015 ReservationService for cancel)
**Blocks:** task-02-reservations-dashboard-component.md

**Context from dependencies:** STORY-016 created `GET /api/reservations/my`. STORY-017 created `DELETE /api/reservations/{id}`. `MyReservationDto` shape: `{reservationId, restaurantName, date, time, partySize, status}`. This task creates new files in `features/reservations/` that don't overlap with STORY-015's service file (different service class).

## Files to Create

- `client/src/app/features/reservations/services/reservations.service.ts` (list + cancel — distinct from `reservation.service.ts` which handles create)
- `client/src/app/features/reservations/store/reservations.store.ts`
- Update `client/src/app/features/reservations/models/reservation.model.ts` — add `MyReservationDto`

## Technical Details

### Code Snippets

```typescript
// services/reservations.service.ts (different from reservation.service.ts)
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { MyReservationDto } from '../models/reservation.model';

@Injectable({ providedIn: 'root' })
export class ReservationsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/reservations`;

  getMyReservations() {
    return this.http.get<MyReservationDto[]>(`${this.base}/my`);
  }

  cancelReservation(id: string) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
```

```typescript
// Add to models/reservation.model.ts:
export interface MyReservationDto {
  reservationId: string;
  restaurantName: string;
  date: string;
  time: string;
  partySize: number;
  status: 'Confirmed' | 'Cancelled';
}
```

```typescript
// store/reservations.store.ts
export const ReservationsStore = signalStore(
  { providedIn: 'root' },
  withState({ reservations: [] as MyReservationDto[], loading: false }),
  withMethods((store, service = inject(ReservationsService)) => ({
    loadReservations() {
      patchState(store, { loading: true });
      service.getMyReservations().subscribe({
        next: (reservations) => patchState(store, { reservations, loading: false }),
        error: () => patchState(store, { loading: false }),
      });
    },
    cancelReservation(id: string) {
      service.cancelReservation(id).subscribe({
        next: () => patchState(store, {
          reservations: store.reservations().map(r =>
            r.reservationId === id ? { ...r, status: 'Cancelled' as const } : r
          )
        }),
      });
    },
  }))
);
```

## Acceptance Criteria

- [ ] `ReservationsService` has `getMyReservations()` and `cancelReservation(id)`
- [ ] `ReservationsStore` has `reservations`, `loading` state
- [ ] `loadReservations()` calls GET /my
- [ ] `cancelReservation(id)` calls DELETE and updates local state optimistically
- [ ] `MyReservationDto` added to models file
