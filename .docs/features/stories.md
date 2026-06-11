# TableNow — User Stories

**Source:** `.docs/prd/PRD.md`  
**Generated:** 2026-06-10  
**Total Stories:** 28

---

## Phase 1 — Core Reservation Flow (MVP)

---

### [STORY-001] Project Scaffolding — Backend

**Type**: Technical  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 1 — MVP  
**Labels**: `backend`, `infrastructure`, `dotnet`

### Description
As a developer, I want a properly scaffolded .NET 10 solution with the modular monolith structure, so that all subsequent backend stories have a consistent, ready-to-use project foundation.

### Acceptance Criteria
- [ ] Given the repo is cloned, when `dotnet build` is run from `./server`, then the solution compiles with zero errors
- [ ] Given the solution structure, when inspected, then it contains: `Api`, `Application`, `Domain`, `Data`, `Contracts`, `Shared`, `Infrastructure` projects per module layout
- [ ] Given the `Shared` project, when inspected, then it contains a `Result<T>` type with `Data`, `Errors`, `StatusCode`, and `IsSuccess` members
- [ ] Given the `appsettings.json`, when inspected, then it has `Jwt` and `ConnectionStrings` sections with no secrets committed

### Technical Notes
- Follow directory layout in `.docs/design/backend/backend-project-structure.md`
- Each business context (`Auth`, `Restaurants`, `Reservations`) is a separate class library project
- `ServiceCollectionExtensions.RegisterServices()` wires up `Add[Context]Module()` calls
- Add `.gitignore` entry for secrets / `.env` files

### Dependencies
- Blocked by: none

---

### [STORY-002] Project Scaffolding — Frontend

**Type**: Technical  
**Priority**: High  
**Complexity**: Small  
**Phase**: 1 — MVP  
**Labels**: `frontend`, `infrastructure`, `angular`

### Description
As a developer, I want an Angular 21 standalone-component project with the feature-based folder structure, so that all frontend stories have a consistent foundation.

### Acceptance Criteria
- [ ] Given the repo is cloned, when `npm run build` is run from `./client`, then the build succeeds with zero errors
- [ ] Given the project structure, when inspected, then it contains `core/`, `shared/`, and `features/` folders under `src/app/`
- [ ] Given the Angular config, when inspected, then `bootstrapApplication` is used (no `AppModule`)
- [ ] Given the environment files, when inspected, then `environment.ts` defines `apiBaseUrl` pointing to `http://localhost:5000/api`

### Technical Notes
- Use Angular 21 standalone components (`bootstrapApplication`)
- Angular Material with a custom theme for UI components
- NgRx Signal Store as the state management library
- All components must use `OnPush` change detection

### Dependencies
- Blocked by: none

---

### [STORY-003] Database Schema & EF Core Migrations

**Type**: Technical  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 1 — MVP  
**Labels**: `backend`, `database`, `ef-core`

### Description
As a developer, I want EF Core code-first entities and migrations for all core domain objects, so that the database can be created and updated repeatably.

### Acceptance Criteria
- [ ] Given the entities, when inspected, then `User`, `Restaurant`, `TimeSlot`, and `Reservation` tables exist with correct columns and foreign keys
- [ ] Given `dotnet ef database update`, when run, then the schema is applied without errors against SQLite (dev) and SQL Server (prod)
- [ ] Given the `TimeSlot` entity, when inspected, then it has a `RemainingCapacity` column and a concurrency token (row version or timestamp)
- [ ] Given `OnModelCreating`, when inspected, then it uses `ApplyConfigurationsFromAssembly` with Fluent API configurations in `Configurations/`

### Technical Notes
- Domain entities live in `Domain/<Context>/`; EF models live in `Data/<Context>/Models/`
- Do not annotate domain entities with EF attributes — use Fluent API only
- `TimeSlot.RemainingCapacity` needs an optimistic concurrency token to prevent double-booking
- Migrations project: `server/src/Migrations/`

### Dependencies
- Blocked by: [STORY-001]

---

### [STORY-004] Database Seed Data

**Type**: Technical  
**Priority**: High  
**Complexity**: Small  
**Phase**: 1 — MVP  
**Labels**: `backend`, `database`

### Description
As a developer, I want the database seeded with realistic restaurant, time-slot, and test-user data, so that the MVP is demonstrable without any manual data entry.

