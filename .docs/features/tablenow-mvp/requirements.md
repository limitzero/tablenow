# Requirements: TableNow MVP — Core Reservation Flow

## Summary

TableNow is a dual-sided restaurant reservation platform. The MVP delivers one complete user journey: a diner registers or signs in, browses a seeded restaurant catalog, selects a date and party size on a restaurant detail page, picks an available time slot, confirms the booking, and can later view or cancel it on their dashboard.

The backend is a .NET 10 modular monolith with CQRS (Mediator), EF Core on SQL Server, and JWT authentication. The frontend is an Angular 21 SPA with standalone components, NgRx Signal Store, and Angular Material. All restaurant and time-slot data is seeded — no operator-facing management UI in Phase 1.

The most technically critical requirement is **atomic slot capacity management**: when a reservation is created or cancelled, the `TimeSlot.RemainingCapacity` must be updated in the same database transaction using optimistic concurrency so that double-booking is impossible even under concurrent requests.

## Goals

- A new user can complete the full flow (register → browse → book → view) in ≤ 4 clicks from the restaurant listing.
- Two simultaneous booking attempts for the same full slot — only one succeeds; the other gets a clear "no longer available" error.
- JWT-protected routes: unauthenticated requests to reservation endpoints return 401; cross-user cancellation returns 403.
- Page load for restaurant listing < 2s; slot availability API response < 300ms.
- `dotnet build` and `npm run build` pass cleanly from a fresh clone.

## Non-Goals

The following are explicitly deferred to later phases and must not be implemented in this spec:

- **Phase 2**: Email confirmations, `.ics` calendar attachments, reminder emails.
- **Phase 3**: User reviews, star ratings, menu/entree photo uploads.
- **Phase 4**: Popularity rankings, personal restaurant bookmarks.
- Restaurant self-service registration and profile management (any phase).
- Mobile application (iOS/Android/PWA).
- Payment processing, deposits, or waitlist management.
- Admin CMS for managing the restaurant catalog.
- OAuth/social login, two-factor auth, rate limiting.
- Real-time availability via WebSocket/SSE.

## Acceptance Criteria

- [ ] A new user can register with name, email, and password; plaintext password is never stored.
- [ ] A registered user can sign in and receive a JWT (24h expiry) stored in `localStorage`.
- [ ] The restaurant listing renders all seeded restaurants with name, cuisine, and location.
- [ ] Filtering by cuisine type narrows the listing (client-side).
- [ ] The restaurant detail page shows available slots filtered by the selected date and party size.
- [ ] Confirming a booking returns 201 and the slot's `RemainingCapacity` is decremented atomically.
- [ ] Two concurrent booking requests for the same last-capacity slot: exactly one succeeds (201), the other returns 409.
- [ ] The user's dashboard lists all their reservations with restaurant name, date, time, party size, and status.
- [ ] Cancelling a reservation sets its status to `Cancelled` and restores the slot's capacity.
- [ ] All reservation endpoints return 401 for unauthenticated requests; cancelling another user's reservation returns 403.
- [ ] `dotnet test` passes (unit + integration tests including the concurrency test).
- [ ] `npm run build` produces a production bundle with zero errors.

## Assumptions

- SQLite is used for local development; SQL Server for production. EF Core targets both via the connection string.
- The Angular dev server runs on `http://localhost:4200`; the .NET API runs on `http://localhost:5000`.
- Seed data is applied on startup (or via `dotnet ef database update`) and is idempotent.
- The JWT secret is never committed to source control — it is provided via environment variable `JWT__Secret`.
- `CM.OpenTable.Common` is the namespace used by the `TableNow.Shared` project for `Result<T>` and `TypedResultHelper`.

## Technical Constraints

- **Backend**: .NET 10 modular monolith. Business contexts (`Auth`, `Restaurants`, `Reservations`) are separate class library projects. No repository pattern. No AutoMapper in module code. No cross-module Application/Data project references — communicate only via `IMediator` and `Contracts.Public` assemblies.
- **CQRS via Mediator**: Source-generated `IMediator`. Application handlers call Data handlers. Data handlers operate directly on `DbContext`. All handlers return `Result<T>`.
- **EF**: Fluent API only (no data annotations on domain entities). `OnModelCreating` uses `ApplyConfigurationsFromAssembly`. One EF model per entity in `Data/<Context>/Models/`.
- **Frontend**: Angular 21 standalone components only (`bootstrapApplication`). `OnPush` change detection on every component. `inject()` for DI. `@if`/`@for` control flow. No `.subscribe()` in components — use `httpResource()` or `async` pipe. NgRx Signal Store for feature state.
- **Code style (backend)**: Primary constructors for DI, records for DTOs/commands, file-scoped namespaces, `CancellationToken` on all async methods, nullable enabled.
- **Test naming (backend)**: BDD — `namespace describe_[feature]`, `class when_[scenario]`, `method it_should_[expectation]`.
