import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

import { externalUrlWarning, openableExternalUrl } from '../../common';
import { ExternalLinkDisclosureService } from '../../services/external-link-disclosure.service';

@Component({
  selector: 'steam-external-link-disclosure-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './external-link-disclosure-page.html',
  styleUrl: './external-link-disclosure-page.scss',
})
export class ExternalLinkDisclosurePage implements OnInit {
  readonly acceptedControl = new FormControl(false, { nonNullable: true });

  targetUrl: string | null = null;
  openableTargetUrl: string | null = null;
  targetWarning: string | null = null;
  returnTo = '/home';
  acceptanceError: string | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly disclosure: ExternalLinkDisclosureService,
  ) {}

  ngOnInit(): void {
    this.targetUrl = this.route.snapshot.queryParamMap.get('target');
    this.openableTargetUrl = openableExternalUrl(this.targetUrl);
    this.targetWarning = externalUrlWarning(this.targetUrl);
    this.returnTo = this.disclosure.normalizeReturnUrl(
      this.route.snapshot.queryParamMap.get('returnTo'),
    );
  }

  acceptAndOpenTarget(): void {
    if (!this.openableTargetUrl || !this.accept()) {
      return;
    }

    window.open(this.openableTargetUrl, '_blank', 'noopener,noreferrer');
    void this.router.navigateByUrl(this.returnTo);
  }

  acceptWithoutOpening(): void {
    if (!this.accept()) {
      return;
    }

    void this.router.navigateByUrl(this.returnTo);
  }

  cancel(): void {
    void this.router.navigateByUrl(this.returnTo);
  }

  private accept(): boolean {
    if (!this.acceptedControl.value) {
      this.acceptanceError =
        'Confirm that you accept responsibility for reviewing external links before opening them.';
      return false;
    }

    this.acceptanceError = null;
    this.disclosure.accept();
    return true;
  }
}
