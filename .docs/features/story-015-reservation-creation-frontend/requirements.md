# Requirements: Reservation Creation — Frontend

## Summary

Diners need a clear, low-risk way to commit to a booking. After selecting an available time slot on a restaurant's detail page, the diner should see a confirmation step that restates exactly what they are about to book — restaurant name, date, time, and party size — before any reservation is created. Only when they explicitly confirm does the app issue the booking request.

This feature implements that confirmation step as an Angular Material dialog plus a dedicated `reservations` feature with an NgRx Signal Store slice that owns the booking lifecycle (selected slot, in-flight status, errors). On success the diner is taken to their "My Reservations" view with a success toast. Because slot capacity is decremented atomically on the backend (STORY-014), a slot can sell out between selection and confirmation; in that case the backend returns 409 Conflict and the frontend must show a friendly error and refresh the slot list so the diner can pick another time.

The expected outcome is a booking flow that feels deliberate and safe, never double-submits, and gracefully handles the race where a slot becomes unavailable mid-flow.

## Goals

- Show a confirmation step (Angular Material dialog) displaying restaurant name, date, time, and party size before booking.
- Issue `POST /api/reservations` only after explicit confirmation.
- Disable the confirm button and show a loading indicator while the request is in flight (prevent double-submit).
- On success, navigate to `/reservations` ("My Reservations") and show a success toast.
- On 409 Conflict, show "This time slot is no longer available.", clear the selected slot, and refresh the slot list.
- Own booking state in a new `reservations.store.ts` NgRx Signal Store slice.

## Non-Goals

- The "My Reservations" dashboard UI itself — that is STORY-018. This feature only navigates to its route.
- Reservation cancellation — that is STORY-017 (backend) and STORY-018 (frontend).
- The backend reservation creation endpoint — that is STORY-014.
- The restaurant detail page and slot selector — that is STORY-013. This feature integrates with the existing slot-selection output.

## Acceptance Criteria

- [ ] Given a selected time slot, when tapped, then a confirmation dialog shows restaurant name, date, time, and party size.
- [ ] Given the confirmation screen, when "Confirm Booking" is clicked, then a `POST /api/reservations` is issued.
- [ ] Given a successful booking, when returned, then the user is navigated to `/reservations` with a success toast.
- [ ] Given a 409 response, when received, then an error message is shown, the selected slot is cleared, and the slot list is refreshed.
- [ ] Given the confirmation is in progress, when waiting for the API, then a loading indicator is shown and the confirm button is disabled.

## Assumptions

- STORY-013 (restaurant detail + slot selector) exposes a selected slot with `slotId`, `time`, plus access to the restaurant `name`, selected `date`, and `partySize` so the confirmation can be populated.
- STORY-014 (reservation creation backend) is available at `POST /api/reservations` accepting `{ slotId, partySize }`, returning 201 with reservation details or 409 with the message "This time slot is no longer available.".
- STORY-009 has provided the JWT interceptor, so the `Authorization` header is attached automatically; this feature does not manage tokens.
- A toast/snackbar mechanism is available via Angular Material's `MatSnackBar`.
- The slots endpoint used by STORY-013 can be re-invoked to refresh availability after a 409 (via the existing restaurants/slots data flow keyed on `{date, partySize}`).

## Technical Constraints

- Angular 21 standalone components only (no NgModules); `bootstrapApplication` app.
- Every component uses `OnPush` change detection.
- Dependency injection via the `inject()` function — no constructor injection.
- Template control flow uses `@if` / `@for` — never `*ngIf` / `*ngFor`.
- No `.subscribe()` in components; use `httpResource()` or the async pipe. API access goes through services only.
- State management via NgRx Signal Store — one store slice per feature (`reservations.store.ts`).
- Angular Material with the custom theme for all UI (dialog, buttons, progress indicator, snackbar).
- Feature folder: `client/src/app/features/reservations/` with `components/`, `services/`, `store/`, `models/`, `routes/`, and a required barrel `index.ts`.
