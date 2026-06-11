# Task 03: Login Component

## Status

pending

## Wave

1

## Description

Creates the login form component at `features/auth/components/login.component.ts`. Reactive form with email and password. POSTs to `/api/auth/login`. On 200: stores JWT via `AuthService.storeToken()` and navigates to `/restaurants`. On 401: shows inline error. OnPush.

## Dependencies

**Depends on:** None (Wave 1 — parallel with task-01 and task-02, different files)
**Blocks:** task-04-auth-routes.md

**Context from dependencies:** STORY-006 created `POST /api/auth/login` returning `{token, expiresAt}`. `AuthService` from task-01 (same wave, already defined) provides `storeToken()`. Since both are Wave 1, this file imports `AuthService` by its path — Angular's DI will resolve it correctly at runtime.

## Files to Create

- `client/src/app/features/auth/components/login.component.ts`

## Technical Details

### Code Snippets

```typescript
// client/src/app/features/auth/components/login.component.ts
import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { environment } from '../../../../environments/environment';

interface LoginResponse { token: string; expiresAt: string; }

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="auth-container">
      <h1>Sign In</h1>
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <mat-form-field>
          <mat-label>Email</mat-label>
          <input matInput type="email" formControlName="email" />
        </mat-form-field>
        <mat-form-field>
          <mat-label>Password</mat-label>
          <input matInput type="password" formControlName="password" />
        </mat-form-field>
        @if (errorMessage()) {
          <p class="error">{{ errorMessage() }}</p>
        }
        <button mat-raised-button color="primary" type="submit" [disabled]="loading()">
          {{ loading() ? 'Signing in...' : 'Sign In' }}
        </button>
      </form>
      <p>No account? <a routerLink="/register">Create one</a></p>
    </div>
  `,
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  readonly errorMessage = signal<string | null>(null);
  readonly loading = signal(false);

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.errorMessage.set(null);

    this.http.post<LoginResponse>(`${environment.apiBaseUrl}/auth/login`, this.form.value).subscribe({
      next: (res) => {
        this.authService.storeToken(res.token);
        this.router.navigate(['/restaurants']);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(
          err.status === 401 ? 'Invalid email or password.' : 'Login failed. Please try again.'
        );
      },
      complete: () => this.loading.set(false),
    });
  }
}
```

## Acceptance Criteria

- [ ] Component exists at `features/auth/components/login.component.ts`
- [ ] Reactive form with email (required, email format) and password (required)
- [ ] On 200: calls `authService.storeToken(token)` then navigates to `/restaurants`
- [ ] On 401: shows inline error "Invalid email or password."
- [ ] Loading state disables button during API call
- [ ] `OnPush` change detection, standalone, `inject()` DI
