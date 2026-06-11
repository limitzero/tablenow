# Product Requirements Document
# TableNow — Restaurant Reservation Platform

**Version:** 1.0  
**Date:** 2026-06-10  
**Status:** Draft

---

## 1. Executive Summary

TableNow is a web-based restaurant reservation platform that connects diners with restaurants in their area. Diners can browse available restaurants, check open time slots, specify their party size, and confirm a booking in minutes. Restaurant operators benefit from increased visibility and a structured reservation pipeline without requiring a separate management portal in the initial release.

The platform operates as a dual-sided marketplace. Diners get a streamlined discovery and booking experience, while restaurants (initially seeded with mock data) gain a public-facing presence and a reservation record. Subsequent phases introduce notification workflows, calendar integration, user reviews with photo content, and popularity rankings — progressively deepening the value delivered to both sides.

The MVP goal is to deliver a functional, browser-based reservation flow: a diner can sign in, search restaurants, select a date and party size, choose an available time slot, and confirm a booking. All restaurant and availability data will be seeded for the MVP.

---

## 2. Mission

**Mission Statement:** Make it effortless for anyone to discover a great restaurant and secure a table — and give restaurants a dependable, low-friction way to fill their seats.

**Core Principles:**

1. **Simplicity first** — The reservation flow must be completable in as few steps as possible.
2. **Trust through transparency** — Show diners accurate availability; never double-book a slot.
3. **Dual value** — Every feature must deliver meaningful value to both diners and restaurant operators.
4. **Progressive enhancement** — Start with a solid MVP, layer on complexity only when the foundation is proven.
5. **Data integrity** — Reservations are commitments; the system must treat them with consistency and reliability.

---

## 3. Target Users

### Persona A — The Diner

- **Description:** A person planning a meal out, ranging from a solo lunch to a group celebration.
- **Technical comfort:** Moderate. Comfortable with web apps, online forms, and email confirmations.
- **Key needs:**
  - Find a restaurant that fits the occasion (cuisine, location, party size).
  - See real-time availability without calling ahead.
  - Receive a confirmation they can refer back to.
- **Pain points:**
  - Calling restaurants to check availability.
  - Uncertainty about whether a reservation was actually recorded.
  - Forgetting reservation details (time, address, date).

### Persona B — The Restaurant Operator

- **Description:** An owner or manager responsible for seating, floor management, and maximizing table turnover.
- **Technical comfort:** Variable. May rely on staff to manage digital systems.
- **Key needs:**
  - Accurate visibility into upcoming reservations.
  - Prevent overbooking specific time slots.
  - Attract new customers through the platform.
- **Pain points:**
  - Walk-ins displacing reservation holders.
  - No-shows consuming reserved slots.
  - Lack of a centralized system for tracking bookings.

---

## 4. MVP Scope

### In Scope — Phase 1 (MVP)

**Core Functionality**
- [x] User registration and sign-in (email + password)
- [x] Browse a listing of seeded restaurants
- [x] View restaurant detail page (name, cuisine, address, available time slots)
- [x] Select date and party size for a reservation
- [x] Choose an available time slot and confirm booking
- [x] View upcoming reservations on a user profile/dashboard page
- [x] Cancel an existing reservation

**Technical**
- [x] Angular 20 SPA (web browser only)
- [x] .NET backend REST API
- [x] JWT-based authentication
- [x] Seeded database (restaurants, time slots, users)
- [x] Basic server-side availability enforcement (no double-booking)

**Data**
- [x] Seeded restaurant catalog (name, cuisine type, location, description, capacity per slot)
- [x] Seeded time-slot availability per restaurant

### Out of Scope — Phase 1 (Deferred)

- [ ] Restaurant self-service registration and profile management
- [ ] Email notifications and calendar invites (Phase 2)
- [ ] Directions / map integration (Phase 2)
- [ ] User reviews and ratings (Phase 3)
- [ ] Restaurant menu / entree photo uploads (Phase 3)
- [ ] Favorites and popularity rankings (Phase 4)
- [ ] Mobile application (iOS/Android)
- [ ] Payment processing / deposits
- [ ] Waitlist management
- [ ] Admin CMS for managing restaurant catalog

---

## 5. User Stories

### Diner Stories

