import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

import { AuthService } from '../../services';

@Component({
  selector: 'steam-session-expired-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './session-expired-page.html',
  styleUrl: './session-expired-page.scss',
})
export class SessionExpiredPage {
  constructor(
    private readonly auth: AuthService,
    private readonly router: Router,
  ) {}

  goToLogin(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/login', { replaceUrl: true });
  }
}
