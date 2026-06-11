# Task 04: Auth Routes & Barrel Exports

## Status

pending

## Wave

3

## Description

Creates the auth feature route configuration and barrel exports, completing the auth feature folder structure. Defines `AUTH_ROUTES` with `/login` and `/register` paths pointing to the components created in tasks 02 and 03.

## Dependencies

**Depends on:** task-02-register-component.md, task-03-login-component.md
**Blocks:** STORY-009 (route guard uses these routes for redirects)

**Context from dependencies:**
- task-02 created `RegisterComponent` at `features/auth/components/register/register.component.ts`
- task-03 created `LoginComponent` at `features/auth/components/login/login.component.ts`
- STORY-002 task-02 created `client/src/app/features/auth/routes.ts` with an empty `AUTH_ROUTES = []` placeholder

## Files to Create

- `client/src/app/features/auth/index.ts` — Barrel export for auth feature

## Files to Modify

- `client/src/app/features/auth/routes.ts` — Replace empty array with real route config

## Technical Details

### Code Snippets

```typescript
// client/src/app/features/auth/routes.ts
import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./components/login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./components/register/register.component').then(m => m.RegisterComponent),
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
];
```

```typescript
// client/src/app/features/auth/index.ts
export { AUTH_ROUTES } from './routes';
```

## Acceptance Criteria

- [ ] `AUTH_ROUTES` contains routes for `login` and `register` paths with lazy-loaded components
- [ ] `/auth/login` loads `LoginComponent`
- [ ] `/auth/register` loads `RegisterComponent`
- [ ] `npm run build` exits with code 0
