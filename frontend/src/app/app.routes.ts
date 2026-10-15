import { FullscreenOverlayContainer } from '@angular/cdk/overlay';
import { Routes } from '@angular/router';
import { authGuard } from './guards/auth-guard';
import { menuGuard } from './guards/menu-guard';

export const routes: Routes = [
    {
        path: 'login',
        loadComponent: () => import('./views/pages/login/login').then(m => m.Login)
    },
    {
        path: 'register',
        loadComponent: () => import('./views/pages/register/register').then(m => m.Register)
    },
    {
        path: '',
        canMatch: [authGuard],
        loadComponent: () => import('./views/layouts/main-layout/main-layout').then(m => m.MainLayout),
        children: [
            {
                path: 'home',
                canActivate: [menuGuard],
                loadComponent: () => import('./views/pages/home/home').then(m => m.Home)
            },
            {
                path: 'admin',
                canActivate: [menuGuard],
                loadComponent: () => import('./views/pages/admin/admin').then(m => m.Admin)
            },
            {
                path: 'seller',
                canActivate: [menuGuard],
                loadComponent: () => import('./views/pages/seller/seller').then(m => m.Seller)
            },
            {
                path: 'notfound',
                loadComponent: () => import('./views/pages/not-found/not-found').then(m => m.NotFound)
            },
            {
                path: '',
                redirectTo: 'home',
                pathMatch: 'full'
            },
            {
                path: '**',
                redirectTo: 'notfound',
                pathMatch: 'full'
            }
        ]
    },
    {
        path: '**',
        redirectTo: 'login',
        pathMatch: 'full'
    }
];
