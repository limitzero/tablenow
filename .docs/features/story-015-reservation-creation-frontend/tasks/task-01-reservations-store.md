# Task 01: Reservations Signal Store

## Status

pending

## Wave

1

## Description

Create the `reservations` feature's state container: an NgRx Signal Store slice (`reservations.store.ts`) plus a `ReservationsService` for HTTP access and the feature models. The store owns the booking lifecycle — the currently selected slot, the in-flight booking status, the list of the user's reservations, and the last error. It exposes a `createReservation` action that issues the booking request through the service, and a `clearSelectedSlot` action. Critically, when the backend returns 409 (slot sold out between selection and confirmation), the store clears the selected slot and records a friendly error so the UI can refresh availability. This store is the contract that the confirmation dialog (task 02) and routes (task 03) build on.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-03-reservation-routes.md

**Context from dependencies:** None. This is a Wave 1 task. It relies on existing project foundations from prior stories: STORY-002 established the Angular 21 standalone-component app with NgRx Signal Store and Angular Material; STORY-009 added a JWT HTTP interceptor that attaches `Authorization: Bearer <token>` to all API requests automatically (so this service does not manage tokens); the API base URL is available via `environment.apiBaseUrl` (`http://localhost:5000/api`). The backend endpoint `POST /api/reservations` (STORY-014) accepts `{ slotId, partySize }` and returns 201 with reservation details or 409 Conflict with the message "This time slot is no longer available." when capacity is insufficient.

## Files to Create

- `client/src/app/features/reservations/models/reservation.models.ts` — TypeScript interfaces/types for booking state, requests, and responses.
- `client/src/app/features/reservations/services/reservations.service.ts` — Injectable service wrapping `HttpClient` calls to the reservations API.
- `client/src/app/features/reservations/store/reservations.store.ts` — NgRx Signal Store slice owning booking state and actions.

## Files to Modify

None.

## Technical Details

### Implementation Steps

1. Create the models file with the types listed in Code Snippets below.
2. Create `ReservationsService` using `inject(HttpClient)`. Build the URL from `environment.apiBaseUrl`. Expose:
   - `create(request: CreateReservationRequest): Promise<ReservationResponse>` — `POST {apiBaseUrl}/reservations`. Use `firstValueFrom(this.http.post<ReservationResponse>(...))` so the store can `await` it (no `.subscribe()`).
3. Create `ReservationsStore` with `signalStore({ providedIn: 'root' }, ...)`:
   - `withState(initialState)` — `reservations: []`, `selectedSlot: null`, `bookingStatus: 'idle'`, `error: null`.
   - `withComputed` — `isBooking` = `bookingStatus() === 'pending'`.
   - `withMethods` — `setSelectedSlot(slot)`, `clearSelectedSlot()`, `createReservation()`.
4. In `createReservation()`: read `selectedSlot()`; if null, return null. Set `bookingStatus: 'pending'`. Call the service; on success set `bookingStatus: 'success'` and return the created reservation. On error, inspect `HttpErrorResponse.status`:
   - `409`: set `bookingStatus: 'error'`, `error: 'This time slot is no longer available.'`, and `selectedSlot: null` (so the UI can refresh availability).
   - other: set `bookingStatus: 'error'`, `error: 'Booking failed. Please try again.'`.
5. Use `patchState(store, { ... })` for all mutations. Do not throw for handled business failures — surface them via state.

### Code Snippets

`models/reservation.models.ts`:

```typescript
export type BookingStatus = 'idle' | 'pending' | 'success' | 'error';

export interface SelectedSlot {
  slotId: string;
  time: string;        // e.g. "19:30"
  date: string;        // ISO date, e.g. "2026-06-12"
  partySize: number;
  restaurantId: string;
  restaurantName: string;
}

export interface CreateReservationRequest {
  slotId: string;
  partySize: number;
}

export interface ReservationResponse {
  reservationId: string;
  restaurantName: string;
  date: string;
  time: string;
  partySize: number;
  status: 'Confirmed' | 'Cancelled';
}
```

`services/reservations.service.ts`:

```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';
import type { CreateReservationRequest, ReservationResponse } from '../models/reservation.models';

@Injectable({ providedIn: 'root' })
export class ReservationsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/reservations`;

  create(request: CreateReservationRequest): Promise<ReservationResponse> {
    return firstValueFrom(this.http.post<ReservationResponse>(this.baseUrl, request));
  }
}
```

`store/reservations.store.ts`:

```typescript
import { computed, inject } from '@angular/core';
import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { HttpErrorResponse } from '@angular/common/http';
import { ReservationsService } from '../services/reservations.service';
import type { BookingStatus, SelectedSlot, ReservationResponse } from '../models/reservation.models';

interface ReservationsState {
  reservations: ReservationResponse[];
  selectedSlot: SelectedSlot | null;
  bookingStatus: BookingStatus;
  error: string | null;
}

const initialState: ReservationsState = {
  reservations: [],
  selectedSlot: null,
  bookingStatus: 'idle',
  error: null,
};

export const ReservationsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    isBooking: computed(() => store.bookingStatus() === 'pending'),
  })),
  withMethods((store, service = inject(ReservationsService)) => ({
    setSelectedSlot(slot: SelectedSlot): void {
      patchState(store, { selectedSlot: slot, error: null, bookingStatus: 'idle' });
    },
    clearSelectedSlot(): void {
      patchState(store, { selectedSlot: null });
    },
    async createReservation(): Promise<ReservationResponse | null> {
      const slot = store.selectedSlot();
      if (!slot) return null;
      patchState(store, { bookingStatus: 'pending', error: null });
      try {
        const created = await service.create({ slotId: slot.slotId, partySize: slot.partySize });
        patchState(store, { bookingStatus: 'success', error: null });
        return created;
      } catch (err) {
        const status = err instanceof HttpErrorResponse ? err.status : 0;
        if (status === 409) {
          patchState(store, {
            bookingStatus: 'error',
            error: 'This time slot is no longer available.',
            selectedSlot: null,
          });
        } else {
          patchState(store, { bookingStatus: 'error', error: 'Booking failed. Please try again.' });
        }
        return null;
      }
    },
  })),
);
```

### API Endpoints

- `POST /api/reservations` — request `{ slotId: string, partySize: number }`; response 201 `ReservationResponse`; 409 Conflict when the slot no longer has capacity.

## Acceptance Criteria

- [ ] `reservations.store.ts` exposes state `reservations`, `selectedSlot`, `bookingStatus`, and `error`.
- [ ] The store exposes `createReservation`, `clearSelectedSlot`, and `setSelectedSlot` methods.
- [ ] `createReservation` sets `bookingStatus` to `'pending'` before the call and `'success'` after a successful response.
- [ ] On a 409 response, the store sets `error` to "This time slot is no longer available." and clears `selectedSlot`.
- [ ] `ReservationsService.create` issues `POST {apiBaseUrl}/reservations` and is the only place the HTTP call lives (no `.subscribe()` in the store; uses `firstValueFrom`).
- [ ] The store is provided in root (`{ providedIn: 'root' }`).

## Notes

- The `Authorization` header is added by the STORY-009 interceptor; do not set it here.
- `setSelectedSlot` is included so task 02/03 can populate the slot before opening the confirmation dialog; it resets `error` and `bookingStatus` to a clean state.
- Keep `bookingStatus` as the single source of truth for the loading indicator and disabled state in the dialog (task 02).
