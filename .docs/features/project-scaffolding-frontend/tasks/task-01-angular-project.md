# Task 01: Create Angular 21 Standalone Project

## Status

pending

## Wave

1

## Description

Scaffolds the Angular 21 frontend application using `ng new` with standalone components, routing, and Angular Material. The app must use `bootstrapApplication` in `main.ts` — no `AppModule`. This establishes the compilable base that Phase 2 tasks build on.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-02-folder-structure.md, task-03-environment-config.md

**Context from dependencies:** None — this is the foundation task.

## Files to Create

- `client/` — entire Angular project directory created by `ng new`
- Key generated files: `client/src/main.ts`, `client/src/app/app.component.ts`, `client/src/app/app.config.ts`, `client/src/app/app.routes.ts`, `client/angular.json`, `client/package.json`

## Files to Modify

- `client/src/app/app.component.ts` — Ensure it's a standalone component with `OnPush` change detection

## Technical Details

### Implementation Steps

1. From the repo root, run:
   ```bash
   ng new client \
     --routing \
     --style scss \
     --standalone \
     --skip-tests \
     --package-manager npm
   ```

2. Add Angular Material:
   ```bash
   cd client
   ng add @angular/material
   ```
   When prompted: choose a custom theme (or "Custom" option), enable typography, enable animations.

3. Add NgRx Signal Store:
   ```bash
   npm install @ngrx/signals
   ```

4. Update `client/src/app/app.component.ts` to use `OnPush` change detection:
   ```typescript
   import { ChangeDetectionStrategy, Component } from '@angular/core';
   import { RouterOutlet } from '@angular/router';

   @Component({
     selector: 'app-root',
     standalone: true,
     imports: [RouterOutlet],
     template: `<router-outlet />`,
     changeDetection: ChangeDetectionStrategy.OnPush,
   })
   export class AppComponent {}
   ```

5. Verify `client/src/main.ts` uses `bootstrapApplication`:
   ```typescript
   import { bootstrapApplication } from '@angular/platform-browser';
   import { appConfig } from './app/app.config';
   import { AppComponent } from './app/app.component';

   bootstrapApplication(AppComponent, appConfig).catch(console.error);
   ```

6. Run `npm run build` to verify a clean build.

### Code Snippets

`client/src/app/app.config.ts` baseline (NgRx and router providers will be expanded in task-03):
```typescript
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
  ],
};
```

## Acceptance Criteria

- [ ] `client/` directory exists with a valid Angular 21 project
- [ ] `npm run build` from `./client` exits with code 0
- [ ] `main.ts` calls `bootstrapApplication` — no `AppModule` anywhere
- [ ] `AppComponent` has `changeDetection: ChangeDetectionStrategy.OnPush`
- [ ] Angular Material and NgRx Signals are in `package.json` dependencies

## Notes

Use `--skip-tests` during scaffolding since test configuration will be set up alongside each feature. Vitest replaces the default Karma/Jest setup — this configuration happens in task-03.
