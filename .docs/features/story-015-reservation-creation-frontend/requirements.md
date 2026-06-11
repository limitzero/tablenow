# Requirements: Reservation Creation — Frontend

## Summary

After selecting a time slot, the diner sees a confirmation step with restaurant, date, time, and party size before committing. A loading state prevents double-clicks. A 409 from the server (slot taken) shows an inline error and refreshes the slot list.

## Goals

- Clicking a slot opens a confirmation dialog
- Dialog shows restaurant name, date, time, party size
- "Confirm Booking" button posts to `POST /api/reservations`
- Success: toast + navigate to `/reservations`
- 409: inline error + close dialog + refresh slot list

## Acceptance Criteria

- [ ] Clicking a slot opens the confirmation dialog
- [ ] Dialog shows restaurant, date, time, partySize
- [ ] Loading indicator on button during POST
- [ ] 201 success: snackbar toast + navigate to /reservations
- [ ] 409: error shown, dialog closed, slot list refreshed

## Technical Constraints

- `ReservationService` in `features/reservations/services/`
- Confirmation dialog uses `MatDialog`
- `MatSnackBar` for success toast
