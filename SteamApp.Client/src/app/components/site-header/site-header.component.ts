import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { CommonModule } from '@angular/common';
import { CountdownTimerComponent } from '../countdown-timer.component';
import { DropdownComponent } from "../dropdown/dropdown.component";

@Component({
    selector: 'steam-site-header',
    imports: [CommonModule, RouterModule, CountdownTimerComponent, DropdownComponent],
    templateUrl: './site-header.component.html',
    styleUrl: './site-header.component.scss'
})
export class SiteHeaderComponent {
  expiration: number = 0;

  constructor(private auth: AuthService, private router: Router) {}

  ngOnInit() {
    this.getTimeUntilExpirationDate();
  }

  getTimeUntilExpirationDate(): void {
    if (this.expiration === 0) {
      this.expiration = this.auth.getTimeBeforeExpiration();
    }
  }

  logout(event: MouseEvent) {
    event.preventDefault();
    this.auth.logout();
    this.expiration = 0;
    this.router.navigate(['login']);
  }

  isLoggedIn() {
    const isLogged = this.auth.isLoggedIn();
    if (isLogged) {
      this.getTimeUntilExpirationDate();
    }

    return isLogged;
  }
}
