# Requirements: Auth Feature — Frontend

## Summary

Provides Register and Login pages so visitors can create accounts and sign in from the browser. The auth service persists the JWT in localStorage and exposes an `isAuthenticated` computed Signal consumed by guards and components throughout the app.

## Goals

- `/register` route shows a form with name, email, password fields
- `/login` route shows a form with email and password fields
- JWT stored in localStorage on successful login
- User redirected to restaurant listing on successful auth
- API errors shown inline (duplicate email, wrong password)

## Non-Goals

- No OAuth / social login
- No password reset
- No email verification
- No "remember me" / persistent sessions beyond localStorage

## Acceptance Criteria

- [ ] `/register` form submits to `POST /api/auth/register` and redirects on 201
- [ ] `/login` form submits to `POST /api/auth/login` and stores token on 200
- [ ] Auth errors display as inline messages
- [ ] `isAuthenticated` signal returns true when a token is in localStorage

## Technical Constraints

- Feature folder: `client/src/app/features/auth/`
- Auth service in `client/src/app/core/auth.service.ts`
- `inject()` for DI — no constructor injection
- Reactive forms for validation
- `@if` / `@for` in templates — no `*ngIf` / `*ngFor`
- No `.subscribe()` in components
