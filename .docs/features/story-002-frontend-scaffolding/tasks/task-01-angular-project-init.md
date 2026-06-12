# Task 01: Angular Project Initialization

## Status

pending

## Wave

1

## Description

Initialize the Angular 21 workspace for TableNow under `./client` as a standalone-component application (bootstrapped with `bootstrapApplication`, no `AppModule`) and install the project's required tooling: Angular Material, NgRx Signal Store, Vitest as the unit-test runner, and Playwright for end-to-end tests. Wire the `npm` scripts (`test`, `e2e`, `lint`, `build`). This task produces the package/config foundation every other frontend story depends on; `npm run build` must succeed with zero errors.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** None (parallel with task-02; both complete the scaffold)

**Context from dependencies:** None. Greenfield. This task creates `package.json`, `angular.json`, `tsconfig*.json`, the bootstrap entry (`main.ts`, `app.config.ts`, `app.component.ts`), and test runner config. task-02 adds the folder structure, theme, and environment files into the same workspace.

## Files to Create

- `client/package.json` — dependencies, devDependencies, and the `test`/`e2e`/`lint`/`build` scripts.
- `client/angular.json` — Angular CLI workspace config (application builder, styles, assets).
- `client/tsconfig.json`, `client/tsconfig.app.json`, `client/tsconfig.spec.json` — TypeScript configs.
- `client/src/main.ts` — `bootstrapApplication(AppComponent, appConfig)`.
- `client/src/app/app.component.ts` — root standalone component (`OnPush`, `<router-outlet />`).
- `client/src/app/app.config.ts` — application providers (router, http client; theme/material wiring may be extended by task-02).
- `client/src/app/app.routes.ts` — empty route table (to be populated by feature stories).
- `client/vitest.config.ts` — Vitest configuration.
- `client/playwright.config.ts` — Playwright configuration.
- `client/e2e/example.spec.ts` — a smoke e2e test that loads the app shell.
- `client/.eslintrc` / `client/eslint.config.js` — ESLint config (Angular ESLint).

## Files to Modify

- None (greenfield).

## Technical Details

### Implementation Steps

1. Scaffold the workspace: `npx @angular/cli@21 new tablenow-client --directory client --standalone --routing --style=scss --ssr=false --skip-tests=false`. Confirm there is no `AppModule` and `main.ts` uses `bootstrapApplication`.
2. Add Angular Material: `cd client && ng add @angular/material` (choose a prebuilt-or-custom theme; the custom theme is finalized in task-02). Accept Angular animations and typography.
3. Add NgRx Signal Store: `npm install @ngrx/signals`.
4. Replace the default unit-test runner with Vitest:
   - `npm install -D vitest @analogjs/vitest-angular jsdom @angular/build`
   - Create `vitest.config.ts` (Angular + jsdom environment).
   - Set the `test` script to `vitest`.
5. Add Playwright: `npm install -D @playwright/test && npx playwright install`. Create `playwright.config.ts` and one smoke spec under `e2e/`.
6. Add Angular ESLint: `ng add @angular-eslint/schematics`. Set the `lint` script to `ng lint` (or `eslint .`).
7. Ensure `npm` scripts exist: `"build": "ng build"`, `"test": "vitest"`, `"e2e": "playwright test"`, `"lint": "ng lint"`.
8. Run `npm run build` and confirm zero errors.

### Code Snippets

`main.ts`:

```ts
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';

bootstrapApplication(AppComponent, appConfig).catch((err) => console.error(err));
```

`app.component.ts`:

```ts
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet],
  template: `<router-outlet />`,
})
export class AppComponent {}
```

`app.config.ts` (base — task-02 may extend with theme-related providers):

```ts
import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withFetch()),
    provideAnimationsAsync(),
  ],
};
```

`vitest.config.ts` (outline):

```ts
import { defineConfig } from 'vitest/config';
import angular from '@analogjs/vitest-angular/plugin';

export default defineConfig({
  plugins: [angular()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['src/test-setup.ts'],
  },
});
```

## Acceptance Criteria

- [ ] `./client` contains an Angular 21 workspace that uses `bootstrapApplication` in `main.ts` with no `AppModule` anywhere.
- [ ] `AppComponent` is a standalone component using `ChangeDetectionStrategy.OnPush`.
- [ ] Angular Material is installed (a custom theme is finalized in task-02).
- [ ] `@ngrx/signals` is a dependency.
- [ ] Vitest is the unit-test runner (`npm run test` invokes Vitest).
- [ ] Playwright is installed with a `playwright.config.ts` and at least one smoke e2e spec (`npm run e2e`).
- [ ] `package.json` defines `build`, `test`, `e2e`, and `lint` scripts.
- [ ] `npm run build` from `./client` succeeds with zero errors.

## Notes

- Use zoneless change detection (`provideZonelessChangeDetection`) if the Angular 21 default supports it; otherwise fall back to the default. Keep `OnPush` mandatory on all components regardless.
- Do not create feature folders, the theme SCSS, or `environment.ts` here — those are task-02. Avoid editing the `styles`/theme entries in `angular.json` beyond the Material defaults so task-02's theme work does not conflict.
- Do not add an HTTP interceptor or route guard — that is STORY-009.
