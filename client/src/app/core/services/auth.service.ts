import { Injectable, computed, signal } from '@angular/core';

const TOKEN_KEY = 'tablenow.jwt';

interface JwtPayload {
  exp?: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));

  readonly token = this._token.asReadonly();

  readonly isAuthenticated = computed(() => {
    const t = this._token();
    return t !== null && !this.isExpired(t);
  });

  login(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
    this._token.set(token);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    this._token.set(null);
  }

  private isExpired(token: string): boolean {
    const payload = this.decode(token);
    if (payload?.exp === undefined) return true;
    return payload.exp * 1000 <= Date.now();
  }

  private decode(token: string): JwtPayload | null {
    try {
      const [, segment] = token.split('.');
      if (!segment) return null;
      const json = atob(segment.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(json) as JwtPayload;
    } catch {
      return null;
    }
  }
}
