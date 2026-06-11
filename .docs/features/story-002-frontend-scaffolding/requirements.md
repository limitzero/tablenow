# Requirements: Project Scaffolding — Frontend

## Summary

TableNow's frontend is an Angular 21 application using standalone components exclusively — no NgModules anywhere. The project must use `bootstrapApplication` from `@angular/platform-browser`, Angular Material for UI components with a custom color theme, and NgRx Signal Store for state management.

The feature-based folder structure (`core/`, `shared/`, `features/`) must exist with `index.ts` barrel exports in every feature folder. This structure is the contract that all subsequent frontend stories follow. All components must use `OnPush` change detection and `inject()` for dependency injection.

## Goals

- Angular 21 project at `./client` using `bootstrapApplication` (no NgModule, no AppModule)
- Angular Material with a custom theme configured in `styles.scss`
- NgRx Signal Store installed and available
- `core/`, `shared/`, `features/` directory structure under `src/app/`
- `environment.ts` pointing to `http://localhost:5000/api` as `apiBaseUrl`
- `npm run build` succeeds with zero errors

## Non-Goals

- No actual feature components (STORY-008+)
- No HTTP client setup (added per-feature)
- No routing for specific features (added in STORY-008, STORY-012, etc.)

## Acceptance Criteria

- [ ] `npm run build` from `./client` succeeds with zero errors
- [ ] `bootstrapApplication` is used in `main.ts` (no AppModule)
- [ ] `src/app/core/`, `src/app/shared/`, `src/app/features/` directories exist with `index.ts` barrels
- [ ] `environment.ts` exports `apiBaseUrl: 'http://localhost:5000/api'`
- [ ] Angular Material is imported and a custom theme is active in `styles.scss`
- [ ] TypeScript strict mode is enabled

## Assumptions

- Node.js ≥ 20 and Angular CLI ≥ 21 are installed
- The `client/` directory does not yet exist in the repository

## Technical Constraints

- Standalone components only — `NgModule` is forbidden
- `OnPush` change detection on every component, no exceptions
- `inject()` for all DI — no constructor injection
- `@if` / `@for` in templates — no `*ngIf` / `*ngFor`
- No `.subscribe()` in components — use `httpResource()` or async pipe
- NgRx Signal Store for state — one store slice per feature
