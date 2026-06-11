# Task 02: Register Component

## Status

pending

## Wave

1

## Description

Creates the registration form component at `features/auth/components/register.component.ts`. Reactive form with name, email, password validation. POSTs to `/api/auth/register`. On 201 redirects to `/restaurants`. On 409 displays inline error. OnPush change detection.

## Dependencies

**Depends on:** None (Wave 1 — parallel with task-01 and task-03, different files)
**Blocks:** task-04-auth-routes.md

**Context from dependencies:** STORY-002 established the Angular project. STORY-006 created `POST /api/auth/register` on the backend at `http://localhost:5000/api/auth/register`. The `environment.ts` `apiBaseUrl` from STORY-002 task-02 provides the base URL. This component does NOT use `AuthService` (registration doesn't log you in — the user must then login).

## Files to Create

- `client/src/app/features/auth/components/register.component.ts`

## Technical Details

### Code Snippets

```typescript
// client/src/app/features/auth/components/register.component.ts
import { Component, ChangeDetectionStrategy, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="auth-container">
      <h1>Create Account</h1>
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <mat-form-field>
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" />
        </mat-form-field>
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
          {{ loading() ? 'Creating...' : 'Create Account' }}
        </button>
      </form>
      <p>Already have an account? <a routerLink="/login">Sign in</a></p>
    </div>
  `,
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  readonly errorMessage = signal<string | null>(null);
  readonly loading = signal(false);

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.errorMessage.set(null);

    this.http.post(`${environment.apiBaseUrl}/auth/register`, this.form.value).subscribe({
      next: () => this.router.navigate(['/restaurants']),
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(
          err.status === 409
            ? 'An account with this email already exists.'
            : 'Registration failed. Please try again.'
        );
      },
      complete: () => this.loading.set(false),
    });
  }
}
```

**Note:** `.subscribe()` is used here on a one-shot HTTP call inside an event handler — this is the accepted pattern when `httpResource()` doesn't apply (form submissions triggered by user action).

## Acceptance Criteria

- [ ] Component exists at `features/auth/components/register.component.ts`
- [ ] Reactive form with name (required), email (required, email format), password (required, minLength 8)
- [ ] On 201: navigates to `/restaurants`
- [ ] On 409: shows inline error "An account with this email already exists."
- [ ] Loading state disables button during API call
- [ ] `OnPush` change detection
- [ ] Standalone component with `inject()` for DI