### Acceptance Criteria
- [ ] Given `dotnet ef database update`, when run, then at least 15 restaurants across 3–5 cuisine types are present
- [ ] Given each restaurant, when seeded, then it has 30 days of future time slots (multiple slots per day)
- [ ] Given the seed data, when inspected, then at least 2 test user accounts exist with known credentials
- [ ] Given a test slot with `RemainingCapacity = 0`, when queried with a valid party size, then it is excluded from results

### Technical Notes
- Use `Bogus` or custom seed logic in `Data/<Context>/`
- Seed runs inside `HasData` or a dedicated `IHostedService` / startup hook
- Restaurant fields: name, cuisine, address, description, thumbnailUrl

### Dependencies
- Blocked by: [STORY-003]

---

### [STORY-005] User Registration — Backend

**Type**: Feature  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 1 — MVP  
**Labels**: `backend`, `auth`, `api`

### Description
As a new visitor, I want to create an account with my name, email, and password, so that I can make and manage reservations.

### Acceptance Criteria
- [ ] Given a POST to `/api/auth/register` with valid name/email/password, when processed, then a 201 response is returned with `userId`, `name`, and `email`
- [ ] Given a registration request, when processed, then the password is hashed with BCrypt (work factor ≥ 12) and plaintext is never stored
- [ ] Given a duplicate email, when a registration is attempted, then a 409 Conflict is returned with a descriptive error
- [ ] Given an invalid request (missing fields, weak password), when submitted, then a 400 Bad Request is returned with validation details

### Technical Notes
- Handler: `RegisterUserRequest` / `RegisterUserRequestHandler` in `Application/Auth/Features/RegisterUser/`
- Password hashing: `BCrypt.Net-Next`
- Follow `Result<T>` pattern — handler returns `Result<RegisterUserResponse>`; endpoint uses `TypedResultHelper`
- BDD test: `describe_register_user` with classes `when_email_is_already_taken`, `when_request_is_valid`

### Dependencies
- Blocked by: [STORY-001], [STORY-003]

---

### [STORY-006] User Sign-In — Backend

**Type**: Feature  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 1 — MVP  
**Labels**: `backend`, `auth`, `api`

### Description
As a returning user, I want to sign in with my email and password, so that I receive a JWT to access my reservations.

### Acceptance Criteria
- [ ] Given a POST to `/api/auth/login` with valid credentials, when processed, then a 200 response is returned with `token` and `expiresAt`
- [ ] Given invalid credentials, when submitted, then a 401 Unauthorized is returned (no detail about which field is wrong)
- [ ] Given the JWT, when decoded, then it contains `userId`, `email`, and `role` claims
- [ ] Given the JWT config, when checked, then expiry is 24 hours and the secret comes from environment config (never source code)

### Technical Notes
- Handler: `LoginRequest` / `LoginResponse` / `LoginRequestHandler` in `Application/Auth/Features/Login/`
- JWT generation helper in `Infrastructure/Auth/`
- JWT secret via `IOptions<JwtOptions>` bound from `appsettings.json` + environment overrides

### Dependencies
- Blocked by: [STORY-005]

---

### [STORY-007] JWT Middleware & Route Protection — Backend

**Type**: Technical  
**Priority**: High  
**Complexity**: Small  
**Phase**: 1 — MVP  
**Labels**: `backend`, `auth`, `security`

### Description
As the system, I need JWT bearer middleware configured so that all reservation endpoints are protected and unauthenticated requests return 401.

### Acceptance Criteria
- [ ] Given an unauthenticated request to `/api/reservations`, when sent, then a 401 Unauthorized is returned
- [ ] Given a valid JWT on a protected endpoint, when sent, then the request is processed normally
- [ ] Given an expired JWT, when sent, then a 401 is returned
- [ ] Given a CORS policy, when configured, then only the known Angular origin is allowed

### Technical Notes
- `Microsoft.AspNetCore.Authentication.JwtBearer` configured in `Api/` startup
- Use `.RequireAuthorization()` on reservation endpoint groups
- CORS policy restricts to `http://localhost:4200` (dev) and production Angular origin

### Dependencies
- Blocked by: [STORY-006]

---

### [STORY-008] Auth Feature — Frontend

**Type**: Feature  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 1 — MVP  
**Labels**: `frontend`, `auth`, `angular`

### Description
As a visitor, I want Register and Login pages, so that I can create an account or sign in directly from the browser.

