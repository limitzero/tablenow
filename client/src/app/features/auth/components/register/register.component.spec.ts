import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { RegisterComponent } from './register.component';

describe('RegisterComponent', () => {
  let fixture: ComponentFixture<RegisterComponent>;
  let component: RegisterComponent;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideAnimationsAsync('noop'),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => httpMock.verify());

  it('renders the registration form', () => {
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('form')).toBeTruthy();
  });

  it('does not submit when the form is invalid', () => {
    component.form.setValue({ name: '', email: 'bad', password: '123' });
    component.submit();
    httpMock.expectNone(() => true);
  });

  it('shows a duplicate-email error on 409', async () => {
    component.form.setValue({ name: 'Alice', email: 'a@example.com', password: 'password123' });
    const p = component.submit();
    httpMock.expectOne((r) => r.url.includes('/auth/register')).flush(
      {},
      { status: 409, statusText: 'Conflict' },
    );
    await p;
    expect(component.errorMessage()).toBe('An account with this email already exists.');
    expect(component.submitting()).toBe(false);
  });

  it('shows a validation error on 400', async () => {
    component.form.setValue({ name: 'Alice', email: 'a@example.com', password: 'password123' });
    const p = component.submit();
    httpMock.expectOne((r) => r.url.includes('/auth/register')).flush(
      {},
      { status: 400, statusText: 'Bad Request' },
    );
    await p;
    expect(component.errorMessage()).toBe('Please check the highlighted fields.');
  });

  it('shows a generic error on unexpected failure', async () => {
    component.form.setValue({ name: 'Alice', email: 'a@example.com', password: 'password123' });
    const p = component.submit();
    httpMock.expectOne((r) => r.url.includes('/auth/register')).flush(
      {},
      { status: 500, statusText: 'Server Error' },
    );
    await p;
    expect(component.errorMessage()).toBe('Something went wrong. Please try again.');
  });
});
