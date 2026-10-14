import { CanMatchFn, Router } from '@angular/router';
import { AuthService } from '../services/auth-service';
import { inject } from '@angular/core';

export const authGuard: CanMatchFn = (route, segments) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  console.log("authGuard GO!")
  if(authService.currentUser()) return true;
  return router.createUrlTree(['/login']);
};