### Acceptance Criteria
- [ ] Given the `/register` route, when visited, then a form with name, email, and password fields is shown
- [ ] Given valid registration form data, when submitted, then the user is redirected to the restaurant listing
- [ ] Given the `/login` route, when visited, then a form with email and password fields is shown
- [ ] Given valid login credentials, when submitted, then the JWT is stored in `localStorage` and the user is redirected to the restaurant listing
- [ ] Given an auth error (duplicate email, wrong password), when returned by the API, then an inline error message is shown

### Technical Notes
- Feature folder: `client/src/app/features/auth/`
- Auth service in `core/` handles token storage and `isAuthenticated` signal
- Use `inject()` for DI, reactive forms for validation, `@if` / `@for` in templates
- No `.subscribe()` in components — use `httpResource()` or async pipe

### Dependencies
- Blocked by: [STORY-002], [STORY-006]

---

### [STORY-009] JWT Interceptor & Route Guard — Frontend

**Type**: Technical  
**Priority**: High  
**Complexity**: Small  
**Phase**: 1 — MVP  
**Labels**: `frontend`, `auth`, `angular`

### Description
As the system, I need an HTTP interceptor that attaches the JWT to all API requests and a route guard that redirects unauthenticated users to `/login`, so that auth is enforced transparently.

### Acceptance Criteria
- [ ] Given a stored JWT, when any API request is made, then the `Authorization: Bearer <token>` header is attached automatically
- [ ] Given no JWT in storage, when a guarded route is navigated to, then the user is redirected to `/login`
- [ ] Given a 401 response from the API, when received by the interceptor, then the token is cleared and the user is redirected to `/login`
- [ ] Given a valid token, when guarded routes are accessed, then navigation proceeds normally

### Technical Notes
- HTTP interceptor in `core/interceptors/auth.interceptor.ts`
- Auth guard in `core/guards/auth.guard.ts` using the `canActivate` functional API
- Auth service exposes `isAuthenticated` as a computed Signal

### Dependencies
- Blocked by: [STORY-008]

---

### [STORY-010] Restaurant Listing — Backend

**Type**: Feature  
**Priority**: High  
**Complexity**: Small  
**Phase**: 1 — MVP  
**Labels**: `backend`, `restaurants`, `api`

### Description
As the system, I need a `GET /api/restaurants` endpoint that returns all seeded restaurants, so that the frontend can render the listing page.

### Acceptance Criteria
- [ ] Given a GET to `/api/restaurants`, when called without auth, then a 200 response with all restaurants is returned
- [ ] Given the response, when inspected, then each restaurant includes `id`, `name`, `cuisine`, `address`, `description`, and `thumbnailUrl`
- [ ] Given the endpoint, when covered by an integration test, then it returns the expected number of seeded restaurants
- [ ] Given a GET to `/api/restaurants/{id}`, when called with a valid id, then a single restaurant detail object is returned

### Technical Notes
- Query: `GetRestaurantsQuery` / `GetRestaurantsQueryHandler` in `Data/Restaurants/Queries/`
- Application: `GetRestaurantsRequest` / `GetRestaurantsRequestHandler` in `Application/Restaurants/Features/GetRestaurants/`
- No auth required on restaurant endpoints
- BDD test: `describe_get_restaurants`

### Dependencies
- Blocked by: [STORY-004]

---

### [STORY-011] Slot Availability — Backend

**Type**: Feature  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 1 — MVP  
**Labels**: `backend`, `restaurants`, `reservations`, `api`

### Description
As the system, I need a `GET /api/restaurants/{id}/slots?date=&partySize=` endpoint that returns only slots with sufficient remaining capacity, so that diners see accurate availability.

### Acceptance Criteria
- [ ] Given a valid `date` and `partySize`, when queried, then only slots where `RemainingCapacity >= partySize` for that date are returned
- [ ] Given a slot with `RemainingCapacity = 0`, when queried, then it is excluded from results
- [ ] Given a missing or invalid date/partySize, when queried, then a 400 Bad Request is returned
- [ ] Given the response, when inspected, then each slot includes `slotId`, `time`, and `remainingCapacity`

### Technical Notes
- Query: `GetAvailableSlotsQuery(restaurantId, date, partySize)` in `Data/Restaurants/Queries/`
- Filter applied in EF LINQ — do not load all slots and filter in memory
- Response time target: < 300ms
- BDD test: `describe_get_available_slots` with class `when_party_size_exceeds_remaining_capacity`

### Dependencies
- Blocked by: [STORY-004]

---

### [STORY-012] Restaurant Listing — Frontend

**Type**: Feature  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 1 — MVP  
**Labels**: `frontend`, `restaurants`, `angular`

