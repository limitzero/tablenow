# Task 01: Auth Service

## Status

pending

## Wave

1

## Description

Creates `AuthService` in `core/` with methods for register, login, logout, and token management. Exposes `isAuthenticated` as a computed Signal derived from the token in localStorage. This service is the single source of truth for authentication state throughout the app.

## Dependencies

**Depends on:** STORY-002 task-03-environment-config.md (environment.ts with apiBaseUrl must exist)
**Blocks:** task-02-register-component.md, task-03-login-component.md

**Context from dependencies:** STORY-002 established `environment.ts` with `apiBaseUrl: 'http://localhost:5000/api'` and wired `provideHttpClient(withInterceptors([]))` in `app.config.ts`. The auth service uses `HttpClient` via `inject()`.

## Files to Create

- `client/src/app/core/auth.service.ts` — Auth service with token management and signals
- `client/src/app/core/models/auth.models.ts` — TypeScript interfaces for auth API responses

## Files to Modify

- `client/src/app/core/index.ts` — Export `AuthService`

## Technical Details

### Code Snippets

```typescript
// client/src/app/core/models/auth.models.ts
export interface LoginResponse {
  token: string;
  expiresAt: string;
}

export interface RegisterResponse {
  userId: string;
  name: string;
  email: string;
}
```

```typescript
// client/src/app/core/auth.service.ts
import { inject, Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { LoginResponse, RegisterResponse } from './models/auth.models';

const TOKEN_KEY = 'tablenow_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiUrl = `${environment.apiBaseUrl}/auth`;

  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  readonly isAuthenticated = computed(() => this._token() !== null);
  readonly token = this._token.asReadonly();

  register(name: string, email: string, password: string) {
    return this.http.post<RegisterResponse>(`${this.apiUrl}/register`, { name, email, password });
  }

  login(email: string, password: string) {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, { email, password });
  }

  setToken(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
    this._token.set(token);
  }

  clearToken(): void {
    localStorage.removeItem(TOKEN_KEY);
    this._token.set(null);
  }

  logout(): void {
    this.clearToken();
    this.router.navigateByUrl('/auth/login');
  }
}
```

## Acceptance Criteria

- [ ] `AuthService` exists at `client/src/app/core/auth.service.ts`
- [ ] `isAuthenticated` is a computed Signal returning `true` when a token exists in localStorage
- [ ] `setToken()` stores the token in localStorage and updates the signal
- [ ] `clearToken()` removes from localStorage and updates the signal
- [ ] `login()` and `register()` return Observables (not subscribed here)
- [ ] `npm run build` exits with code 0
