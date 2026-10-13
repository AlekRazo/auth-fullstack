import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const isMyApi = req.url.startsWith(environment.apiUrl);
  console.log(req.url);

  if (!isMyApi) return next(req);

  const reqClone = req.clone({withCredentials: true});

  return next(reqClone)
  .pipe(
    catchError((error: HttpErrorResponse) => {
      console.log("Error from AuthInterceptor.")
      if(error.status === 401 && !router.url.startsWith('/login')){
        router.navigate(['/login']);
      }

      return throwError(() => error);
    }));
};
