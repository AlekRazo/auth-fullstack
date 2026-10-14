import { inject, Service, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { GetUserResponse } from '../interfaces/get-user-response';
import { RegisterRequest } from '../interfaces/register-request';
import { catchError, finalize, Observable, tap, throwError } from 'rxjs';
import { LoginRequest } from '../interfaces/login-request';

@Service()
export class AuthService {
    private endpoint = `${environment.apiUrl}/auth`;
    private http = inject(HttpClient);
    public currentUser = signal<GetUserResponse | null>(null);

    register(request: RegisterRequest): Observable<void>{
        return this.http.post<void>(`${this.endpoint}/register`, request);
    }

    login(request: LoginRequest): Observable<void>{
        return this.http.post<void>(`${this.endpoint}/login`, request);
    }

    loadCurrentUser(): Observable<GetUserResponse>{
        return this.http.get<GetUserResponse>(`${this.endpoint}/me`)
        .pipe(
            tap(user => {
                this.currentUser.set(user);
            }),
            catchError((error) => {
                console.log("Error from AuthService.");
                console.log(error);
                this.currentUser.set(null);
                return throwError(() => error);
            })
        );
    }

    logout(): Observable<void>{
        return this.http.post<void>(`${this.endpoint}/logout`, {})
        .pipe(
            finalize(() => {
                this.currentUser.set(null);
            })
        );
    }
}
