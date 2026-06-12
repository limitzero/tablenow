import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./features/auth').then((m) => m.AUTH_ROUTES),
  },
];