### Description
As a diner, I want to browse a grid of available restaurants with their name, cuisine type, and location, so that I can find one that fits my preference.

### Acceptance Criteria
- [ ] Given the `/restaurants` route, when visited, then a grid of restaurant cards is displayed
- [ ] Given a restaurant card, when rendered, then it shows name, cuisine, and address/neighborhood
- [ ] Given a cuisine filter control, when a value is selected, then only restaurants matching that cuisine are shown (client-side filter)
- [ ] Given a restaurant card, when clicked, then the user is navigated to `/restaurants/:id`
- [ ] Given page load on a standard connection, when measured, then it completes in < 2s

### Technical Notes
- Feature folder: `client/src/app/features/restaurants/`
- NgRx Signal Store slice: `restaurants.store.ts` holds `restaurants` and `cuisineFilter` state
- Use `httpResource()` for data fetching (no `.subscribe()`)
- Angular Material cards for UI; `@for` to iterate restaurant list

### Dependencies
- Blocked by: [STORY-002], [STORY-009], [STORY-010]

---

### [STORY-013] Restaurant Detail & Availability — Frontend

**Type**: Feature  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 1 — MVP  
**Labels**: `frontend`, `restaurants`, `angular`

### Description
As a diner, I want to view a restaurant's details, select a date and party size, and see available time slots, so that I can choose a time that works for my group.

### Acceptance Criteria
- [ ] Given the `/restaurants/:id` route, when visited, then full restaurant info (name, description, cuisine, address) is displayed
- [ ] Given a date picker defaulting to today + 1, when a date is selected, then the slot list refreshes
- [ ] Given a party size selector (1–20), when changed, then the slot list refreshes to show only slots with sufficient capacity
- [ ] Given the slot list, when rendered, then each slot shows its time and remaining seats
- [ ] Given no available slots, when the list is empty, then a "No availability for this date and party size" message is shown

### Technical Notes
- Reactive form with `date` and `partySize` controls; on value change, dispatch slot query
- Slot list updates via `httpResource()` keyed on `{date, partySize}`
- Belongs to `features/restaurants/` — a detail component with its own route

### Dependencies
- Blocked by: [STORY-012], [STORY-011]

---

### [STORY-014] Reservation Creation — Backend

**Type**: Feature  
**Priority**: High  
**Complexity**: Large  
**Phase**: 1 — MVP  
**Labels**: `backend`, `reservations`, `api`, `concurrency`

### Description
As a diner, I want to POST a reservation for a time slot, so that my table is secured and the slot capacity is decremented atomically.

### Acceptance Criteria
- [ ] Given a POST to `/api/reservations` with a valid `slotId` and `partySize`, when processed, then a 201 is returned with reservation details
- [ ] Given a slot where `RemainingCapacity < partySize`, when a booking is attempted, then a 409 Conflict is returned with "This time slot is no longer available."
- [ ] Given two simultaneous requests for the same last-capacity slot, when processed, then exactly one succeeds (201) and the other receives a 409
- [ ] Given the reservation, when created, then the `TimeSlot.RemainingCapacity` is decremented by the party size within the same DB transaction
- [ ] Given an unauthenticated request, when sent, then a 401 Unauthorized is returned

### Technical Notes
- Use EF Core optimistic concurrency on `TimeSlot` (row version / `[Timestamp]` or `IsConcurrencyToken`)
- On `DbUpdateConcurrencyException`, re-read slot — if still insufficient, return 409
- Handler: `CreateReservationRequest` / `CreateReservationRequestHandler`
- BDD test: `describe_create_reservation` → `when_slot_is_fully_booked` → `it_should_return_conflict_status`
- Integration test fires two concurrent HTTP requests and asserts exactly one 201 and one 409

### Dependencies
- Blocked by: [STORY-007], [STORY-011]

---

### [STORY-015] Reservation Creation — Frontend

**Type**: Feature  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 1 — MVP  
**Labels**: `frontend`, `reservations`, `angular`

### Description
As a diner, I want to select a time slot and confirm my booking in a confirmation step, so that I can review details before committing.

### Acceptance Criteria
- [ ] Given a selected time slot, when tapped, then a confirmation modal/step shows restaurant name, date, time, and party size
- [ ] Given the confirmation screen, when "Confirm Booking" is clicked, then a POST to `/api/reservations` is issued
- [ ] Given a successful booking, when returned, then the user is navigated to "My Reservations" with a success toast
- [ ] Given a 409 response (slot no longer available), when received, then an error message is shown and the slot list is refreshed
- [ ] Given the confirmation is in progress, when waiting for the API, then a loading indicator is shown and the button is disabled

