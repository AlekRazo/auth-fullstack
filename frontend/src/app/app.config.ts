import { ApplicationConfig, inject, provideAppInitializer, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './interceptors/auth-interceptor';
import { AuthService } from './services/auth-service';
import { catchError, firstValueFrom, of, throwError } from 'rxjs';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAppInitializer(() => {
      const authService = inject(AuthService);

      return firstValueFrom(
        authService.loadCurrentUser().pipe(
          catchError((error: HttpErrorResponse) => {
            console.log("Error from initializer.");
            if(error.status == 401) return of(null);

            return throwError(() => error)
          })
        )
      );
    })
  ]
};
