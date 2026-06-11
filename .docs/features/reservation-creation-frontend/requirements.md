# Requirements: Reservation Creation — Frontend

## Summary

Users select a time slot from the detail page and confirm the booking in a dialog. The dialog shows restaurant name, date, time, and party size. On 409, the slot list refreshes and an error is displayed. Loading state during API call.

## Goals

- Selected slot triggers confirmation dialog
- Dialog shows booking summary
- Confirm Booking calls `POST /api/reservations`
- Success navigates to My Reservations with toast
- 409 shows error and refreshes slot list

## Non-Goals

- No group booking
- No payment flow

## Acceptance Criteria

- [ ] Clicking a slot opens confirmation dialog
- [ ] "Confirm Booking" POSTs to `/api/reservations`
- [ ] Success: navigate to `/reservations` with success toast
- [ ] 409: error message + slot list refresh
- [ ] Loading indicator during request

## Technical Constraints

- Feature folder: `client/src/app/features/reservations/`
- Use Angular Material Dialog (`MatDialog`)
