# Requirements: Project Scaffolding — Frontend

## Summary

The TableNow frontend is an Angular 21 single-page application using exclusively standalone components — no NgModules. The project uses Angular Material for UI, NgRx Signal Store for state management, and `httpResource()` for data fetching. All components must use `OnPush` change detection.

The folder structure is feature-based: each feature lives under `src/app/features/{feature-name}/` with sub-folders for components, services, store, models, and routes. Cross-cutting concerns (auth service, interceptors, guards) live in `src/app/core/`. Reusable UI components live in `src/app/shared/`.

After this story, `npm run build` succeeds and the app boots at `http://localhost:4200` showing an empty shell ready for feature development.

## Goals

- `npm run build` exits with code 0
- `bootstrapApplication` used in `main.ts` (no AppModule)
- Feature-based folder structure established with correct barrel exports
- Angular Material custom theme configured
- Environment files define `apiBaseUrl` pointing to the correct backend URL
- NgRx Signal Store wired into the app config

## Non-Goals

- No feature implementations (those are in subsequent stories)
- No backend integration yet
- No auth flows (STORY-008, STORY-009)
- No routing beyond a placeholder home route

## Acceptance Criteria

- [ ] `npm run build` exits with code 0
- [ ] `main.ts` uses `bootstrapApplication` with no NgModule in sight
- [ ] `src/app/core/`, `src/app/shared/`, `src/app/features/` directories exist with `index.ts` barrel exports
- [ ] `environment.ts` defines `apiBaseUrl: 'http://localhost:5000/api'`
- [ ] Angular Material theme is configured (custom palette, not default indigo/pink)
- [ ] `provideStore()` from NgRx Signal Store is included in `app.config.ts`

## Assumptions

- Angular CLI 21 is available globally (`ng` command)
- Node.js 20+ is installed
- The client root is `./client/`
- API backend runs on port 5000 during development

## Technical Constraints

- Angular 21, standalone components only — no NgModules
- `OnPush` change detection on every component, enforced from the start
- `inject()` function for DI — no constructor injection
- Template control flow: `@if` / `@for` — never `*ngIf` / `*ngFor`
- No `.subscribe()` in components
