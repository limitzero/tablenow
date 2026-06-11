# Task 01: Reservations NgRx Signal Store

## Status

pending

## Wave

1

## Description

Creates the NgRx Signal Store slice for the reservations feature. Holds the reservations list, loading state, and methods for fetching and cancelling. TypeScript model mirrors the API response from `GET /api/reservations/my`.

## Dependencies

**Depends on:** STORY-002 task-03-environment-config.md
**Blocks:** task-03-dashboard-page.md

**Context from dependencies:** `GET /api/reservations/my` returns `[{ reservationId, restaurantName, date, time, partySize, status }]`. `DELETE /api/reservations/{id}` returns 200 or error.

## Files to Create

- `client/src/app/features/reservations/models/reservation.model.ts`
- `client/src/app/features/reservations/store/reservations.store.ts`
- `client/src/app/features/reservations/services/reservation-dashboard.service.ts`

## Technical Details

### Code Snippets

```typescript
// reservation.model.ts
export interface MyReservation {
  reservationId: string;
  restaurantName: string;
  date: string;
  time: string;
  partySize: number;
  status: 'Confirmed' | 'Cancelled';
}
```

```typescript
// reservation-dashboard.service.ts
import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { MyReservation } from '../models/reservation.model';

@Injectable({ providedIn: 'root' })
export class ReservationDashboardService {
  private readonly http = inject(HttpClient);

  getMyReservations() {
    return this.http.get<MyReservation[]>(`${environment.apiBaseUrl}/reservations/my`);
  }

  cancel(reservationId: string) {
    return this.http.delete(`${environment.apiBaseUrl}/reservations/${reservationId}`);
  }
}
```

```typescript
// reservations.store.ts
export const ReservationsStore = signalStore(
  { providedIn: 'root' },
  withState<{ reservations: MyReservation[]; loading: boolean; error: string | null }>({
    reservations: [], loading: false, error: null,
  })
);
```

## Acceptance Criteria

- [ ] `MyReservation` model exists with correct fields including typed status
- [ ] `ReservationDashboardService` wraps GET and DELETE calls
- [ ] `ReservationsStore` with `reservations`, `loading`, `error` state exists
- [ ] `npm run build` exits with code 0
