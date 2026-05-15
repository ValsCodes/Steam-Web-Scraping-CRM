import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';

import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'steam-site-footer',
  imports: [CommonModule, RouterModule],
  templateUrl: './site-footer.html',
  styleUrl: './site-footer.scss',
  standalone: true,
})
export class SiteFooter {
  readonly loggedIn$ = inject(AuthService).loggedIn$;
}
