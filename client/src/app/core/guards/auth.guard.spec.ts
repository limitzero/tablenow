import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { authGuard } from './auth.guard';

function makeJwt(expOffsetSeconds: number): string {
  const payload = { exp: Math.floor(Date.now() / 1000) + expOffsetSeconds };
  return `header.${btoa(JSON.stringify(payload))}.sig`;
}

const mockRoute = {} as ActivatedRouteSnapshot;

function mockState(url: string): RouterStateSnapshot {
  return { url } as RouterStateSnapshot;
}

describe('authGuard', () => {
  let authService: AuthService;
  let router: Router;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter([])],
    });
    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
  });

  afterEach(() => localStorage.clear());

  it('returns true when the user is authenticated', () => {
    authService.login(makeJwt(3600));

    const result = TestBed.runInInjectionContext(() =>
      authGuard(mockRoute, mockState('/reservations')),
    );

    expect(result).toBe(true);
  });

  it('returns a UrlTree redirecting to /login when not authenticated', () => {
    const result = TestBed.runInInjectionContext(() =>
      authGuard(mockRoute, mockState('/reservations')),
    );

    expect(result).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(result as UrlTree)).toBe('/login?returnUrl=%2Freservations');
  });

  it('preserves the attempted URL as a returnUrl query param', () => {
    const result = TestBed.runInInjectionContext(() =>
      authGuard(mockRoute, mockState('/reservations/my')),
    );

    const tree = result as UrlTree;
    expect(tree.queryParams['returnUrl']).toBe('/reservations/my');
  });
});
