# TableNow — Implementation Plan

This file is the master task index for agent-driven implementation. Agents should pick up tasks in phase order (Phase 1 before Phase 2, etc.), mark tasks `in-progress` before starting, and `complete` when done.

Status legend: `pending` | `in-progress` | `complete`

---

## Phase 1 — MVP Core (STORY-001 through STORY-019)

### Wave 1A — Scaffolding (no dependencies)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-001 | [task-01-solution-structure](story-001-backend-scaffolding/tasks/task-01-solution-structure.md) | .NET 10 solution + all project files |
| pending | STORY-002 | [task-01-angular-project-init](story-002-frontend-scaffolding/tasks/task-01-angular-project-init.md) | Angular 21 project with bootstrapApplication |

### Wave 1B — Foundation types (depend on 1A)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-001 | [task-02-shared-result-type](story-001-backend-scaffolding/tasks/task-02-shared-result-type.md) | Result&lt;T&gt; and TypedResultHelper |
| pending | STORY-001 | [task-03-module-registration](story-001-backend-scaffolding/tasks/task-03-module-registration.md) | Module self-registration + appsettings |
| pending | STORY-002 | [task-02-folder-structure-theme](story-002-frontend-scaffolding/tasks/task-02-folder-structure-theme.md) | Feature folder structure + Material theme |

### Wave 1C — Database schema (depends on STORY-001 complete)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-003 | [task-01-domain-entities](story-003-database-schema-ef-core-migrations/tasks/task-01-domain-entities.md) | User, Restaurant, TimeSlot, Reservation entities |

### Wave 1D — EF configuration (depends on STORY-003 task-01)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-003 | [task-02-ef-models-configs](story-003-database-schema-ef-core-migrations/tasks/task-02-ef-models-configs.md) | EF Fluent API configs + AppDbContext |

### Wave 1E — Migrations (depends on STORY-003 task-02)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-003 | [task-03-migrations-project](story-003-database-schema-ef-core-migrations/tasks/task-03-migrations-project.md) | Migrations project + initial migration |

### Wave 1F — Seed data (depends on STORY-003 complete; parallel tasks)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-004 | [task-01-restaurant-timeslot-seed](story-004-database-seed-data/tasks/task-01-restaurant-timeslot-seed.md) | 15+ restaurants + 30 days of time slots |
| pending | STORY-004 | [task-02-user-accounts-seed](story-004-database-seed-data/tasks/task-02-user-accounts-seed.md) | Test user accounts (parallel) |

### Wave 1G — Auth backend application layer (depends on STORY-001 + STORY-003)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-005 | [task-01-register-user-handler](story-005-user-registration-backend/tasks/task-01-register-user-handler.md) | RegisterUser application + data handlers |
| pending | STORY-006 | [task-01-jwt-helper](story-006-user-sign-in-backend/tasks/task-01-jwt-helper.md) | JwtOptions + JwtTokenGenerator |

### Wave 1H — Auth backend handlers (depend on Wave 1G)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-005 | [task-02-register-user-endpoint](story-005-user-registration-backend/tasks/task-02-register-user-endpoint.md) | POST /api/auth/register + BDD tests |
| pending | STORY-006 | [task-02-login-handler](story-006-user-sign-in-backend/tasks/task-02-login-handler.md) | LoginRequest + LoginRequestHandler |

### Wave 1I — Auth endpoints + JWT middleware (depend on Wave 1H)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-006 | [task-03-login-endpoint](story-006-user-sign-in-backend/tasks/task-03-login-endpoint.md) | POST /api/auth/login + BDD test |
| pending | STORY-007 | [task-01-jwt-middleware](story-007-jwt-middleware-route-protection/tasks/task-01-jwt-middleware.md) | JWT bearer middleware + CORS |

