import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';

import { AuthService, CurrentUser } from '../../services/auth/auth.service';
import { DropdownComponent } from '../dropdown/dropdown.component';

@Component({
  selector: 'steam-site-header',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    // DropdownComponent,
  ],
  templateUrl: './site-header.component.html',
  styleUrl: './site-header.component.scss',
})
export class SiteHeaderComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  isLoggedIn = false;
  currentUser: CurrentUser | null = null;

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.auth.loggedIn$
      .pipe(takeUntil(this.destroy$))
      .subscribe((loggedIn) => {
        this.isLoggedIn = loggedIn;
      });

    this.auth.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe((user) => {
        this.currentUser = user;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  logout(event: MouseEvent): void {
    event.preventDefault();
    this.auth.logout();
    this.router.navigate(['login']);
  }
}
