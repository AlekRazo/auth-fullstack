import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { form, required, FormField } from '@angular/forms/signals';
import { AuthService } from '../../../services/auth-service';
import { RegisterRequest } from '../../../interfaces/register-request';
import { RouterLink } from '@angular/router';

@Component({
  imports: [MatCardModule, MatButtonModule, MatInputModule, MatFormFieldModule, FormField, RouterLink],
  selector: 'app-register',
  styleUrl: './register.scss',
  templateUrl: './register.html',
})
export class Register {
  private initialData = {
    name: '',
    email:'',
    password:''
  };

  protected formModel = signal(this.initialData);
  protected myForm = form(this.formModel, (path) => {
    required(path.name);
    required(path.email);
    required(path.password);
  });
  protected authService = inject(AuthService);

  register(): void {
    const { name, email, password } = this.myForm().value();
    const req: RegisterRequest = {
      name, 
      email, 
      password, 
      idRole: 2
    }

    this.authService.register(req).subscribe({
      next:() => {
        alert("success");
        this.formModel.set(this.initialData);
        this.myForm().reset();
      },
      error: (e) => console.log(e.error)
    })
  }
}
