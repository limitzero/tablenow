# Implementation Plan: Project Scaffolding — Frontend

## Phase 1 — Initialization & Structure (parallel)

These two tasks touch largely disjoint files. task-01 owns package/config files and the bootstrap entry points; task-02 owns the `core/`/`shared/`/`features/` folders, the theme SCSS, and `environment.ts`. They can run in parallel; if a sequencing conflict arises on shared config (e.g., `angular.json` styles array), task-01 establishes the base config and task-02 edits only the theme/styles and environment entries.

- [ ] **task-01-angular-project-init** — Create the Angular 21 workspace under `./client` using `bootstrapApplication` (no `AppModule`). Add Angular Material (`ng add @angular/material`), `@ngrx/signals`, Vitest (replace default test runner), and Playwright. Define `npm` scripts `test`, `e2e`, `lint`, `build`. Verify `npm run build` succeeds.
- [ ] **task-02-folder-structure-theme** — Create `src/app/core/`, `src/app/shared/`, `src/app/features/` (each with a barrel `index.ts`). Author the custom Angular Material theme SCSS and reference it from global styles. Create `src/environments/environment.ts` (and `environment.development.ts`) with `apiBaseUrl: 'http://localhost:5000/api'`. Wire `provideHttpClient`, `provideRouter`, and the Material theme into `app.config.ts`.