### Technical Notes
- Confirmation step can be a dialog (Angular Material Dialog) or an inline expansion
- Feature folder: `client/src/app/features/reservations/`
- On 409: refresh slot list by re-calling the slots endpoint; clear the selected slot

### Dependencies
- Blocked by: [STORY-013], [STORY-014]

---

### [STORY-016] My Reservations Dashboard — Backend

**Type**: Feature  
**Priority**: High  
**Complexity**: Small  
**Phase**: 1 — MVP  
**Labels**: `backend`, `reservations`, `api`

### Description
As the system, I need a `GET /api/reservations/my` endpoint that returns all reservations for the authenticated user, so that the dashboard can be populated.

### Acceptance Criteria
- [ ] Given a GET to `/api/reservations/my` with a valid JWT, when processed, then all reservations for that `userId` are returned
- [ ] Given the response, when inspected, then each item includes `reservationId`, `restaurantName`, `date`, `time`, `partySize`, and `status`
- [ ] Given an unauthenticated request, when sent, then a 401 is returned
- [ ] Given a user with no reservations, when queried, then an empty array `[]` is returned with 200

### Technical Notes
- `userId` is read from the JWT claims — never from the request body
- Query: `GetMyReservationsQuery(userId)` → joins Reservation + TimeSlot + Restaurant
- Status values: `Confirmed`, `Cancelled`

### Dependencies
- Blocked by: [STORY-014]

---

### [STORY-017] Reservation Cancellation — Backend

**Type**: Feature  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 1 — MVP  
**Labels**: `backend`, `reservations`, `api`

### Description
As a diner, I want to cancel a reservation I no longer need, so that the time slot capacity is restored for other users.

### Acceptance Criteria
- [ ] Given a DELETE to `/api/reservations/{id}` with a valid JWT for the reservation owner, when processed, then a 200 is returned and the reservation status is set to `Cancelled`
- [ ] Given a cancellation, when processed, then `TimeSlot.RemainingCapacity` is restored by the party size within the same DB transaction
- [ ] Given a JWT for a different user, when the endpoint is called, then a 403 Forbidden is returned
- [ ] Given an already-cancelled reservation, when cancelled again, then a 409 Conflict is returned

### Technical Notes
- Authorization check: compare `reservationId` owner's `userId` with JWT `userId` claim
- Atomic: update reservation status + restore slot capacity in one transaction
- Command: `CancelReservationCommand` / `CancelReservationCommandHandler`
- BDD test: `describe_cancel_reservation` → `when_user_is_not_owner` → `it_should_return_forbidden`

### Dependencies
- Blocked by: [STORY-016]

---

### [STORY-018] My Reservations Dashboard — Frontend

**Type**: Feature  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 1 — MVP  
**Labels**: `frontend`, `reservations`, `angular`

### Description
As a signed-in diner, I want to view all my upcoming reservations in one place and cancel any I no longer need, so that I can manage my plans easily.

### Acceptance Criteria
- [ ] Given the `/reservations` route, when visited by an authenticated user, then a list of reservations is shown
- [ ] Given the list, when rendered, then each row shows restaurant name, date, time, party size, and status badge
- [ ] Given a "Cancel" button on a Confirmed reservation, when clicked, then a confirmation prompt is shown before issuing the cancel request
- [ ] Given a successful cancellation, when returned, then the reservation's status updates to "Cancelled" and the button is removed
- [ ] Given the route is accessed without authentication, when navigated to, then the user is redirected to `/login`

### Technical Notes
- Feature folder: `client/src/app/features/reservations/`
- NgRx Signal Store slice: `reservations.store.ts`
- Use Angular Material table or list; status shown as a colored chip
- Route guarded by the `AuthGuard` from [STORY-009]

### Dependencies
- Blocked by: [STORY-009], [STORY-016], [STORY-017]

---

### [STORY-019] Concurrency Integration Test

**Type**: Technical  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 1 — MVP  
**Labels**: `backend`, `testing`, `concurrency`

### Description
As the system, I need an integration test that fires two simultaneous booking requests for the same fully-stocked slot, so that double-booking is provably prevented.

### Acceptance Criteria
- [ ] Given two concurrent POST requests for the same slot, when fired simultaneously, then exactly one returns 201 and the other returns 409
- [ ] Given the test, when it passes, then `TimeSlot.RemainingCapacity` is decremented by exactly one party size (not two)
- [ ] Given the integration test suite, when `dotnet test` is run, then the concurrency test is included and passes
- [ ] Given the test base class, when used, then it extends `api_fixture` (`WebApplicationFactory<Program>`) and uses a real test database

