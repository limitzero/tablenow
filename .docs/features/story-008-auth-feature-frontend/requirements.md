# Requirements: Auth Feature — Frontend

## Summary

Visitors need to register and sign in before making reservations. This story adds `/register` and `/login` routes with reactive forms, backend integration, and inline error display. A shared `AuthService` in `core/` persists the JWT to `localStorage` and exposes an `isAuthenticated` computed Signal consumed by route guards and components throughout the app.

## Goals

- `/register` form with name, email, password fields; 201 → redirect to /restaurants; 409 → inline error
- `/login` form with email, password fields; 200 → store JWT + redirect; 401 → inline error
- `AuthService` in `core/services/` with `storeToken`, `clearToken`, `getToken`, `isAuthenticated` signal
- All components use OnPush, inject(), reactive forms, @if/@for templates

## Non-Goals

- Password confirmation field
- "Forgot password" flow
- Social auth (OAuth)
- JWT interceptor and route guard (STORY-009)

## Acceptance Criteria

- [ ] `/register` shows name, email, password form; valid submission redirects to /restaurants
- [ ] Duplicate email registration shows inline error from 409 response
- [ ] `/login` shows email, password form; valid credentials store token and redirect to /restaurants
- [ ] Invalid login credentials show inline error from 401 response
- [ ] `AuthService.isAuthenticated` is a computed Signal (true when token present in localStorage)
- [ ] No `.subscribe()` calls in any component

## Technical Constraints

- Feature folder: `client/src/app/features/auth/`
- `AuthService` in `client/src/app/core/services/` — not in the auth feature folder
- `inject()` for all DI — no constructor injection
- `OnPush` change detection on all components
- Reactive forms only — no template-driven forms