**US-01 — Registration**
> As a new visitor, I want to create an account with my email and password, so that I can make and manage reservations.

*Example:* Sarah visits the site for the first time, fills in her name, email, and a password, and receives a welcome confirmation. She is redirected to the restaurant listing.

**US-02 — Sign In**
> As a returning user, I want to sign in with my email and password, so that I can access my reservations.

*Example:* Marcus returns the next day, enters his credentials, and lands on his personal dashboard showing his upcoming bookings.

**US-03 — Browse Restaurants**
> As a diner, I want to browse a list of available restaurants, so that I can find one that fits my preference.

*Example:* Ana sees a grid of restaurant cards showing name, cuisine, and location. She filters by cuisine type to narrow down to Italian options.

**US-04 — View Availability**
> As a diner, I want to see available time slots for a specific date and party size, so that I can choose a time that works for my group.

*Example:* David selects a date of June 20 and a party of 4. The restaurant detail page updates to show only time slots with enough capacity for 4 guests.

**US-05 — Make a Reservation**
> As a diner, I want to select a time slot and confirm my booking, so that my table is secured.

*Example:* Lisa picks 7:30 PM on a Saturday for 2 people, reviews the details on a confirmation screen, and clicks "Confirm Booking." The slot is immediately marked unavailable to other users.

**US-06 — View My Reservations**
> As a signed-in diner, I want to view all my upcoming reservations in one place, so that I can keep track of my plans.

*Example:* James opens his dashboard and sees two upcoming reservations listed with restaurant name, date, time, and party size.

**US-07 — Cancel a Reservation**
> As a diner, I want to cancel a reservation I no longer need, so that the time slot becomes available for others.

*Example:* Elena cancels her Friday reservation 48 hours before. The slot is released and her dashboard no longer shows that booking.

### Technical Stories

**US-08 — Slot Concurrency**
> As the system, I need to prevent two users from booking the same time slot simultaneously, so that double-booking never occurs.

*Example:* Two users submit a booking for the same slot at the same second. Only one succeeds; the other receives a "Slot no longer available" message.

---

## 6. Core Architecture & Patterns

### High-Level Architecture

```
Browser (Angular 20 SPA)
        │
        │  HTTPS / REST
        ▼
.NET Web API (ASP.NET Core)
        │
        ├── Auth module (JWT)
        ├── Restaurants module
        ├── Reservations module
        └── Seed data / EF Core
        │
        ▼
   SQL Database (SQL Server / SQLite for dev)
```

### Frontend (Angular 20)

- **Standalone components** — no NgModules; use `bootstrapApplication`
- **Angular Signals** for reactive state management
- **Angular Router** with lazy-loaded feature routes
- **HttpClient** with interceptors for JWT attachment and error handling
- **Reactive Forms** for reservation flow (date picker, party size, slot selection)

**Directory Structure (Frontend)**

```
src/
  app/
    core/               # Auth service, HTTP interceptors, guards
    shared/             # Reusable components, pipes, directives
    features/
      auth/             # Login, Register pages
      restaurants/      # Restaurant list, detail pages
      reservations/     # Booking flow, My Reservations page
    app.routes.ts
    app.config.ts
```

### Backend (.NET)

- **Minimal API** or **Controller-based** REST endpoints (prefer Minimal API for .NET 9+)
- **Entity Framework Core** with code-first migrations
- **Repository pattern** for data access
- **JWT Bearer authentication** via `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Optimistic concurrency** or DB-level locking on slot booking to prevent double-booking

**Directory Structure (Backend)**

```
src/
  TableNow.Api/
    Endpoints/          # Minimal API endpoint groups
    Models/             # EF Core entities
    DTOs/               # Request/response shapes
    Services/           # Business logic
    Data/               # DbContext, seed data, migrations
    Auth/               # JWT helpers, password hashing
  TableNow.Api.Tests/
