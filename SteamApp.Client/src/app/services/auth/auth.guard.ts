import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree, CanActivateChild } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate, CanActivateChild  {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(): boolean | UrlTree {
    if (this.auth.isLoggedIn()) {
      return true;
    }
    return this.router.createUrlTree(['/login']);
  }

    canActivateChild(): boolean | UrlTree {
    return this.canActivate();
  }
}
