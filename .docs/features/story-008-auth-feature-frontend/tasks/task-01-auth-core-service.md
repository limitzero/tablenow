# Task 01: Auth Core Service

## Status

pending

## Wave

1

## Description

Create the `AuthService` that owns all client-side authentication state for TableNow. It persists the JWT to `localStorage`, exposes authentication state as a computed Angular Signal (`isAuthenticated`), and provides `login(token)` / `logout()` methods. This service is the single source of truth for the token: the Register and Login pages (tasks 02/03) call `login()` after a successful API response, and the future HTTP interceptor and route guard (STORY-009) read the token and the `isAuthenticated` signal from here. Centralizing storage means no component or interceptor ever touches `localStorage` directly.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-02-register-page.md, task-03-login-page.md, task-04-auth-routes.md

**Context from dependencies:** This is a Wave 1 task with no upstream dependencies. It assumes the STORY-002 Angular 21 scaffolding exists (standalone components, `bootstrapApplication`, `provideHttpClient`, `environment.ts` exposing `apiBaseUrl = 'http://localhost:5000/api'`). The service contract you define here — `isAuthenticated` (computed Signal), `token` (read-only Signal), `login(token: string)`, `logout()` — is the contract that downstream tasks consume.

## Files to Create

- `client/src/app/core/services/auth.service.ts` — The root-provided authentication service.
- `client/src/app/core/services/auth.service.spec.ts` — Unit tests (Vitest) for storage, signal state, and expiry handling.

## Files to Modify

- None.

## Technical Details

### Implementation Steps

1. Create `auth.service.ts` decorated with `@Injectable({ providedIn: 'root' })`.
2. Define a constant for the storage key, e.g. `const TOKEN_KEY = 'tablenow.jwt';`.
3. Hold the raw token in a private `WritableSignal<string | null>`, seeded from `localStorage.getItem(TOKEN_KEY)`.
4. Expose a public read-only `token` signal (`asReadonly()`) for the future interceptor.
5. Expose `isAuthenticated = computed(() => { const t = this._token(); return !!t && !this.isExpired(t); })`.
6. Implement `login(token: string): void` — write to `localStorage`, set the signal.
7. Implement `logout(): void` — remove from `localStorage`, set the signal to `null`.
8. Implement a private `isExpired(token: string): boolean` that decodes the JWT payload (`exp` claim) and compares to `Date.now()`. Treat any decode failure as expired.
9. Use the `inject()` function for any dependencies (none strictly required here; do not use constructor injection).

### Code Snippets

```ts
import { Injectable, computed, signal } from '@angular/core';

const TOKEN_KEY = 'tablenow.jwt';

interface JwtPayload {
  exp?: number; // seconds since epoch
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _token = signal<string | null>(this.readToken());

  /** Raw JWT, exposed read-only for the HTTP interceptor (STORY-009). */
  readonly token = this._token.asReadonly();

  /** True when a non-expired token is present. */
  readonly isAuthenticated = computed(() => {
    const token = this._token();
    return token !== null && !this.isExpired(token);
  });

  login(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
    this._token.set(token);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    this._token.set(null);
  }

  private readToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  private isExpired(token: string): boolean {
    const payload = this.decode(token);
    if (payload?.exp === undefined) {
      return true; // no expiry claim → treat as invalid
    }
    return payload.exp * 1000 <= Date.now();
  }

  private decode(token: string): JwtPayload | null {
    try {
      const [, payloadSegment] = token.split('.');
      if (!payloadSegment) return null;
      const json = atob(payloadSegment.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(json) as JwtPayload;
    } catch {
      return null;
    }
  }
}
```

## Acceptance Criteria

- [ ] `AuthService` is `providedIn: 'root'` and constructed with no constructor parameters (DI via `inject()` only).
- [ ] `login(token)` writes the token to `localStorage` under a single dedicated key and flips `isAuthenticated` to `true` for a non-expired token.
- [ ] `logout()` removes the token from `localStorage` and flips `isAuthenticated` to `false`.
- [ ] `isAuthenticated` is a `computed` Signal that returns `false` when no token is stored, when the token is expired, or when the token is malformed.
- [ ] A read-only `token` signal is exposed for downstream consumers.
- [ ] On service construction, an existing valid token in `localStorage` is honored (user stays signed in across reloads).

## Notes

- Base64 decoding uses `atob`; convert URL-safe base64 (`-`/`_`) before decoding. A malformed token must never throw — return `null`/expired instead.
- Do not add the HTTP interceptor or route guard here; those belong to STORY-009 and only consume this service.
