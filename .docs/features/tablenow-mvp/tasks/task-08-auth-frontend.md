# Task 08: Auth Feature — Frontend

## Status

pending

## Phase

5

## Description

Build the Register and Login pages for the Angular frontend, plus the `AuthService` in `core/` that manages JWT storage and the `isAuthenticated` signal. On successful registration the user is redirected to `/restaurants`. On successful login the JWT is stored in `localStorage` and the user is redirected to `/restaurants`. Auth API errors display inline below the relevant form field or at the top of the form.

## Dependencies

**Depends on:** task-02-frontend-scaffolding, task-06-user-signin-jwt-backend  
**Blocks:** task-10-jwt-interceptor-guard-frontend

**Context from dependencies:** task-02 created the Angular 21 project with `app.config.ts`, `app.routes.ts`, and the `core/`, `shared/`, `features/` folder structure. task-06 implemented `POST /api/v1/auth/register` (returns `{userId, name, email}` on 201) and `POST /api/v1/auth/login` (returns `{token, expiresAt}` on 200, 401 on bad credentials). The API base URL is `http://localhost:5000/api` from `environment.ts`.

## Files to Create

- `client/src/app/core/services/auth.service.ts` — JWT storage, `isAuthenticated` signal, login/register API calls
- `client/src/app/features/auth/components/login/login.component.ts`
- `client/src/app/features/auth/components/login/login.component.html`
- `client/src/app/features/auth/components/register/register.component.ts`
- `client/src/app/features/auth/components/register/register.component.html`
- `client/src/app/features/auth/routes/auth.routes.ts` — lazy-loaded routes for `/login` and `/register`
- `client/src/app/features/auth/index.ts` — barrel export

## Files to Modify

- `client/src/app/app.routes.ts` — add lazy-loaded auth routes

## Technical Details

### Implementation Steps

1. **Create the auth feature folder structure:**
   ```bash
   mkdir -p src/app/features/auth/components/login
   mkdir -p src/app/features/auth/components/register
   mkdir -p src/app/features/auth/routes
   ```

2. **Write `AuthService`** — see snippet below.

3. **Write `LoginComponent`** using reactive forms, `OnPush`, `inject()`.

4. **Write `RegisterComponent`** using reactive forms, `OnPush`, `inject()`.

5. **Write `auth.routes.ts`** with routes for `/login` and `/register`.

6. **Add auth routes to `app.routes.ts`** using lazy loading.

### Code Snippets

**`auth.service.ts`:**
```typescript
import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

interface LoginResponse { token: string; expiresAt: string; }
interface RegisterResponse { userId: string; name: string; email: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly TOKEN_KEY = 'tablenow_token';

  private readonly _token = signal<string | null>(
    localStorage.getItem(this.TOKEN_KEY)
  );

  readonly isAuthenticated = computed(() => this._token() !== null);
  readonly token = this._token.asReadonly();

  login(email: string, password: string) {
    return this.http.post<LoginResponse>(
      `${environment.apiBaseUrl}/v1/auth/login`,
      { email, password }
    );
  }

  register(name: string, email: string, password: string) {
    return this.http.post<RegisterResponse>(
      `${environment.apiBaseUrl}/v1/auth/register`,
      { name, email, password }
    );
  }

  storeToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
    this._token.set(token);
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    this._token.set(null);
    this.router.navigate(['/login']);
  }
}
```

**`login.component.ts`:**
```typescript
import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, MatInputModule, MatButtonModule, MatCardModule, RouterLink],
  templateUrl: './login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  readonly errorMessage = signal<string | null>(null);
  readonly isLoading = signal(false);

  submit(): void {
    if (this.form.invalid) return;
    const { email, password } = this.form.getRawValue();
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.auth.login(email!, password!).subscribe({
      next: (res) => {
        this.auth.storeToken(res.token);
        this.router.navigate(['/restaurants']);
      },
      error: () => {
        this.errorMessage.set('Invalid email or password.');
        this.isLoading.set(false);
      },
    });
  }
}
```

**`login.component.html`:**
```html
<mat-card class="auth-card">
  <mat-card-header>
    <mat-card-title>Sign In</mat-card-title>
  </mat-card-header>
  <mat-card-content>
    <form [formGroup]="form" (ngSubmit)="submit()">
      @if (errorMessage()) {
        <p class="error-banner">{{ errorMessage() }}</p>
      }
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Email</mat-label>
        <input matInput type="email" formControlName="email" />
      </mat-form-field>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Password</mat-label>
        <input matInput type="password" formControlName="password" />
      </mat-form-field>
      <button mat-flat-button color="primary" type="submit" [disabled]="isLoading()">
        @if (isLoading()) { Signing in… } @else { Sign In }
      </button>
    </form>
    <p>No account? <a routerLink="/register">Register</a></p>
  </mat-card-content>
</mat-card>
```

**`register.component.ts` (same pattern as login but with `name` field and calls `auth.register()`):**
```typescript
// Mirror LoginComponent structure; form has 'name', 'email', 'password' controls.
// On success: do NOT storeToken — redirect to /login with a "Account created" message.
```

**`auth.routes.ts`:**
```typescript
import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('../components/login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('../components/register/register.component').then(m => m.RegisterComponent),
  },
];
```

**`app.routes.ts` update:**
```typescript
export const routes: Routes = [
  { path: '', redirectTo: '/restaurants', pathMatch: 'full' },
  {
    path: '',
    loadChildren: () =>
      import('./features/auth/routes/auth.routes').then(m => m.AUTH_ROUTES),
  },
  // Restaurants and reservations routes added in task-13 and task-15
];
```

**`index.ts` (barrel):**
```typescript
export { AuthService } from './core/services/auth.service'; // re-exported from core, not feature
```

### API Contracts Used

| Endpoint | Request | Success Response | Error |
|----------|---------|-----------------|-------|
| `POST /api/v1/auth/register` | `{name, email, password}` | 201 `{userId, name, email}` | 409 `{errors:[...]}`, 400 |
| `POST /api/v1/auth/login` | `{email, password}` | 200 `{token, expiresAt}` | 401 |

## Acceptance Criteria

- [ ] `/register` renders a form with `name`, `email`, `password` fields; submitting redirects to `/login`
- [ ] `/login` renders a form with `email`, `password` fields; submitting on success stores token and redirects to `/restaurants`
- [ ] Duplicate email on register shows an error message (extracted from the 409 response body)
- [ ] Invalid credentials on login shows "Invalid email or password."
- [ ] All components use `ChangeDetectionStrategy.OnPush`
- [ ] DI via `inject()` — no constructor injection
- [ ] `@if` used for conditional rendering — no `*ngIf`

## Notes

- Registration does not issue a JWT — the user must log in after registering. This matches the PRD flow.
- `AuthService` is `providedIn: 'root'` — it is a singleton shared across the app.
- The `isAuthenticated` computed signal is the single source of truth for whether a user is logged in. It is read by the route guard in task-10.
- The `.subscribe()` call in the component is acceptable here because it is a one-shot HTTP call (not a long-lived observable) that also calls `router.navigate()`. For ongoing reactive state, use `httpResource()` instead.
