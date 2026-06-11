# Task 04: Auth Routes

## Status

pending

## Wave

2

## Description

Creates the route configuration for `/register` and `/login` routes and wires them into the app-level router. Creates the `features/auth/index.ts` barrel.

## Dependencies

**Depends on:** task-01-auth-service.md, task-02-register-component.md, task-03-login-component.md
**Blocks:** STORY-009 (guard redirects to /login — route must exist), STORY-012 (success redirect goes to /restaurants)

**Context from dependencies:** task-02 created `RegisterComponent` at `features/auth/components/register.component.ts`. task-03 created `LoginComponent` at `features/auth/components/login.component.ts`. This task creates the route definitions and adds them to `app.routes.ts`.

## Files to Create

- `client/src/app/features/auth/routes/auth.routes.ts`
- `client/src/app/features/auth/index.ts`

## Files to Modify

- `client/src/app/app.routes.ts` — add auth routes

## Technical Details

### Code Snippets

```typescript
// client/src/app/features/auth/routes/auth.routes.ts
import { Routes } from '@angular/router';

export const authRoutes: Routes = [
  {
    path: 'register',
    loadComponent: () =>
      import('../components/register.component').then(m => m.RegisterComponent),
  },
  {
    path: 'login',
    loadComponent: () =>
      import('../components/login.component').then(m => m.LoginComponent),
  },
];
```

```typescript
// client/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { authRoutes } from './features/auth/routes/auth.routes';

export const routes: Routes = [
  { path: '', redirectTo: 'restaurants', pathMatch: 'full' },
  ...authRoutes,
  // Feature routes added in STORY-012 (restaurants) and STORY-018 (reservations)
];
```

```typescript
// client/src/app/features/auth/index.ts
export { RegisterComponent } from './components/register.component';
export { LoginComponent } from './components/login.component';
export { authRoutes } from './routes/auth.routes';
```

## Acceptance Criteria

- [ ] `/register` route loads `RegisterComponent`
- [ ] `/login` route loads `LoginComponent`
- [ ] Routes use lazy loading (`loadComponent` with dynamic import)
- [ ] Routes are added to `app.routes.ts`
- [ ] `features/auth/index.ts` barrel exports components and routes
