# Requirements: Project Scaffolding — Frontend

## Summary

TableNow's frontend is an Angular 21 single-page application built entirely with standalone components (no `NgModule`). Before feature work begins, the project needs a consistent foundation: a freshly initialized Angular workspace bootstrapped via `bootstrapApplication`, the required tooling (Angular Material with a custom theme, NgRx Signal Store for state, Vitest for unit tests, Playwright for e2e), and the feature-based folder structure (`core/`, `shared/`, `features/`) described in CLAUDE.md.

This story produces that scaffold under `./client`. It establishes the environment configuration (`environment.ts` with `apiBaseUrl` pointing at the backend), the custom Material theme, and the conventions every later frontend story relies on: `OnPush` change detection, `inject()` DI, `@if`/`@for` control flow, services-only API access, and `httpResource()`/async-pipe instead of manual `.subscribe()`.

The expected outcome is that `npm run build` from `./client` succeeds with zero errors and that subsequent frontend stories can add feature folders without restructuring the workspace.

## Goals

- An Angular 21 workspace under `./client` bootstrapped with `bootstrapApplication` (no `AppModule`).
- Dependencies installed: Angular Material, `@ngrx/signals`, Vitest, Playwright.
- `npm` scripts: `test` (Vitest), `e2e` (Playwright), `lint` (ESLint), `build` (production build).
- Folder structure under `src/app/`: `core/`, `shared/`, `features/`.
- A custom Angular Material theme.
- `environment.ts` defining `apiBaseUrl` = `http://localhost:5000/api`.
- `npm run build` from `./client` succeeds with zero errors.

## Non-Goals

- No auth, restaurant, or reservation feature components, routes, or stores (covered by STORY-008+).
- No HTTP interceptor or route guard (covered by STORY-009).
- No backend work (covered by STORY-001).
- No production deployment configuration beyond the standard production build.

## Acceptance Criteria

- [ ] `npm run build` from `./client` succeeds with zero errors.
- [ ] The project uses `bootstrapApplication` (there is no `AppModule`).
- [ ] `src/app/` contains `core/`, `shared/`, and `features/` folders.
- [ ] `environment.ts` defines `apiBaseUrl` = `http://localhost:5000/api`.
- [ ] Angular Material is installed with a custom theme applied.
- [ ] NgRx Signal Store (`@ngrx/signals`), Vitest, and Playwright are installed and wired to npm scripts.

## Assumptions

- Node.js (LTS compatible with Angular 21) and npm are installed.
- The backend API will be reachable at `http://localhost:5000/api` during local development (matching STORY-001's host).
- The Angular CLI used to scaffold supports standalone-only generation (Angular 21 default).
- Vitest is used in place of the default Karma/Jasmine runner; the Angular build target remains the application builder.

## Technical Constraints

- Standalone components only — no `NgModule`. Bootstrap via `bootstrapApplication(AppComponent, appConfig)`.
- Every component uses `ChangeDetectionStrategy.OnPush` — no exceptions.
- State management: NgRx Signal Store, one store slice per feature.
- DI via the `inject()` function — no constructor injection.
- API access through services only, never directly in components.
- Templates use `@if` / `@for` — never `*ngIf` / `*ngFor`.
- Async data via `httpResource()` or the async pipe — no `.subscribe()` in components.
- UI built with Angular Material using the custom theme.
- Feature folders require a barrel `index.ts` export (per CLAUDE.md).