### Wave 1J — Auth frontend (depends on STORY-002 + STORY-006; parallel tasks)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-008 | [task-01-auth-service](story-008-auth-feature-frontend/tasks/task-01-auth-service.md) | AuthService with isAuthenticated signal |
| pending | STORY-008 | [task-02-register-component](story-008-auth-feature-frontend/tasks/task-02-register-component.md) | Register form component |
| pending | STORY-008 | [task-03-login-component](story-008-auth-feature-frontend/tasks/task-03-login-component.md) | Login form component |
| pending | STORY-009 | [task-01-auth-interceptor](story-009-jwt-interceptor-route-guard/tasks/task-01-auth-interceptor.md) | HTTP interceptor with Bearer token |
| pending | STORY-009 | [task-02-auth-guard](story-009-jwt-interceptor-route-guard/tasks/task-02-auth-guard.md) | CanActivate route guard |

### Wave 1K — Auth routes (depends on STORY-008 Wave 1J)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-008 | [task-04-auth-routes](story-008-auth-feature-frontend/tasks/task-04-auth-routes.md) | /register and /login route config |

### Wave 1L — Restaurant backend (depends on STORY-004 seed data; parallel tasks)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-010 | [task-01-get-restaurants-handler](story-010-restaurant-listing-backend/tasks/task-01-get-restaurants-handler.md) | GetRestaurants + GetRestaurantById queries |
| pending | STORY-011 | [task-01-get-available-slots-handler](story-011-slot-availability-backend/tasks/task-01-get-available-slots-handler.md) | GetAvailableSlots query with EF filter |

### Wave 1M — Restaurant endpoints (depend on Wave 1L; parallel tasks)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-010 | [task-02-restaurants-endpoints](story-010-restaurant-listing-backend/tasks/task-02-restaurants-endpoints.md) | GET /api/restaurants endpoints + test |
| pending | STORY-011 | [task-02-slots-endpoint](story-011-slot-availability-backend/tasks/task-02-slots-endpoint.md) | GET /api/restaurants/{id}/slots + test |

### Wave 1N — Restaurant frontend (depends on STORY-009 + STORY-010)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-012 | [task-01-restaurants-store-service](story-012-restaurant-listing-frontend/tasks/task-01-restaurants-store-service.md) | RestaurantsStore + RestaurantsService |

### Wave 1O — Restaurant list UI (depends on Wave 1N; parallel tasks)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-012 | [task-02-restaurant-list-component](story-012-restaurant-listing-frontend/tasks/task-02-restaurant-list-component.md) | Restaurant card grid + cuisine filter |
| pending | STORY-012 | [task-03-restaurant-routes](story-012-restaurant-listing-frontend/tasks/task-03-restaurant-routes.md) | /restaurants route config |

### Wave 1P — Restaurant detail UI (depends on STORY-012)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-013 | [task-01-detail-component-scaffold](story-013-restaurant-detail-availability-frontend/tasks/task-01-detail-component-scaffold.md) | Restaurant detail component (info display) |

### Wave 1Q — Availability form (depends on STORY-013 task-01 + STORY-011)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-013 | [task-02-availability-form-slots](story-013-restaurant-detail-availability-frontend/tasks/task-02-availability-form-slots.md) | Date/party-size form + slot list |

### Wave 1R — Reservation creation (depends on STORY-007 + STORY-011; parallel tasks)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-014 | [task-01-create-reservation-handler](story-014-reservation-creation-backend/tasks/task-01-create-reservation-handler.md) | Application layer handler + validator |
| pending | STORY-014 | [task-02-create-reservation-command](story-014-reservation-creation-backend/tasks/task-02-create-reservation-command.md) | Data layer command with EF concurrency |

### Wave 1S — Reservation endpoint + tests (depend on Wave 1R; parallel tasks)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-014 | [task-03-reservation-endpoint](story-014-reservation-creation-backend/tasks/task-03-reservation-endpoint.md) | POST /api/reservations endpoint |
| pending | STORY-014 | [task-04-reservation-unit-tests](story-014-reservation-creation-backend/tasks/task-04-reservation-unit-tests.md) | BDD unit tests for reservation creation |

