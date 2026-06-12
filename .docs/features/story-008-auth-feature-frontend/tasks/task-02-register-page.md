# Task 02: Register Page

## Status

pending

## Wave

1

## Description

Create the `RegisterComponent` — the standalone Angular page mounted at `/register` that lets a new visitor create a TableNow account. It renders a reactive form (name, email, password), submits the credentials to the backend `POST /api/auth/register` endpoint through a thin auth API service, stores the returned JWT via `AuthService.login()`, and redirects the user to the restaurant listing. Backend errors (409 duplicate email, 400 validation) are mapped to inline, accessible error messages.

## Dependencies

**Depends on:** task-01-auth-core-service.md
**Blocks:** task-04-auth-routes.md

**Context from dependencies:** `task-01-auth-core-service.md` creates `AuthService` in `client/src/app/core/services/auth.service.ts`, providedIn `root`. It exposes `login(token: string): void` (persists the JWT to `localStorage` and flips the `isAuthenticated` computed signal) and `logout(): void`. After a successful registration response you call `auth.login(response.token)` and then navigate to `/restaurants`. Note: tasks 02 and 03 both create their own files in separate folders and share only this service contract, so they run in parallel.

## Files to Create

- `client/src/app/features/auth/components/register/register.component.ts` — Standalone RegisterComponent.
- `client/src/app/features/auth/components/register/register.component.html` — Template with the reactive form.
- `client/src/app/features/auth/components/register/register.component.scss` — Component styles.
- `client/src/app/features/auth/services/auth-api.service.ts` — Thin HTTP client for `register`/`login` (shared with task-03).
- `client/src/app/features/auth/models/auth.models.ts` — Request/response types (shared with task-03).
- `client/src/app/features/auth/components/register/register.component.spec.ts` — Unit tests for submit + error mapping.

## Files to Modify

- None.

## Technical Details

### Implementation Steps

1. Create the shared `auth.models.ts` with the request/response interfaces (see snippet). If task-03 creates the same file, coordinate to keep them identical; the content here is authoritative.
2. Create the shared `auth-api.service.ts` (`providedIn: 'root'`) with `register(payload)` and `login(payload)` methods returning typed Observables; base URL from `environment.apiBaseUrl`.
3. Create `RegisterComponent` as a standalone component with `changeDetection: ChangeDetectionStrategy.OnPush`.
4. Build a reactive form with `FormBuilder` (via `inject()`): `name` (`Validators.required`), `email` (`Validators.required`, `Validators.email`), `password` (`Validators.required`, `Validators.minLength(8)`).
5. Hold submission state in signals: `submitting = signal(false)`, `errorMessage = signal<string | null>(null)`.
6. On submit (only if `form.valid`): set `submitting(true)`, clear the error, call `authApi.register(...)`. On success call `auth.login(token)` then `router.navigate(['/restaurants'])`. On error map the status code to a message. Always reset `submitting(false)`.
7. Map errors: `409` → "An account with this email already exists."; `400` → "Please check the highlighted fields." (or surface server validation detail); anything else → "Something went wrong. Please try again."
8. Template: Angular Material form fields, `@if (errorMessage())` to render an inline error, disable the submit button while `submitting()` is true. Use `@if`/`@for` only — never `*ngIf`/`*ngFor`.

### Code Snippets

```ts
// auth.models.ts (shared with task-03)
export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface RegisterResponse {
  userId: string;
  name: string;
  email: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string; // ISO-8601
}
```

```ts
// auth-api.service.ts (shared with task-03)
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { LoginRequest, LoginResponse, RegisterRequest, RegisterResponse } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/auth`;

  register(payload: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.baseUrl}/register`, payload);
  }

  login(payload: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, payload);
  }
}
```

```ts
// register.component.ts (essentials)
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../../core/services/auth.service';
import { AuthApiService } from '../../services/auth-api.service';

@Component({
  selector: 'app-register',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule /*, Material modules */],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  submit(): void {
    if (this.form.invalid || this.submitting()) return;
    this.submitting.set(true);
    this.errorMessage.set(null);
    this.authApi.register(this.form.getRawValue()).subscribe({
      next: () => {
        // login on success then redirect; AuthService owns token storage
      },
      error: (err: HttpErrorResponse) => this.handleError(err),
    });
  }

  private handleError(err: HttpErrorResponse): void {
    this.submitting.set(false);
    this.errorMessage.set(
      err.status === 409
        ? 'An account with this email already exists.'
        : err.status === 400
          ? 'Please check the highlighted fields.'
          : 'Something went wrong. Please try again.',
    );
  }
}
```

> The project rule is "no `.subscribe()` in components." For login on success, prefer wiring the API call so token storage happens via `AuthService.login()` and navigation via the `Router`. If you keep an RxJS call, encapsulate it so the component reads as declarative — e.g. convert to a promise (`firstValueFrom`) inside the submit handler, or back the trigger with `rxResource`/`httpResource`. The snippet above shows the shape; the final implementation must comply with the no-`.subscribe()`-in-components convention. The success branch must call `this.auth.login(res.token)` then `this.router.navigate(['/restaurants'])`.

### API Endpoints

- `POST /api/auth/register` — Request `{ name, email, password }`; success `201` `{ userId, name, email }`; `409` on duplicate email; `400` on validation failure.

## Acceptance Criteria

- [ ] Navigating to `/register` (wired in task-04) renders a form with name, email, and password fields.
- [ ] The form validates required fields, email format, and a minimum password length before allowing submit.
- [ ] On a valid submit and successful API response, the JWT is stored via `AuthService.login()` and the user is redirected to `/restaurants`.
- [ ] A `409` response renders an inline "account already exists" message; a `400` renders a validation message.
- [ ] The submit button is disabled while a request is in flight.
- [ ] Component uses `OnPush`, `inject()`, reactive forms, and `@if`/`@for` only; no direct `localStorage` access.

## Notes

- `auth.models.ts` and `auth-api.service.ts` are shared with task-03 (login). They are listed here as the authoritative creator; the two tasks must produce byte-identical versions of those two files. If your orchestrator forbids two parallel tasks creating the same file, assign these two shared files to this task and have task-03 import them.
- Keep the JWT entirely inside `AuthService` — the component never reads or writes `localStorage`.