```

---

## 7. Core Features

### Feature 1 — Authentication

- Register with name, email, and password (bcrypt-hashed)
- Sign in returns a signed JWT (expiry: 24h)
- JWT stored in browser `localStorage`; attached to all API requests via Angular HTTP interceptor
- Route guard redirects unauthenticated users to `/login`

### Feature 2 — Restaurant Listing

- Grid/list view of all seeded restaurants
- Each card shows: restaurant name, cuisine type, neighborhood/city, and a thumbnail
- Optional: filter by cuisine type (client-side filter on seeded data)
- Clicking a card navigates to the restaurant detail page

### Feature 3 — Restaurant Detail & Availability

- Full restaurant info: name, description, cuisine, address
- Date picker (defaults to today + 1)
- Party size selector (1–20)
- Time slot list dynamically filtered by: selected date + party size ≤ slot remaining capacity
- Slots display: time, and available seats remaining

### Feature 4 — Reservation Booking

- Tapping a slot opens a confirmation modal/step
- Confirmation screen shows: restaurant name, date, time, party size
- On confirm: POST to `/api/reservations`
- Backend atomically decrements slot capacity; returns conflict (409) if slot is full
- Success: navigate to "My Reservations" with success toast

### Feature 5 — My Reservations Dashboard

- Lists all upcoming reservations for the authenticated user
- Each row: restaurant name, date, time, party size, status (Confirmed / Cancelled)
- "Cancel" button triggers DELETE/PATCH to cancel the reservation and restore slot capacity

---

## 8. Technology Stack

### Frontend

| Technology | Version | Purpose |
|---|---|---|
| Angular | 20.x | SPA framework |
| TypeScript | 5.x | Language |
| Angular Signals | (built-in) | Reactive state |
| Angular Router | (built-in) | Client-side routing |
| RxJS | 7.x | Async streams |
| Angular Material or PrimeNG | Latest | UI component library |
| date-fns or Luxon | Latest | Date handling |

### Backend

| Technology | Version | Purpose |
|---|---|---|
| .NET | 9 or 10 | Runtime |
| ASP.NET Core | 9/10 | Web API framework |
| Entity Framework Core | 9/10 | ORM |
| SQL Server (prod) / SQLite (dev) | — | Database |
| BCrypt.Net-Next | Latest | Password hashing |
| Microsoft.AspNetCore.Authentication.JwtBearer | Latest | JWT auth |
| Bogus or custom seed | — | Test/seed data generation |

### Dev Tooling

| Tool | Purpose |
|---|---|
| Angular CLI | Project scaffolding, serve, build |
| dotnet CLI | API project management, migrations |
| Swagger / Scalar | API documentation |
| xUnit + Testcontainers | Backend unit + integration tests |
| Jasmine + Karma / Jest | Angular unit tests |

---

## 9. Security & Configuration

### Authentication & Authorization

- Passwords hashed with BCrypt (work factor ≥ 12) before storage; plaintext never persisted
- JWT signed with a secret stored in environment config (not source code)
- JWT claims include: `userId`, `email`, `role` (diner / operator)
- All reservation endpoints require a valid JWT; unauthenticated requests return 401
- Users can only view/cancel their own reservations (authorization check by `userId` in token)

### Configuration

**Backend (`appsettings.json` + environment overrides)**

```json
{
  "Jwt": {
    "Secret": "<env-override>",
    "Issuer": "tablenow-api",
    "Audience": "tablenow-client",
    "ExpiryHours": 24
  },
  "ConnectionStrings": {
    "DefaultConnection": "<env-override>"
  }
}
```

**Frontend (`environment.ts`)**

```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000/api'
};
```

### Security Scope

**In scope (MVP):**
- [x] Password hashing (BCrypt)
- [x] JWT authentication
- [x] User-scoped authorization on reservations
- [x] HTTPS in production
- [x] CORS policy restricting to known Angular origin

**Out of scope (MVP):**
- [ ] OAuth / social login
- [ ] Rate limiting / brute-force protection (Phase 2+)
- [ ] Email verification on registration
- [ ] Two-factor authentication
- [ ] PCI-DSS compliance (no payments in MVP)

---

## 10. API Specification

### Auth

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | None | Register new user |
| POST | `/api/auth/login` | None | Sign in, returns JWT |

**POST `/api/auth/register`**
```json
// Request
{ "name": "Jane Doe", "email": "jane@example.com", "password": "Secure123!" }

// Response 201
{ "userId": "uuid", "name": "Jane Doe", "email": "jane@example.com" }
```

**POST `/api/auth/login`**
```json
// Request
{ "email": "jane@example.com", "password": "Secure123!" }

