import { Injectable } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivate,
  Router,
  UrlTree,
  CanActivateChild,
} from '@angular/router';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate, CanActivateChild  {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean | UrlTree {
    return this.authorize(route);
  }

  canActivateChild(childRoute: ActivatedRouteSnapshot): boolean | UrlTree {
    return this.authorize(childRoute);
  }

  private authorize(route: ActivatedRouteSnapshot): boolean | UrlTree {
    if (this.auth.isLoggedIn()) {
      const requiredRoles = this.getRequiredRoles(route);
      if (requiredRoles.length === 0) {
        return true;
      }

      const currentUser = this.auth.getCurrentUser();
      if (currentUser && requiredRoles.some(role => this.hasRole(currentUser.roles, role))) {
        return true;
      }

      return this.router.createUrlTree(['/home']);
    }

    if (this.auth.hasToken()) {
      return this.router.createUrlTree(['/session-expired']);
    }

    return this.router.createUrlTree(['/login']);
  }

  private getRequiredRoles(route: ActivatedRouteSnapshot): string[] {
    const roles = route.data?.['roles'];

    if (typeof roles === 'string' && roles.trim()) {
      return [roles];
    }

    if (Array.isArray(roles)) {
      return roles.filter((role): role is string => typeof role === 'string' && !!role.trim());
    }

    return [];
  }

  private hasRole(roles: readonly string[], role: string): boolean {
    return roles.some(value => value.toLowerCase() === role.toLowerCase());
  }
}