### Technical Notes
- Use `Task.WhenAll` to fire two concurrent HTTP requests from the same test
- Test class: `describe_concurrent_booking` / `when_two_users_book_the_last_slot`
- Consider using `Testcontainers` for SQL Server in integration tests

### Dependencies
- Blocked by: [STORY-014]

---

## Phase 2 — Notifications & Calendar

---

### [STORY-020] Email Service Integration

**Type**: Technical  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 2 — Notifications  
**Labels**: `backend`, `infrastructure`, `email`

### Description
As a developer, I want an email service abstraction backed by SendGrid or Mailgun, so that notification features can send transactional emails reliably.

### Acceptance Criteria
- [ ] Given a booking confirmation, when the email service is called, then an HTML email is delivered within 30 seconds
- [ ] Given a delivery failure, when it occurs, then the error is logged and does not crash the booking flow
- [ ] Given the in-app confirmation, when shown, then it appears regardless of email delivery status
- [ ] Given the email provider config, when inspected, then API keys are in environment config (not source code)

### Technical Notes
- `IEmailService` abstraction in `Infrastructure/Notifications/`
- Retry logic on transient failures; log delivery outcomes
- Provider-agnostic interface allows swapping SendGrid ↔ Mailgun

### Dependencies
- Blocked by: [STORY-007]

---

### [STORY-021] Booking Confirmation Email

**Type**: Feature  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 2 — Notifications  
**Labels**: `backend`, `email`, `reservations`

### Description
As a diner, I want to receive an HTML confirmation email with a calendar invite after booking, so that I have a reliable record of my reservation.

### Acceptance Criteria
- [ ] Given a confirmed reservation, when created, then a confirmation email is sent with restaurant name, date, time, and party size
- [ ] Given the email, when received, then it includes a `.ics` calendar file attachment
- [ ] Given the email body, when inspected, then it contains a Google Maps directions link constructed from the restaurant's address
- [ ] Given email failure, when it occurs, then the reservation is still confirmed and the user sees an in-app success message

### Technical Notes
- Trigger email after successful reservation creation in `CreateReservationRequestHandler`
- `.ics` generation: iCalendar format, include DTSTART, DTEND, SUMMARY, LOCATION
- Google Maps URL: `https://www.google.com/maps/dir/?api=1&destination=<encoded address>`
- HTML template stored in `Infrastructure/Notifications/Templates/`

### Dependencies
- Blocked by: [STORY-020], [STORY-014]

---

### [STORY-022] 24-Hour Reminder Email

**Type**: Feature  
**Priority**: Medium  
**Complexity**: Medium  
**Phase**: 2 — Notifications  
**Labels**: `backend`, `email`, `background-jobs`

### Description
As a diner, I want a reminder email 24 hours before my reservation, so that I don't forget the booking.

### Acceptance Criteria
- [ ] Given a confirmed reservation 24 hours away, when the background job runs, then a reminder email is sent to the diner
- [ ] Given a cancelled reservation, when the job runs, then no reminder is sent
- [ ] Given the background job, when inspected, then it runs on a schedule (e.g., every 15 minutes) and queries upcoming reservations
- [ ] Given email failure, when it occurs, then the failure is logged and retried on the next run

### Technical Notes
- .NET `IHostedService` or `BackgroundService` for the scheduled job
- Query reservations where `Status = Confirmed` and `DateTime` is between `now + 23h` and `now + 25h` to avoid duplicates
- Mark reminders as sent to prevent duplicate emails

### Dependencies
- Blocked by: [STORY-021]

---

## Phase 3 — Reviews & Menu Photos

---

### [STORY-023] User Reviews — Backend

**Type**: Feature  
**Priority**: Medium  
**Complexity**: Medium  
**Phase**: 3 — Reviews  
**Labels**: `backend`, `reviews`, `api`

### Description
As a registered diner, I want to submit a star rating and text review for a restaurant, so that other diners can benefit from my experience.

### Acceptance Criteria
- [ ] Given a POST to `/api/restaurants/{id}/reviews` with rating (1–5) and body text, when submitted by an authenticated user, then a 201 is returned
- [ ] Given the restaurant detail endpoint, when called, then it includes the most recent reviews sorted by timestamp (newest first)
- [ ] Given a review submission, when the rating is outside 1–5, then a 400 Bad Request is returned
- [ ] Given the reviews list, when rendered, then each review shows author name, rating, body, and timestamp

