import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { CommonModule } from '@angular/common';
import { CountdownTimerComponent } from "../countdown-timer.component";

@Component({
  selector: 'steam-site-header',
  standalone: true,
  imports: [CommonModule, RouterModule, CountdownTimerComponent],
  templateUrl: './site-header.component.html',
  styleUrl: './site-header.component.scss',
})
export class SiteHeaderComponent {
  constructor(private auth: AuthService, private router:Router) {}

  logout(event: MouseEvent) {
    event.preventDefault();  
    this.auth.logout();    
     this.router.navigate(['login']);
  }

  isLoggedIn() {
    return this.auth.isLoggedIn();
  }
}