### Wave 1T — Booking UI (depends on STORY-013 + STORY-014)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-015 | [task-01-reservation-service](story-015-reservation-creation-frontend/tasks/task-01-reservation-service.md) | ReservationService + confirmation dialog |

### Wave 1U — Booking flow (depends on Wave 1T)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-015 | [task-02-booking-confirmation-flow](story-015-reservation-creation-frontend/tasks/task-02-booking-confirmation-flow.md) | Slot click → dialog → POST → navigate |

### Wave 1V — Dashboard backend (depends on STORY-014)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-016 | [task-01-get-my-reservations-handler](story-016-my-reservations-dashboard-backend/tasks/task-01-get-my-reservations-handler.md) | GetMyReservations query + handler |
| pending | STORY-019 | [task-01-integration-test-infra](story-019-concurrency-integration-test/tasks/task-01-integration-test-infra.md) | api_fixture base class + test helpers |

### Wave 1W — Dashboard + cancellation (depend on Wave 1V)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-016 | [task-02-my-reservations-endpoint](story-016-my-reservations-dashboard-backend/tasks/task-02-my-reservations-endpoint.md) | GET /api/reservations/my |
| pending | STORY-017 | [task-01-cancel-reservation-handler](story-017-reservation-cancellation-backend/tasks/task-01-cancel-reservation-handler.md) | CancelReservation command + handler |
| pending | STORY-019 | [task-02-concurrent-booking-test](story-019-concurrency-integration-test/tasks/task-02-concurrent-booking-test.md) | Concurrent booking integration test |

### Wave 1X — Cancel endpoint (depends on STORY-017 task-01)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-017 | [task-02-cancel-endpoint](story-017-reservation-cancellation-backend/tasks/task-02-cancel-endpoint.md) | DELETE /api/reservations/{id} + BDD tests |

### Wave 1Y — Reservations frontend (depends on STORY-009 + STORY-016 + STORY-017)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-018 | [task-01-reservations-store](story-018-my-reservations-dashboard-frontend/tasks/task-01-reservations-store.md) | ReservationsStore + ReservationsService |

### Wave 1Z — Reservations dashboard (depends on Wave 1Y)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-018 | [task-02-reservations-dashboard-component](story-018-my-reservations-dashboard-frontend/tasks/task-02-reservations-dashboard-component.md) | Dashboard with cancel flow + guarded route |

---

## Phase 2 — Notifications & Calendar (STORY-020 through STORY-022)

### Wave 2A — Email service (depends on STORY-007)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-020 | [task-01-email-service-abstraction](story-020-email-service-integration/tasks/task-01-email-service-abstraction.md) | IEmailService + SendGridEmailService |

### Wave 2B — Email registration (depends on Wave 2A)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-020 | [task-02-email-registration](story-020-email-service-integration/tasks/task-02-email-registration.md) | EmailOptions + DI registration |

### Wave 2C — Email templates + reminder (depends on STORY-020; parallel tasks)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-021 | [task-01-email-template](story-021-booking-confirmation-email/tasks/task-01-email-template.md) | BookingConfirmation.html + IcsGenerator |
| pending | STORY-022 | [task-01-reminder-service](story-022-24-hour-reminder-email/tasks/task-01-reminder-service.md) | ReminderBackgroundService |
| pending | STORY-022 | [task-02-reminder-template](story-022-24-hour-reminder-email/tasks/task-02-reminder-template.md) | Reminder.html + service registration |

### Wave 2D — Confirmation email integration (depends on Wave 2C task-01 + STORY-014)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-021 | [task-02-handler-integration](story-021-booking-confirmation-email/tasks/task-02-handler-integration.md) | Trigger email in CreateReservationRequestHandler |

---

## Phase 3 — Reviews & Menu Photos (STORY-023 through STORY-026)