### Technical Notes
- `Review` entity: `Id`, `RestaurantId`, `UserId`, `Rating`, `Body`, `CreatedAt`
- Add `GET /api/restaurants/{id}/reviews` endpoint for paginated review listing
- Any registered user (not just past diners) can submit a review per PRD

### Dependencies
- Blocked by: [STORY-007], [STORY-003]

---

### [STORY-024] User Reviews — Frontend

**Type**: Feature  
**Priority**: Medium  
**Complexity**: Medium  
**Phase**: 3 — Reviews  
**Labels**: `frontend`, `reviews`, `angular`

### Description
As a diner, I want to read reviews and submit my own on the restaurant detail page, so that I can make informed decisions and share feedback.

### Acceptance Criteria
- [ ] Given the restaurant detail page, when viewed, then existing reviews are listed with author, stars, and text
- [ ] Given an authenticated user, when they visit the detail page, then a review submission form is shown
- [ ] Given the review form, when submitted with valid data, then the new review appears in the list immediately
- [ ] Given an unauthenticated visitor, when they view reviews, then no submission form is shown

### Technical Notes
- Add review section to existing `features/restaurants/` detail component
- Star rating input using Angular Material's rating or a custom component
- Conditionally render form based on `isAuthenticated` signal from auth service

### Dependencies
- Blocked by: [STORY-013], [STORY-023]

---

### [STORY-025] Menu Photo Upload — Backend

**Type**: Feature  
**Priority**: Medium  
**Complexity**: Large  
**Phase**: 3 — Reviews  
**Labels**: `backend`, `photos`, `api`

### Description
As a restaurant operator, I want to upload entree and menu photos associated with a time period, so that my restaurant detail page shows appealing visuals.

### Acceptance Criteria
- [ ] Given a POST to `/api/restaurants/{id}/photos` with an image file, when uploaded by an operator, then a 201 is returned with the photo URL
- [ ] Given a file exceeding the size limit (e.g., 5 MB), when uploaded, then a 400 is returned with a descriptive error
- [ ] Given a non-image file type (not JPEG/PNG/WebP), when uploaded, then a 400 is returned
- [ ] Given uploaded photos, when the restaurant detail endpoint is called, then the `photos` array is included in the response

### Technical Notes
- Server-side validation: file size ≤ 5 MB, MIME type must be image/jpeg, image/png, or image/webp
- Store files in a configured storage path (local disk for dev, blob storage for prod)
- Photo entity: `Id`, `RestaurantId`, `Url`, `UploadedAt`
- Operator role check via JWT `role` claim

### Dependencies
- Blocked by: [STORY-007], [STORY-003]

---

### [STORY-026] Photo Gallery — Frontend

**Type**: Feature  
**Priority**: Medium  
**Complexity**: Medium  
**Phase**: 3 — Reviews  
**Labels**: `frontend`, `photos`, `angular`

### Description
As a diner, I want to view a photo gallery on the restaurant detail page, so that I can get a visual sense of the food and ambiance before booking.

### Acceptance Criteria
- [ ] Given a restaurant with photos, when the detail page loads, then a gallery of photos is displayed
- [ ] Given a restaurant with no photos, when the detail page loads, then a placeholder or no gallery section is shown
- [ ] Given a photo in the gallery, when clicked, then a lightbox/enlarged view is shown
- [ ] Given photo loading, when slow, then skeleton placeholders are shown

### Technical Notes
- Add gallery section to existing `features/restaurants/` detail component
- Use Angular Material or a lightweight gallery library
- Lazy-load images with `loading="lazy"` attribute

### Dependencies
- Blocked by: [STORY-013], [STORY-025]

---

## Phase 4 — Favorites & Popularity

---

### [STORY-027] Popularity Rankings & Favorites Section

**Type**: Feature  
**Priority**: Low  
**Complexity**: Large  
**Phase**: 4 — Favorites  
**Labels**: `backend`, `frontend`, `analytics`

### Description
As a diner, I want to see a "Most Booked" section on the home page ranked by booking counts, so that I can discover trending restaurants.

### Acceptance Criteria
- [ ] Given the home page, when loaded, then a "Favorites" section shows the top-N most-booked restaurants for the current week and month
- [ ] Given the ranking query, when run, then it groups by `restaurantId` and counts confirmed reservations within the period
- [ ] Given a locale filter (city/neighborhood), when applied, then rankings are scoped to that area
- [ ] Given the section, when updated, then it refreshes at least daily (cached or recomputed by a scheduled job)

