# Task 02: Folder Structure, Material Theme & Environment

## Status

pending

## Wave

1

## Description

Establish the feature-based folder structure (`core/`, `shared/`, `features/`) under `src/app/`, author the custom Angular Material theme, and create the environment configuration that points the frontend at the backend API (`http://localhost:5000/api`). This task defines the project's organizational conventions and visual baseline that every subsequent frontend feature relies on.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** None (parallel with task-01)

**Context from dependencies:** task-01 creates the Angular 21 workspace under `./client` (package.json, angular.json, `main.ts` with `bootstrapApplication`, `app.config.ts`, `app.component.ts`, `app.routes.ts`) and installs Angular Material. This task adds the folder skeleton, the custom theme SCSS, and the `environment.ts` files into that workspace. If task-01 has not finished when this runs, create the listed files at their paths; they are additive and do not conflict with task-01's package/config files except for the theme reference in global styles and the optional environment `fileReplacements` in `angular.json`.

## Files to Create

- `client/src/app/core/index.ts` — barrel export for core (auth service, interceptors, guards added by later stories).
- `client/src/app/core/.gitkeep`
- `client/src/app/shared/index.ts` — barrel export for shared (reusable components, pipes, directives).
- `client/src/app/shared/.gitkeep`
- `client/src/app/features/index.ts` — barrel export for features.
- `client/src/app/features/.gitkeep`
- `client/src/styles/_theme.scss` — custom Angular Material theme (palettes, typography, density).
- `client/src/environments/environment.ts` — production-default environment with `apiBaseUrl`.
- `client/src/environments/environment.development.ts` — development environment with `apiBaseUrl`.

## Files to Modify

- `client/src/styles.scss` — import `./styles/_theme.scss` and apply the Material theme; add a small set of global resets.
- `client/angular.json` — register `src/styles.scss` in the build `styles` array (if not already) and add `fileReplacements` mapping `environment.ts` → `environment.development.ts` for the development configuration.

## Technical Details

### Folder structure

Per CLAUDE.md, the app uses a feature-based layout. This task creates the top-level skeleton; feature subfolders (`auth/`, `restaurants/`, `reservations/`) are added by their respective stories, each with:

```
features/<feature-name>/
  components/  services/  store/  models/  routes/  index.ts
```

Every feature folder requires a barrel `index.ts`. Create the three top-level barrels now.

### Environment config

`client/src/environments/environment.ts`:

```ts
export const environment = {
  production: true,
  apiBaseUrl: 'http://localhost:5000/api',
} as const;
```

`client/src/environments/environment.development.ts`:

```ts
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000/api',
} as const;
```

> Both point at `http://localhost:5000/api` per the acceptance criteria. The production value will be overridden at deploy time later; for now they match so local builds work in either configuration.

In `angular.json`, add to the `development` configuration:

```json
"fileReplacements": [
  {
    "replace": "src/environments/environment.ts",
    "with": "src/environments/environment.development.ts"
  }
]
```

### Custom Material theme

`client/src/styles/_theme.scss` (Angular Material 3 / M3 token API):

```scss
@use '@angular/material' as mat;

html {
  color-scheme: light;

  @include mat.theme((
    color: (
      primary: mat.$azure-palette,
      tertiary: mat.$blue-palette,
    ),
    typography: Roboto,
    density: 0,
  ));
}
```

`client/src/styles.scss`:

```scss
@use './styles/theme';

html, body { height: 100%; }
body { margin: 0; font-family: Roboto, 'Helvetica Neue', sans-serif; }
```

### Implementation Steps

1. Create the `core/`, `shared/`, `features/` folders, each with a barrel `index.ts` (initially exporting nothing, with a `// barrel — populated by feature stories` comment) and a `.gitkeep`.
2. Create `src/styles/_theme.scss` with the custom Material theme.
3. Edit `src/styles.scss` to `@use` the theme and add global resets.
4. Create `src/environments/environment.ts` and `environment.development.ts` with `apiBaseUrl`.
5. Edit `angular.json` to register `styles.scss` (if needed) and add the development `fileReplacements`.
6. Run `npm run build` and confirm zero errors.

## Acceptance Criteria

- [ ] `src/app/` contains `core/`, `shared/`, and `features/` folders, each with a barrel `index.ts`.
- [ ] `src/environments/environment.ts` defines `apiBaseUrl` = `http://localhost:5000/api`.
- [ ] `src/environments/environment.development.ts` exists and defines `apiBaseUrl`.
- [ ] A custom Angular Material theme is defined in `src/styles/_theme.scss` and applied via `styles.scss`.
- [ ] `angular.json` maps `environment.ts` → `environment.development.ts` in the development configuration.
- [ ] `npm run build` succeeds with zero errors.

## Notes

- Do not create feature components, stores, or services here — only the top-level folder skeleton and barrels.
- The exact Material palette can be adjusted later; the requirement is a *custom* theme (not a prebuilt CSS theme).
- Coordinate with task-01 on `angular.json`: task-01 sets up the base build/test config; this task only touches the `styles` array and the development `fileReplacements`.