### Wave 3A — Entity migrations (no new deps beyond STORY-003)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-023 | [task-01-review-entity-migration](story-023-user-reviews-backend/tasks/task-01-review-entity-migration.md) | Review entity + EF migration |
| pending | STORY-025 | [task-01-photo-entity-migration](story-025-menu-photo-upload-backend/tasks/task-01-photo-entity-migration.md) | Photo entity + EF migration |

### Wave 3B — Reviews handlers + photo upload (depend on Wave 3A; parallel tasks)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-023 | [task-02-submit-review-handler-endpoint](story-023-user-reviews-backend/tasks/task-02-submit-review-handler-endpoint.md) | POST /api/restaurants/{id}/reviews |
| pending | STORY-023 | [task-03-get-reviews-handler-endpoint](story-023-user-reviews-backend/tasks/task-03-get-reviews-handler-endpoint.md) | GET /api/restaurants/{id}/reviews |
| pending | STORY-025 | [task-02-upload-handler-validation](story-025-menu-photo-upload-backend/tasks/task-02-upload-handler-validation.md) | Upload handler + file validation |

### Wave 3C — Photos endpoint (depends on STORY-025 task-02)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-025 | [task-03-photos-endpoint](story-025-menu-photo-upload-backend/tasks/task-03-photos-endpoint.md) | POST /api/restaurants/{id}/photos |

### Wave 3D — Reviews + gallery frontend (depend on STORY-023/025 backends; parallel tasks)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-024 | [task-01-review-list-component](story-024-user-reviews-frontend/tasks/task-01-review-list-component.md) | Review list display component |
| pending | STORY-024 | [task-02-review-form-component](story-024-user-reviews-frontend/tasks/task-02-review-form-component.md) | Review submission form |
| pending | STORY-026 | [task-01-photo-gallery-component](story-026-photo-gallery-frontend/tasks/task-01-photo-gallery-component.md) | Photo gallery with lightbox |

### Wave 3E — Integration into detail page (depend on Wave 3D)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-024 | [task-03-reviews-integration](story-024-user-reviews-frontend/tasks/task-03-reviews-integration.md) | Add reviews to restaurant detail page |
| pending | STORY-026 | [task-02-gallery-integration](story-026-photo-gallery-frontend/tasks/task-02-gallery-integration.md) | Add gallery to restaurant detail page |

---

## Phase 4 — Favorites & Popularity (STORY-027 through STORY-028)

### Wave 4A — Backend endpoints (no new deps; parallel tasks)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-027 | [task-01-popular-restaurants-endpoint](story-027-popularity-rankings-favorites/tasks/task-01-popular-restaurants-endpoint.md) | GET /api/restaurants/popular with caching |
| pending | STORY-028 | [task-01-favorites-backend](story-028-personal-bookmarks-saved-restaurants/tasks/task-01-favorites-backend.md) | UserFavorite entity + POST/DELETE/GET favorites |

### Wave 4B — Frontend (depend on Wave 4A; parallel tasks)

| Status | Story | Task File | Description |
|--------|-------|-----------|-------------|
| pending | STORY-027 | [task-02-popularity-frontend-section](story-027-popularity-rankings-favorites/tasks/task-02-popularity-frontend-section.md) | "Most Booked" section on restaurant listing |
| pending | STORY-028 | [task-02-favorites-frontend](story-028-personal-bookmarks-saved-restaurants/tasks/task-02-favorites-frontend.md) | FavoritesStore + bookmark icons on cards |

---

## Summary

| Phase | Stories | Tasks | Description |
|-------|---------|-------|-------------|
| 1 | 001–019 | 47 | Complete MVP reservation flow |
| 2 | 020–022 | 6 | Email notifications and calendar |
| 3 | 023–026 | 10 | Reviews and photo gallery |
| 4 | 027–028 | 4 | Popularity rankings and bookmarks |
| **Total** | **28** | **67** | |
