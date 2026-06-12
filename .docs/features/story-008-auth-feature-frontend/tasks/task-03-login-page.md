# Task 03: Login Page

## Status

pending

## Wave

1

## Description

Create the `LoginComponent` — the standalone Angular page mounted at `/login` that lets a returning user sign in to TableNow. It renders a reactive form (email, password), submits the credentials to `POST /api/auth/login` through the shared auth API service, stores the returned JWT via `AuthService.login()`, and redirects to the restaurant listing. An invalid-credentials response (401) is mapped to a single generic inline error that does not reveal which field was wrong.

## Dependencies

**Depends on:** task-01-auth-core-service.md
**Blocks:** task-04-auth-routes.md

**Context from dependencies:** `task-01-auth-core-service.md` creates `AuthService` (`client/src/app/core/services/auth.service.ts`, `providedIn: 'root'`) exposing `login(token: string): void` and `logout(): void`. After a successful login response you call `auth.login(response.token)` then navigate to `/restaurants`. The shared `AuthApiService` and `auth.models.ts` (request/response types) are created by task-02 under `client/src/app/features/auth/services/` and `client/src/app/features/auth/models/`; this task imports them rather than recreating them. Tasks 02 and 03 run in parallel and share only the service contract.

## Files to Create

- `client/src/app/features/auth/components/login/login.component.ts` — Standalone LoginComponent.
- `client/src/app/features/auth/components/login/login.component.html` — Template with the reactive form.
- `client/src/app/features/auth/components/login/login.component.scss` — Component styles.
- `client/src/app/features/auth/components/login/login.component.spec.ts` — Unit tests for submit + 401 mapping.

## Files to Modify

- None.

## Technical Details

### Implementation Steps

1. Create `LoginComponent` as a standalone component with `changeDetection: ChangeDetectionStrategy.OnPush`.
2. Inject `FormBuilder`, `AuthApiService`, `AuthService`, and `Router` via `inject()`.
3. Build a reactive form: `email` (`Validators.required`, `Validators.email`), `password` (`Validators.required`).
4. Hold state in signals: `submitting = signal(false)`, `errorMessage = signal<string | null>(null)`.
5. On submit (only if `form.valid`): set `submitting(true)`, clear the error, call `authApi.login(...)`. On success call `auth.login(res.token)` then `router.navigate(['/restaurants'])`. On error map the status.
6. Map errors: `401` → "Invalid email or password." (generic — never indicate which field is wrong); anything else → "Something went wrong. Please try again."
7. Template: Angular Material form fields, `@if (errorMessage())` for the inline error, disable the submit button while `submitting()` is true. Use `@if`/`@for` only.

### Code Snippets

```ts
// login.component.ts (essentials)
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../../core/services/auth.service';
import { AuthApiService } from '../../services/auth-api.service';

@Component({
  selector: 'app-login',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule /*, Material modules */],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  submit(): void {
    if (this.form.invalid || this.submitting()) return;
    this.submitting.set(true);
    this.errorMessage.set(null);
    // call authApi.login(...), then auth.login(res.token) + router.navigate(['/restaurants'])
  }

  private handleError(err: HttpErrorResponse): void {
    this.submitting.set(false);
    this.errorMessage.set(
      err.status === 401
        ? 'Invalid email or password.'
        : 'Something went wrong. Please try again.',
    );
  }
}
```

> The component must comply with the "no `.subscribe()` in components" rule — encapsulate the RxJS call (e.g. `firstValueFrom`) inside the submit handler or back it with `httpResource`/`rxResource`. The success branch must call `this.auth.login(res.token)` then `this.router.navigate(['/restaurants'])`. The token is stored only through `AuthService` — never via `localStorage` directly.

### API Endpoints

- `POST /api/auth/login` — Request `{ email, password }`; success `200` `{ token, expiresAt }`; `401` on invalid credentials.

## Acceptance Criteria

- [ ] Navigating to `/login` (wired in task-04) renders a form with email and password fields.
- [ ] The form validates required fields and email format before allowing submit.
- [ ] On a valid submit and successful API response, the JWT is stored via `AuthService.login()` and the user is redirected to `/restaurants`.
- [ ] A `401` response renders a single generic "Invalid email or password." message with no field-specific detail.
- [ ] The submit button is disabled while a request is in flight.
- [ ] Component uses `OnPush`, `inject()`, reactive forms, and `@if`/`@for` only; no direct `localStorage` access.

## Notes

- Reuse `AuthApiService` and `auth.models.ts` created by task-02 — do not duplicate the HTTP client or the model types.
- The 401 message must be identical regardless of whether the email or the password was wrong (no account-enumeration leak).
