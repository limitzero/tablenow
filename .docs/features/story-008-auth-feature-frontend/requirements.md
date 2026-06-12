# Requirements: Auth Feature — Frontend

## Summary

TableNow diners need to create an account and sign in directly from the browser before they can make or manage reservations. This feature provides the two entry-point pages — Register and Login — along with the core `AuthService` that all other frontend features rely on for authentication state.

The solution introduces an `AuthService` in `core/` that persists the JWT to `localStorage` and exposes authentication state as a computed Angular Signal (`isAuthenticated`). Two standalone components — `RegisterComponent` and `LoginComponent` — render reactive forms, call the backend auth endpoints through the service, store the returned token, and redirect the user to the restaurant listing on success. API errors are mapped to inline, accessible form messages.

The expected outcome is that an unauthenticated visitor can register or sign in, leave with a stored JWT, and land on `/restaurants` ready to browse and book. The `isAuthenticated` signal and `login`/`logout` methods established here become the foundation for the interceptor and route guard delivered in STORY-009.

## Goals

- Provide a single `AuthService` in `core/` that owns JWT storage (`localStorage`) and exposes `isAuthenticated` as a computed Signal plus `login(token)` and `logout()` methods.
- Provide a `RegisterComponent` with a reactive form (name, email, password) that registers a user, stores the JWT, and redirects to `/restaurants`.
- Provide a `LoginComponent` with a reactive form (email, password) that signs a user in, stores the JWT, and redirects to `/restaurants`.
- Surface backend auth errors (409 duplicate email, 401 invalid credentials, 400 validation) as inline form messages.
- Register `/register` and `/login` routes and expose the feature through a barrel `index.ts`.

## Non-Goals

- The HTTP interceptor that attaches the `Authorization` header (delivered in STORY-009).
- The route guard that redirects unauthenticated users (delivered in STORY-009).
- Backend registration and login endpoints (delivered in STORY-005 and STORY-006).
- "Forgot password", email verification, social/OAuth login, or refresh-token rotation.
- Persisting anything other than the JWT (no user profile caching).

## Acceptance Criteria

- [ ] Visiting `/register` shows a form with name, email, and password fields.
- [ ] Submitting valid registration data redirects the user to the restaurant listing (`/restaurants`).
- [ ] Visiting `/login` shows a form with email and password fields.
- [ ] Submitting valid login credentials stores the JWT in `localStorage` and redirects to `/restaurants`.
- [ ] An auth error (duplicate email, wrong password) returned by the API renders an inline error message.
- [ ] `AuthService.isAuthenticated` is a computed Signal that reflects the presence of a valid stored token.

## Assumptions

- STORY-002 frontend scaffolding exists: Angular 21 standalone-component project, `bootstrapApplication`, `environment.ts` exposing `apiBaseUrl` = `http://localhost:5000/api`, Angular Material configured, NgRx Signal Store available.
- The backend exposes `POST /api/auth/register` (returns `userId`, `name`, `email` with 201) and `POST /api/auth/login` (returns `{ token, expiresAt }` with 200; 401 on bad credentials) per STORY-005/006.
- The application root provides `HttpClient` (via `provideHttpClient`) and the Angular `Router`.
- `localStorage` is available (standard browser context; SSR is not in scope for the MVP).

## Technical Constraints

- Standalone components only — no `NgModule`.
- `OnPush` change detection on every component, no exceptions.
- Dependency injection via the `inject()` function only — no constructor injection.
- Reactive forms (`ReactiveFormsModule`) for validation; template control flow uses `@if` / `@for`, never `*ngIf` / `*ngFor`.
- No `.subscribe()` in components — use `httpResource()` or the async pipe; HTTP calls go through services only.
- Angular Material with the custom theme for form UI.
- The token storage key and JWT decoding logic live solely in `AuthService`; components never read `localStorage` directly.
