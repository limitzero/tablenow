# Task 01: Angular Project Init

## Status

pending

## Wave

1

## Description

Initializes the Angular 21 project at `./client` using the standalone component architecture (`bootstrapApplication`). Installs Angular Material, NgRx Signal Store, and configures TypeScript strict mode. The output is a compilable Angular skeleton without any feature content.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-02-folder-structure-theme.md

**Context from dependencies:** None — this is the first frontend task.

## Files to Create

- `client/package.json` — project manifest with all dependencies
- `client/angular.json` — Angular workspace config
- `client/tsconfig.json` — TypeScript config with strict mode
- `client/src/main.ts` — `bootstrapApplication` entry point
- `client/src/app/app.component.ts` — root standalone component
- `client/src/app/app.config.ts` — `ApplicationConfig` with router and HTTP providers
- `client/src/app/app.routes.ts` — root routes array (empty initially)

## Technical Details

### Implementation Steps

1. Run: `ng new tablenow --standalone --routing --style=scss --directory client --skip-git`

2. Install additional packages:
   ```bash
   cd client
   npm install @angular/material @angular/cdk @ngrx/signals
   ```

3. Ensure `src/main.ts` uses `bootstrapApplication`:
   ```typescript
   import { bootstrapApplication } from '@angular/platform-browser';
   import { appConfig } from './app/app.config';
   import { AppComponent } from './app/app.component';
   bootstrapApplication(AppComponent, appConfig).catch(console.error);
   ```

4. Set `tsconfig.json` to strict mode: `"strict": true`.

5. Confirm `app.config.ts` uses the functional providers pattern:
   ```typescript
   import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
   import { provideRouter } from '@angular/router';
   import { provideHttpClient, withInterceptors } from '@angular/common/http';
   import { routes } from './app.routes';

   export const appConfig: ApplicationConfig = {
     providers: [
       provideZoneChangeDetection({ eventCoalescing: true }),
       provideRouter(routes),
       provideHttpClient(),
     ]
   };
   ```

6. Verify `npm run build` passes.

### Code Snippets

```typescript
// src/app/app.component.ts
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet />',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {}
```

## Acceptance Criteria

- [ ] `npm run build` from `./client` succeeds with zero errors
- [ ] `main.ts` uses `bootstrapApplication` (no `platformBrowserDynamic` or `AppModule`)
- [ ] `app.config.ts` uses `ApplicationConfig` with functional providers
- [ ] TypeScript strict mode is enabled in `tsconfig.json`
- [ ] `@angular/material`, `@ngrx/signals` appear in `package.json` dependencies
