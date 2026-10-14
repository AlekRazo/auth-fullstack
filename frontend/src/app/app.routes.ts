import { FullscreenOverlayContainer } from '@angular/cdk/overlay';
import { Routes } from '@angular/router';

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
        canMatch: ['authGuaard'],
        loadComponent: () => import('./views/layouts/main-layout/main-layout').then(m => m.MainLayout),
        children: [
            {
                path: 'home',
                loadComponent: () => import('./views/pages/home/home').then(m => m.Home)
            },
            {
                path: 'admin',
                loadComponent: () => import('./views/pages/admin/admin').then(m => m.Admin)
            },
            {
                path: 'seller',
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