### Technical Notes
- Backend: `GET /api/restaurants/popular?period=week|month&locale=` endpoint
- Aggregation query on `Reservations` table — index on `RestaurantId` + `CreatedAt`
- Frontend: new section on the landing/home page component
- Consider a materialized view or a cached result refreshed by a scheduled job for performance

### Dependencies
- Blocked by: [STORY-010], [STORY-014]

---

### [STORY-028] Personal Bookmarks / Saved Restaurants

**Type**: Feature  
**Priority**: Low  
**Complexity**: Medium  
**Phase**: 4 — Favorites  
**Labels**: `backend`, `frontend`, `user-profile`

### Description
As a diner, I want to bookmark restaurants to a personal favorites list, so that I can quickly return to places I like.

### Acceptance Criteria
- [ ] Given a restaurant card or detail page, when a "Save" button is clicked by an authenticated user, then the restaurant is added to their favorites
- [ ] Given a saved restaurant, when the "Save" button is clicked again, then it is removed (toggle)
- [ ] Given the user dashboard, when a "My Favorites" tab is visited, then all saved restaurants are listed
- [ ] Given an unauthenticated user, when "Save" is clicked, then they are redirected to `/login`

### Technical Notes
- Backend: `UserFavorite` entity (`UserId`, `RestaurantId`, `SavedAt`); endpoints: POST/DELETE `/api/favorites/{restaurantId}`
- Frontend: save state managed in a `favorites.store.ts` NgRx Signal Store slice
- Favorite state shown on restaurant cards via a heart/bookmark icon

### Dependencies
- Blocked by: [STORY-009], [STORY-010], [STORY-018]

---

## Story Summary

| ID | Title | Phase | Priority | Complexity | Type |
|---|---|---|---|---|---|
| STORY-001 | Project Scaffolding — Backend | 1 | High | Medium | Technical |
| STORY-002 | Project Scaffolding — Frontend | 1 | High | Small | Technical |
| STORY-003 | Database Schema & EF Core Migrations | 1 | High | Medium | Technical |
| STORY-004 | Database Seed Data | 1 | High | Small | Technical |
| STORY-005 | User Registration — Backend | 1 | High | Medium | Feature |
| STORY-006 | User Sign-In — Backend | 1 | High | Medium | Feature |
| STORY-007 | JWT Middleware & Route Protection — Backend | 1 | High | Small | Technical |
| STORY-008 | Auth Feature — Frontend | 1 | High | Medium | Feature |
| STORY-009 | JWT Interceptor & Route Guard — Frontend | 1 | High | Small | Technical |
| STORY-010 | Restaurant Listing — Backend | 1 | High | Small | Feature |
| STORY-011 | Slot Availability — Backend | 1 | High | Medium | Feature |
| STORY-012 | Restaurant Listing — Frontend | 1 | High | Medium | Feature |
| STORY-013 | Restaurant Detail & Availability — Frontend | 1 | High | Medium | Feature |
| STORY-014 | Reservation Creation — Backend | 1 | High | Large | Feature |
| STORY-015 | Reservation Creation — Frontend | 1 | High | Medium | Feature |
| STORY-016 | My Reservations Dashboard — Backend | 1 | High | Small | Feature |
| STORY-017 | Reservation Cancellation — Backend | 1 | High | Medium | Feature |
| STORY-018 | My Reservations Dashboard — Frontend | 1 | High | Medium | Feature |
| STORY-019 | Concurrency Integration Test | 1 | High | Medium | Technical |
| STORY-020 | Email Service Integration | 2 | High | Medium | Technical |
| STORY-021 | Booking Confirmation Email | 2 | High | Medium | Feature |
| STORY-022 | 24-Hour Reminder Email | 2 | Medium | Medium | Feature |
| STORY-023 | User Reviews — Backend | 3 | Medium | Medium | Feature |
| STORY-024 | User Reviews — Frontend | 3 | Medium | Medium | Feature |
| STORY-025 | Menu Photo Upload — Backend | 3 | Medium | Large | Feature |
| STORY-026 | Photo Gallery — Frontend | 3 | Medium | Medium | Feature |
| STORY-027 | Popularity Rankings & Favorites Section | 4 | Low | Large | Feature |
| STORY-028 | Personal Bookmarks / Saved Restaurants | 4 | Low | Medium | Feature |
