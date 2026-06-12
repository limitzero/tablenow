# Requirements: My Reservations Dashboard — Backend

## Summary

Signed-in diners need to see all the bookings they have made. This feature provides the backend read API the dashboard consumes: `GET /api/reservations/my`. It returns the full list of reservations for the authenticated user, with each item enriched with the restaurant name, the slot's date and time, the party size, and the reservation status (`Confirmed` or `Cancelled`).

The endpoint follows the established modular-monolith CQRS flow: a Minimal API endpoint dispatches an Application-layer request via `IMediator`; the Application handler resolves the caller's `userId` from the JWT claims and dispatches a Data-layer query; the Data query joins `Reservation`, `TimeSlot`, and `Restaurant` and projects the result. Authorization is enforced at the endpoint (`RequireAuthorization()`), so unauthenticated requests receive 401. A user with no reservations receives an empty array and a 200 — an empty result is not an error.

The expected outcome is a single, secure, well-shaped endpoint that the STORY-018 frontend dashboard can call to render the diner's reservation list.

## Goals

- Expose `GET /api/reservations/my` returning all reservations for the authenticated user.
- Each item includes `reservationId`, `restaurantName`, `date`, `time`, `partySize`, and `status`.
- Resolve `userId` exclusively from the JWT claims (never the request body or query string).
- Return 401 for unauthenticated requests.
- Return `200` with an empty array `[]` for a user with no reservations.
- Follow the CQRS layering: Data query → Application handler → Minimal API endpoint, all returning `Result<T>`.

## Non-Goals

- The dashboard UI — that is STORY-018.
- Reservation cancellation — that is STORY-017.
- Reservation creation — that is STORY-014.
- Pagination, filtering, or sorting controls (the MVP returns the full list).
- Distinguishing past vs. upcoming reservations server-side (out of scope for this story).

## Acceptance Criteria

- [ ] Given a GET to `/api/reservations/my` with a valid JWT, when processed, then all reservations for that `userId` are returned.
- [ ] Given the response, when inspected, then each item includes `reservationId`, `restaurantName`, `date`, `time`, `partySize`, and `status`.
- [ ] Given an unauthenticated request, when sent, then a 401 is returned.
- [ ] Given a user with no reservations, when queried, then an empty array `[]` is returned with 200.

## Assumptions

- STORY-014 created the `Reservation` entity and Data layer, with a `TimeSlot` foreign key, `PartySize`, and `Status`; `TimeSlot` references `Restaurant` and carries the slot date/time; `Restaurant` carries `Name`.
- STORY-007 configured JWT bearer authentication and the `userId` claim (and `email`, `role`) on the token, and `RequireAuthorization()` is available on endpoint groups.
- `IHttpContextAccessor` is registered (via `AddHttpContextAccessor()`) so the Application handler can read claims; this is registered in the Api startup if not already present.
- The `Reservations` module already self-registers via `AddReservationsModule()` from `ServiceCollectionExtensions.RegisterServices()` (established by STORY-014).
- Status values are `Confirmed` and `Cancelled`.

## Technical Constraints

- .NET 10 modular monolith with CQRS via Mediator (source-generated). Request flow: HTTP endpoint → `IMediator.Send(AppRequest)` → AppRequestHandler → `IMediator.Send(DataQuery)` → EF Core DbContext.
- All handlers return `Result<T>` from `CM.OpenTable.Common` (`Data`, `Errors`, `StatusCode`, `IsSuccess`). Endpoints use `TypedResultHelper`. Never throw for business-logic outcomes.
- No repository pattern — use `DbContext` directly in Data handlers.
- No AutoMapper — use the static `ReservationMapper` in the Api endpoints layer for API model translation.
- The join/projection must be performed in EF LINQ (server-side); do not load entities and map in memory unnecessarily.
- Code style: primary constructors for DI, records for DTOs/queries/responses, file-scoped namespaces, `CancellationToken` on all async methods, nullable enabled.
- `userId` is read from the JWT claims, never from the request.
