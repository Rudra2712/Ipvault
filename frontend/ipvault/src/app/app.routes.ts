import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './core/guards/auth.guard';
import { MainLayoutComponent } from './features/layout/main-layout.component';
import { LoginComponent } from './features/auth/login/login.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { PatentListComponent } from './features/patents/patent-list/patent-list.component';
import { PatentFormComponent } from './features/patents/patent-form/patent-form.component';
import { PatentDetailComponent } from './features/patents/patent-detail/patent-detail.component';
import { ClientListComponent } from './features/clients/client-list/client-list.component';
import { InventorListComponent } from './features/inventors/inventor-list/inventor-list.component';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        component: DashboardComponent
      },
      {
        path: 'patents',
        component: PatentListComponent
      },
      {
        path: 'patents/new',
        component: PatentFormComponent,
        canActivate: [roleGuard(['Admin', 'Attorney'])]
      },
      {
        path: 'patents/:id',
        component: PatentDetailComponent
      },
      {
        path: 'patents/:id/edit',
        component: PatentFormComponent,
        canActivate: [roleGuard(['Admin', 'Attorney'])]
      },
      {
        path: 'clients',
        component: ClientListComponent
      },
      {
        path: 'inventors',
        component: InventorListComponent
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
