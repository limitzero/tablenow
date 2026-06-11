# Task 02: Register Component

## Status

pending

## Wave

2

## Description

Creates the Register form component at `/auth/register`. Uses Angular reactive forms for validation (name required, email format, password ≥ 8 chars). On success, calls `AuthService.setToken()` with the response and navigates to `/restaurants`. API errors (409 conflict) are shown inline.

## Dependencies

**Depends on:** task-01-auth-service.md
**Blocks:** task-04-auth-routes.md

**Context from dependencies:** task-01 created `AuthService` at `client/src/app/core/auth.service.ts` with a `register(name, email, password)` method that returns an Observable. `isAuthenticated` is a computed Signal.

## Files to Create

- `client/src/app/features/auth/components/register/register.component.ts`
- `client/src/app/features/auth/components/register/register.component.html`
- `client/src/app/features/auth/components/register/register.component.scss`

## Files to Modify

None.

## Technical Details

### Code Snippets

```typescript
// register.component.ts
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../../../core/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './register.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  errorMessage = signal<string | null>(null);
  loading = signal(false);

  onSubmit(): void {
    if (this.form.invalid) return;
    const { name, email, password } = this.form.getRawValue();
    this.loading.set(true);
    this.errorMessage.set(null);

    this.authService.register(name!, email!, password!).subscribe({
      next: () => {
        // After register, prompt to login
        this.router.navigateByUrl('/auth/login');
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(
          err.status === 409 ? 'Email already registered.' : 'Registration failed. Please try again.'
        );
      },
    });
  }
}
```

```html
<!-- register.component.html -->
<div class="auth-container">
  <h2>Create Account</h2>
  <form [formGroup]="form" (ngSubmit)="onSubmit()">
    <mat-form-field appearance="outline">
      <mat-label>Name</mat-label>
      <input matInput formControlName="name" autocomplete="name" />
    </mat-form-field>

    <mat-form-field appearance="outline">
      <mat-label>Email</mat-label>
      <input matInput formControlName="email" type="email" autocomplete="email" />
    </mat-form-field>

    <mat-form-field appearance="outline">
      <mat-label>Password</mat-label>
      <input matInput formControlName="password" type="password" autocomplete="new-password" />
    </mat-form-field>

    @if (errorMessage()) {
      <p class="error-message">{{ errorMessage() }}</p>
    }

    <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || loading()">
      {{ loading() ? 'Creating Account...' : 'Register' }}
    </button>
  </form>
  <a routerLink="/auth/login">Already have an account? Sign in</a>
</div>
```

## Acceptance Criteria

- [ ] Register component exists at specified path
- [ ] Form validates name (required), email (format), password (min 8 chars)
- [ ] Successful registration navigates to `/auth/login`
- [ ] 409 response shows "Email already registered." inline
- [ ] `loading` signal disables the submit button during the request
- [ ] `changeDetection: ChangeDetectionStrategy.OnPush` is set
- [ ] `@if` used for conditional rendering (no `*ngIf`)
