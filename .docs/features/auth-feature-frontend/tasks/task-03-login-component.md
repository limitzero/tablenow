# Task 03: Login Component

## Status

pending

## Wave

2

## Description

Creates the Login form component at `/auth/login`. On success, calls `AuthService.setToken()` with the JWT and navigates to `/restaurants`. Shows generic "Invalid email or password." on 401 — no field-level detail.

## Dependencies

**Depends on:** task-01-auth-service.md
**Blocks:** task-04-auth-routes.md

**Context from dependencies:** task-01 created `AuthService` at `client/src/app/core/auth.service.ts` with a `login(email, password)` method returning an Observable of `LoginResponse { token: string, expiresAt: string }`. `setToken(token)` stores the JWT and updates `isAuthenticated`.

## Files to Create

- `client/src/app/features/auth/components/login/login.component.ts`
- `client/src/app/features/auth/components/login/login.component.html`
- `client/src/app/features/auth/components/login/login.component.scss`

## Files to Modify

None.

## Technical Details

### Code Snippets

```typescript
// login.component.ts
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../../../core/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, RouterLink],
  templateUrl: './login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  errorMessage = signal<string | null>(null);
  loading = signal(false);

  onSubmit(): void {
    if (this.form.invalid) return;
    const { email, password } = this.form.getRawValue();
    this.loading.set(true);
    this.errorMessage.set(null);

    this.authService.login(email!, password!).subscribe({
      next: (res) => {
        this.authService.setToken(res.token);
        this.router.navigateByUrl('/restaurants');
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Invalid email or password.');
      },
    });
  }
}
```

```html
<!-- login.component.html -->
<div class="auth-container">
  <h2>Sign In</h2>
  <form [formGroup]="form" (ngSubmit)="onSubmit()">
    <mat-form-field appearance="outline">
      <mat-label>Email</mat-label>
      <input matInput formControlName="email" type="email" autocomplete="email" />
    </mat-form-field>

    <mat-form-field appearance="outline">
      <mat-label>Password</mat-label>
      <input matInput formControlName="password" type="password" autocomplete="current-password" />
    </mat-form-field>

    @if (errorMessage()) {
      <p class="error-message">{{ errorMessage() }}</p>
    }

    <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || loading()">
      {{ loading() ? 'Signing In...' : 'Sign In' }}
    </button>
  </form>
  <a routerLink="/auth/register">Don't have an account? Register</a>
</div>
```

## Acceptance Criteria

- [ ] Login component exists at specified path
- [ ] Successful login stores token via `authService.setToken()` and navigates to `/restaurants`
- [ ] 401 response shows "Invalid email or password." inline (no field-level detail)
- [ ] `loading` signal disables the submit button during request
- [ ] `changeDetection: ChangeDetectionStrategy.OnPush` is set
