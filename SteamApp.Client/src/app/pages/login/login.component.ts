import { Component } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'steam-login',
    imports: [FormsModule, CommonModule],
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss'
})

export class LoginComponent {
  clientId = '';
  clientSecret = '';
  error?: string;
  success?: string;

  constructor(
    private auth: AuthService,
    private router: Router
  ) {}

  onSubmit() {
    this.auth.login(this.clientId, this.clientSecret)
      .subscribe({
        next: () => this.success = 'Successfully Authenticated!',
        error:  () => this.error = 'Login failed'
      });
  }
}
