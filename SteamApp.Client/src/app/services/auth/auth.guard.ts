import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(): boolean | UrlTree {
    // If the user is logged in (token exists & not expired), allow
    if (this.auth.isLoggedIn()) {
      return true;
    }
    // Otherwise redirect to /login
    return this.router.createUrlTree(['/login']);
  }
}
