# Task 01: AuthService

## Status

pending

## Wave

1

## Description

Creates the `AuthService` in `core/services/` — the single source of truth for JWT storage and auth state. Exposes an `isAuthenticated` computed Signal that route guards (STORY-009) and components read throughout the app.

## Dependencies

**Depends on:** None (Wave 1 — requires STORY-002 folder structure)
**Blocks:** task-04-auth-routes.md, STORY-009

**Context from dependencies:** STORY-002 created `client/src/app/core/services/` directory and `core/index.ts` barrel. This task populates that directory with the auth service. Does not overlap with task-02 or task-03.

## Files to Create

- `client/src/app/core/services/auth.service.ts`

## Files to Modify

- `client/src/app/core/index.ts` — add `AuthService` export

## Technical Details

### Code Snippets

```typescript
// client/src/app/core/services/auth.service.ts
import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';

const TOKEN_KEY = 'auth_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));

  readonly isAuthenticated = computed(() => this._token() !== null);

  storeToken(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
    this._token.set(token);
  }

  clearToken(): void {
    localStorage.removeItem(TOKEN_KEY);
    this._token.set(null);
  }

  getToken(): string | null {
    return this._token();
  }
}
```

```typescript
// client/src/app/core/index.ts
export { AuthService } from './services/auth.service';
```

## Acceptance Criteria

- [ ] `AuthService` exists in `client/src/app/core/services/auth.service.ts`
- [ ] `isAuthenticated` is a `computed` Signal derived from the token signal
- [ ] `storeToken` updates both `localStorage` and the internal signal
- [ ] `clearToken` removes from `localStorage` and sets signal to null
- [ ] `AuthService` is exported from `core/index.ts`
- [ ] No constructor injection — uses `inject()` pattern (Router injected if needed)