// Response 200
{ "token": "eyJ...", "expiresAt": "2026-06-11T10:00:00Z" }
```

### Restaurants

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/restaurants` | None | List all restaurants |
| GET | `/api/restaurants/{id}` | None | Get restaurant detail |
| GET | `/api/restaurants/{id}/slots?date=&partySize=` | None | Get available slots |

**GET `/api/restaurants`**
```json
// Response 200
[
  {
    "id": "uuid",
    "name": "Bella Notte",
    "cuisine": "Italian",
    "address": "123 Main St, Chicago, IL",
    "description": "Romantic Italian bistro in the heart of the city.",
    "thumbnailUrl": "/images/bella-notte.jpg"
  }
]
```

**GET `/api/restaurants/{id}/slots?date=2026-06-20&partySize=4`**
```json
// Response 200
[
  { "slotId": "uuid", "time": "18:00", "remainingCapacity": 6 },
  { "slotId": "uuid", "time": "19:30", "remainingCapacity": 4 },
  { "slotId": "uuid", "time": "21:00", "remainingCapacity": 10 }
]
```

### Reservations

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/reservations` | JWT | Create a reservation |
| GET | `/api/reservations/my` | JWT | Get current user's reservations |
| DELETE | `/api/reservations/{id}` | JWT | Cancel a reservation |

**POST `/api/reservations`**
```json
// Request
{ "slotId": "uuid", "partySize": 4 }

// Response 201
{
  "reservationId": "uuid",
  "restaurantName": "Bella Notte",
  "date": "2026-06-20",
  "time": "19:30",
  "partySize": 4,
  "status": "Confirmed"
}

// Response 409 (slot full or gone)
{ "error": "This time slot is no longer available." }
```

**DELETE `/api/reservations/{id}`**
```json
// Response 200
{ "message": "Reservation cancelled." }

