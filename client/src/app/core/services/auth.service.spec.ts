import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';

const TOKEN_KEY = 'tablenow.jwt';

function makeJwt(expOffsetSeconds: number): string {
  const payload = { exp: Math.floor(Date.now() / 1000) + expOffsetSeconds };
  const encoded = btoa(JSON.stringify(payload));
  return `header.${encoded}.sig`;
}

describe('AuthService', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
  });

  afterEach(() => localStorage.clear());

  it('starts unauthenticated when localStorage is empty', () => {
    const service = TestBed.inject(AuthService);
    expect(service.isAuthenticated()).toBe(false);
  });

  it('login() stores the token and flips isAuthenticated to true', () => {
    const service = TestBed.inject(AuthService);
    const token = makeJwt(3600);
    service.login(token);
    expect(localStorage.getItem(TOKEN_KEY)).toBe(token);
    expect(service.isAuthenticated()).toBe(true);
    expect(service.token()).toBe(token);
  });

  it('logout() removes the token and flips isAuthenticated to false', () => {
    const service = TestBed.inject(AuthService);
    service.login(makeJwt(3600));
    service.logout();
    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
    expect(service.token()).toBeNull();
  });

  it('returns false for an expired token', () => {
    const service = TestBed.inject(AuthService);
    service.login(makeJwt(-1));
    expect(service.isAuthenticated()).toBe(false);
  });

  it('returns false for a malformed token', () => {
    const service = TestBed.inject(AuthService);
    service.login('not.valid.jwt');
    expect(service.isAuthenticated()).toBe(false);
  });

  it('returns false for a token with no exp claim', () => {
    const service = TestBed.inject(AuthService);
    const payload = btoa(JSON.stringify({ sub: 'user' }));
    service.login(`header.${payload}.sig`);
    expect(service.isAuthenticated()).toBe(false);
  });

  it('honors an existing valid token in localStorage on construction', () => {
    const token = makeJwt(3600);
    localStorage.setItem(TOKEN_KEY, token);
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({});
    const service = TestBed.inject(AuthService);
    expect(service.isAuthenticated()).toBe(true);
  });
});
