import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { Component } from '@angular/core';
import { LoginComponent } from './login.component';

@Component({ standalone: true, template: '' })
class RestaurantsStub {}

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let component: LoginComponent;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: 'restaurants', component: RestaurantsStub }]),
        provideAnimationsAsync('noop'),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => httpMock.verify());

  it('renders the login form', () => {
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('form')).toBeTruthy();
  });

  it('does not submit when the form is invalid', () => {
    component.form.setValue({ email: 'not-an-email', password: '' });
    component.submit();
    httpMock.expectNone(() => true);
  });

  it('shows a generic error on 401', async () => {
    component.form.setValue({ email: 'a@example.com', password: 'password123' });
    const p = component.submit();
    httpMock.expectOne((r) => r.url.includes('/auth/login')).flush(
      {},
      { status: 401, statusText: 'Unauthorized' },
    );
    await p;
    expect(component.errorMessage()).toBe('Invalid email or password.');
    expect(component.submitting()).toBe(false);
  });

  it('shows a generic error on unexpected failure', async () => {
    component.form.setValue({ email: 'a@example.com', password: 'password123' });
    const p = component.submit();
    httpMock.expectOne((r) => r.url.includes('/auth/login')).flush(
      {},
      { status: 500, statusText: 'Server Error' },
    );
    await p;
    expect(component.errorMessage()).toBe('Something went wrong. Please try again.');
  });

  it('stores the JWT and navigates on success', async () => {
    localStorage.clear();
    component.form.setValue({ email: 'a@example.com', password: 'password123' });
    const exp = Math.floor(Date.now() / 1000) + 3600;
    const payload = btoa(JSON.stringify({ exp }));
    const token = `header.${payload}.sig`;

    const p = component.submit();
    httpMock.expectOne((r) => r.url.includes('/auth/login')).flush(
      { token, expiresAt: new Date(exp * 1000).toISOString() },
      { status: 200, statusText: 'OK' },
    );
    await p;

    expect(localStorage.getItem('tablenow.jwt')).toBe(token);
    expect(component.errorMessage()).toBeNull();
  });
});