// Response 403 (not owner)
{ "error": "You are not authorized to cancel this reservation." }
```

---

## 11. Success Criteria

### MVP Success Definition

The MVP is successful when a new user can, without any manual intervention, complete the full flow: register → browse restaurants → select date + party size → book a time slot → view the confirmed reservation on their dashboard.

### Functional Requirements

- [x] User can register and sign in with email + password
- [x] Restaurant listing displays all seeded restaurants
- [x] Filtering by date and party size returns only slots with sufficient capacity
- [x] Two simultaneous booking attempts for the same full slot — only one succeeds
- [x] Confirmed reservation appears on the user's dashboard immediately
- [x] Cancelling a reservation restores slot availability
- [x] JWT expires and unauthenticated routes are protected

### Quality Indicators

- No double-booking under concurrent requests (validated via integration test)
- Page load time < 2s on a standard connection for the restaurant listing
- API response time < 300ms for slot availability queries
- All API endpoints covered by at least one integration test

### User Experience Goals

- Reservation completion in ≤ 4 clicks from the restaurant listing
- Clear error states (slot unavailable, auth failure) with actionable messages
- Responsive layout functional at 1024px+ (desktop-first for MVP)

---

## 12. Implementation Phases

### Phase 1 — Core Reservation Flow (MVP)

**Goal:** A working end-to-end reservation experience with seeded data.

**Deliverables:**
- [x] Project scaffolding (Angular 20 + .NET solution)
- [x] Database schema + EF Core migrations
- [x] Seed data: 10–15 restaurants, time slots, test user accounts
- [x] Auth endpoints (register, login) + JWT middleware
- [x] Restaurant listing and detail pages
- [x] Slot availability query (date + party size filtering)
- [x] Reservation creation with concurrency protection
- [x] My Reservations dashboard
- [x] Reservation cancellation
- [x] Angular route guards + JWT interceptor

**Validation:** Full E2E flow works in browser; double-booking test passes.

**Estimated effort:** 3–4 weeks

---

### Phase 2 — Notifications & Calendar

**Goal:** Diners receive an email confirmation with a calendar invite and directions link after booking.

**Deliverables:**
- [x] Email service integration (SMTP or SendGrid/Mailgun)
- [x] Booking confirmation email (HTML template: restaurant name, date, time, party size)
- [x] `.ics` calendar file attached to confirmation email
- [x] Directions link in email body (Google Maps URL constructed from restaurant address)
- [x] Reminder email 24h before reservation time (background job / hosted service)

**Validation:** Diner receives email with .ics attachment within 30 seconds of booking.

**Estimated effort:** 1–2 weeks

---

### Phase 3 — Reviews & Menu Photos

**Goal:** Enrich the platform with user-generated content to aid discovery.

**Deliverables:**
- [x] Review model: rating (1–5 stars), text body, author, timestamp
- [x] Any registered user can submit a review for any restaurant
- [x] Reviews displayed on restaurant detail page (sorted by most recent)
- [x] Restaurant operators can upload entree/menu photos associated with a time period
- [x] Photos displayed in a gallery on the restaurant detail page
- [x] Image upload endpoint with server-side size/type validation

**Validation:** Reviews appear on restaurant page; photos upload and render correctly.

**Estimated effort:** 2–3 weeks

---

### Phase 4 — Favorites & Popularity

**Goal:** Surface trending restaurants to drive discovery and repeat engagement.

**Deliverables:**
- [x] "Favorites" section on the home page (most-booked restaurants, configurable period)
- [x] Ranking by: most booked this week / this month / by locale (city/neighborhood)
- [x] Diner "Save" button to bookmark a restaurant to a personal favorites list
- [x] Personal favorites page in the user dashboard
- [x] Analytics data pipeline or query to compute booking counts

**Validation:** Favorites section updates daily; bookmarked restaurants appear in user's list.

**Estimated effort:** 1–2 weeks

---

## 13. Future Considerations

- **Restaurant self-service portal** — Operators register, manage their profile, set availability windows, and view their reservation calendar.
- **Waitlist management** — Allow diners to join a waitlist for fully booked slots; notify them if a cancellation opens a slot.
- **No-show tracking** — Record and surface no-show rates to restaurant operators.
- **Payments / deposits** — Require a card hold for high-demand restaurants to reduce no-shows.
- **Mobile application** — Progressive Web App (PWA) upgrade path before native iOS/Android.
- **Social login** — Google / Apple sign-in for frictionless onboarding.
- **Advanced search** — Filter by rating, distance, price range, and availability windows.
- **Multi-language support** — i18n for restaurant descriptions and UI.
- **Real-time availability** — WebSocket or SSE push to update slot availability without page refresh.

---

## 14. Risks & Mitigations

| # | Risk | Impact | Mitigation |
|---|---|---|---|
| 1 | **Double-booking under concurrency** — two requests for the last available slot succeed simultaneously | High | Use optimistic concurrency (EF Core row version / timestamp) or a DB-level transaction with `SELECT FOR UPDATE`; write an integration test that fires concurrent requests |
| 2 | **Seeded data insufficient for demo** — limited restaurants or slots make the product feel thin | Medium | Seed at least 15 restaurants across 3–5 cuisine types; generate 30 days of time slots per restaurant at creation time |
| 3 | **JWT secret exposure** — secret committed to source control | High | Use environment variables / secrets manager; add `.env` to `.gitignore`; document required env vars in README |
| 4 | **Email delivery in Phase 2 fails silently** — confirmation email never arrives | Medium | Use a proven transactional email provider (SendGrid / Mailgun); implement retry logic; log delivery failures; display in-app confirmation regardless of email status |
| 5 | **Phase 1 scope creep** — pressure to add restaurant management before the diner flow is solid | Medium | Explicitly defer restaurant self-service to a named later phase; use seeded data in Phase 1 with no operator-facing UI |

---

## 15. Appendix

### Related Documents

- [Product Idea Brief](.docs/prd/pr-ideas.md)
- [PRD Creation Context](.docs/prd/prd.context.md)
- [Backend Architecture](.docs/design/backend/backend-architecture-example.md)
- [Backend Tech Stack](.docs/design/backend/backend-tech-stack.md)
- [Backend Patterns](.docs/design/backend/backend-patterns.md)
- [Angular Standards](.docs/design/frontend/angular-standards.md)

### Key External Dependencies

- [Angular](https://angular.dev) — Frontend framework
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core) — Backend framework
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core) — ORM
- [JWT Bearer Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn) — Auth middleware
- SendGrid or Mailgun — Transactional email (Phase 2)
