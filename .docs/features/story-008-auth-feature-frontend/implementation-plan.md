# Implementation Plan: Auth Feature — Frontend

This plan sequences the four tasks for STORY-008 into two phases. Phase 1 builds the auth service and both page components; Phase 2 wires the routes.

## Phase 1 — Core service + page components

Tasks run in parallel; they touch disjoint files. The `AuthService` contract is documented in both this plan and `task-01` so the page tasks do not need to read task-01's file.

- [ ] **task-01-auth-core-service** — Create `client/src/app/core/services/auth.service.ts`.
  - `@Injectable({ providedIn: 'root' })`, DI via `inject()`.
  - Private `signal<string | null>` holding the raw JWT, seeded from `localStorage.getItem('tablenow.jwt')`.
  - `isAuthenticated = computed(() => !!this.token() && !this.isExpired(this.token()))`.
  - `login(token: string)` — store token in `localStorage` and update the signal.
  - `logout()` — remove token from `localStorage` and set the signal to `null`.
  - `token()` accessor (read-only signal) for the future interceptor (STORY-009).
  - JWT expiry decoded from the `exp` claim (base64-decode the payload segment); treat malformed tokens as expired/unauthenticated.

- [ ] **task-02-register-page** — Create `RegisterComponent` in `client/src/app/features/auth/components/register/`.
  - Reactive form: `name` (required), `email` (required, email), `password` (required, minLength 8).
  - On submit, call `AuthApiService.register(...)` (or an injected client) → on success, `auth.login(token)` then `router.navigate(['/restaurants'])`.
  - Map 409 → "An account with this email already exists."; 400 → field/summary validation messages.
  - `OnPush`; `inject()`; `@if` for error display; Angular Material form fields.

- [ ] **task-03-login-page** — Create `LoginComponent` in `client/src/app/features/auth/components/login/`.
  - Reactive form: `email` (required, email), `password` (required).
  - On submit, call `AuthApiService.login(...)` → on success, `auth.login(token)` then `router.navigate(['/restaurants'])`.
  - Map 401 → "Invalid email or password." (no field-specific leak); disable submit while in flight.
  - `OnPush`; `inject()`; `@if` for error display; Angular Material form fields.

> Note on HTTP calls: components must not call `.subscribe()`. Use a thin `AuthApiService` (in `features/auth/services/`) that returns the typed response, invoked from a submit handler that uses `firstValueFrom` / `toPromise`-free async, or prefer an `httpResource()`/`rxResource()`-backed trigger. Token storage always flows through `AuthService.login()`.

## Phase 2 — Routes

- [ ] **task-04-auth-routes** — Create `client/src/app/features/auth/routes/auth.routes.ts` and `client/src/app/features/auth/index.ts`.
  - Routes: `{ path: 'register', component: RegisterComponent }`, `{ path: 'login', component: LoginComponent }` (lazy `loadComponent` preferred).
  - Barrel `index.ts` re-exports the routes (and public components if needed).
  - Register the routes in the application's root route configuration.

## Verification

- `npm run lint` — zero errors.
- `npm run build` — succeeds.
- `npm run test` — unit tests for `AuthService` (login stores token + flips signal; logout clears; expired token → not authenticated) and for the components' error mapping pass.
- Manual smoke: register and login flows redirect to `/restaurants`; a forced API error renders an inline message.
