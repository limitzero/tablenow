# Implementation Plan: Reservation Creation — Frontend

## Overview

Build the diner booking confirmation flow in a new `reservations` feature. A new NgRx Signal Store slice owns the booking lifecycle; a Material dialog presents the confirmation step and drives the `POST /api/reservations` call; routes and the slot-selection flow are wired together to launch the dialog from a selected slot.

## Phase 1: Store & Confirmation Dialog (parallel)

Establish the state container and the confirmation UI independently. These tasks touch disjoint folders (`store/` and `services/` vs. `components/`), so they run in parallel.

### Tasks

- [ ] Task 01 — Create `reservations.store.ts` NgRx Signal Store slice plus a `ReservationsService` and models. State: `reservations[]`, `selectedSlot`, `bookingStatus` (`'idle' | 'pending' | 'success' | 'error'`), `error`. Methods: `createReservation(payload)`, `clearSelectedSlot()`, `setSelectedSlot(slot)`. On 409 the store clears `selectedSlot`, sets an error message, and signals the caller to refresh the slot list.
- [ ] Task 02 — Create `BookingConfirmationComponent` as a Material dialog showing restaurant name, date, time, party size. "Confirm Booking" button with loading/disabled state during the API call. On success: navigate to `/reservations` + success snackbar. On 409: show the conflict error and request a slot-list refresh.

### Technical Details

- Feature folder layout: `client/src/app/features/reservations/{components,services,store,models,routes}/` + `index.ts`.
- HTTP via `ReservationsService` (no `.subscribe()` in components; the store calls the service).
- `bookingStatus` gates the confirm button (`disabled` when `'pending'`) and the `mat-progress-spinner`/`mat-progress-bar`.
- 409 detection: inspect `HttpErrorResponse.status === 409`.

### Code Snippets

Models (`models/reservation.models.ts`):

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

Signal Store skeleton (`store/reservations.store.ts`):

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
          // Slot sold out mid-flow: clear selection so the slot list can refresh.
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

## Phase 2: Routes & Integration

Wire the dialog into the slot-selection flow and expose the feature routes.

### Tasks

- [ ] Task 03 — Define reservation feature routes (`routes/reservations.routes.ts`), open `BookingConfirmationComponent` via `MatDialog` when a slot is selected in the STORY-013 detail flow, and add the feature barrel export `index.ts`.

### Technical Details

- `MatDialog.open(BookingConfirmationComponent, { data: selectedSlot })`; the dialog reads the slot via `MAT_DIALOG_DATA` or the store's `selectedSlot`.
- On 409 the integration point re-invokes the slots fetch (the STORY-013 `httpResource()` keyed on `{date, partySize}`) so refreshed availability renders.
- Route `/reservations` is registered by STORY-018; this task only navigates to it (`Router.navigate(['/reservations'])`).
