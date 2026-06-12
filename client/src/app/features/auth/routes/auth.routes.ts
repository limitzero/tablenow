import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  {
    path: 'register',
    loadComponent: () =>
      import('../components/register/register.component').then((m) => m.RegisterComponent),
    title: 'Create your account',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('../components/login/login.component').then((m) => m.LoginComponent),
    title: 'Sign in',
  },
];
