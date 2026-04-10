import { Routes } from '@angular/router';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { DashboardComponent } from './stock/dashboard/dashboard.component';
import { LoginComponent } from './login/login.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    {
        path: '',
        component: MainLayoutComponent,
        children: [
            {
                path: 'dashboard',
                component: DashboardComponent
            },
        ],
        canActivate: [authGuard]
    },
    {
        path: 'login',
        component: LoginComponent
    },
    {
        path: '**',
        redirectTo: '',
    }
];
