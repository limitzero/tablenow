import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { authInterceptor } from './auth.interceptor';

const API_URL = 'http://localhost:5000/api/test';
const EXTERNAL_URL = 'https://external.example.com/data';

function makeJwt(expOffsetSeconds: number): string {
  const payload = { exp: Math.floor(Date.now() / 1000) + expOffsetSeconds };
  return `header.${btoa(JSON.stringify(payload))}.sig`;
}

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpTesting: HttpTestingController;
  let authService: AuthService;
  let router: Router;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });
    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
  });

  afterEach(() => {
    httpTesting.verify();
    localStorage.clear();
  });

  it('attaches Authorization header for API requests when a token is stored', () => {
    const token = makeJwt(3600);
    authService.login(token);

    http.get(API_URL).subscribe();

    const req = httpTesting.expectOne(API_URL);
    expect(req.request.headers.get('Authorization')).toBe(`Bearer ${token}`);
    req.flush({});
  });

  it('does not attach Authorization header when no token is stored', () => {
    http.get(API_URL).subscribe();

    const req = httpTesting.expectOne(API_URL);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('does not overwrite an existing Authorization header', () => {
    authService.login(makeJwt(3600));

    http.get(API_URL, { headers: { Authorization: 'Basic custom' } }).subscribe();

    const req = httpTesting.expectOne(API_URL);
    expect(req.request.headers.get('Authorization')).toBe('Basic custom');
    req.flush({});
  });

  it('does not attach header for requests outside the API base URL', () => {
    authService.login(makeJwt(3600));

    http.get(EXTERNAL_URL).subscribe();

    const req = httpTesting.expectOne(EXTERNAL_URL);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('calls logout() and redirects to /login on 401, then propagates the error', () => {
    authService.login(makeJwt(3600));
    const navigateSpy = vi.spyOn(router, 'navigate');

    let errorPropagated = false;
    http.get(API_URL).subscribe({ error: () => (errorPropagated = true) });

    const req = httpTesting.expectOne(API_URL);
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    expect(authService.isAuthenticated()).toBe(false);
    expect(navigateSpy).toHaveBeenCalledWith(['/login']);
    expect(errorPropagated).toBe(true);
  });

  it('does not call logout() on non-401 errors', () => {
    authService.login(makeJwt(3600));
    const logoutSpy = vi.spyOn(authService, 'logout');

    http.get(API_URL).subscribe({ error: (_e: unknown) => _e });

    const req = httpTesting.expectOne(API_URL);
    req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });

    expect(logoutSpy).not.toHaveBeenCalled();
  });
});
