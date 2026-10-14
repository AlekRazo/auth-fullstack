import { Component, inject, signal } from '@angular/core';
import {MatButtonModule} from '@angular/material/button';
import {MatCardModule} from '@angular/material/card';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';
import { Router, RouterLink } from '@angular/router';
import { LoginRequest } from '../../../interfaces/login-request';
import { form, required, FormField } from '@angular/forms/signals';
import { AuthService } from '../../../services/auth-service';
import { switchMap } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  imports: [MatCardModule, MatButtonModule, MatInputModule, MatFormFieldModule, RouterLink, FormField],
  selector: 'app-login',
  styleUrl: './login.scss',
  templateUrl: './login.html',
})
export class Login {
  private initialData = {
    email:'',
    password:''
  };
  private router = inject(Router);

  protected formModel = signal(this.initialData);
  protected myForm = form(this.formModel, (path) => {
    required(path.email);
    required(path.password);
  });
  protected authService = inject(AuthService);

  login(): void {
    const { email, password } = this.myForm().value();
    const req: LoginRequest = {
      email, 
      password
    }

    this.authService.login(req).pipe(
      switchMap(() => this.authService.loadCurrentUser())
    ).subscribe({
      next:(user) => {
        console.log(user);
        this.router.navigate(['/'])
      },
      error: (error:HttpErrorResponse) => {
        if(error.status === 401){
          alert("Email o contraseña incorrectos.");
        }
        else{
          alert("Ocurrió un error al iniciar la sesión.")
        }
      }
    })
  }
}
 