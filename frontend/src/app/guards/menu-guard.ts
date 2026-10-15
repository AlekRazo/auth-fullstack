import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth-service';
import { inject } from '@angular/core';

export const menuGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const currentUser = authService.currentUser();

  if (!currentUser) return router.createUrlTree(['/login']);

  const currentUrl = state.url;
  const hasPermissions = currentUser.menus.some(menu => menu.link == currentUrl);

  if (hasPermissions) return true;

  return router.createUrlTree(['/not-found']);
};
