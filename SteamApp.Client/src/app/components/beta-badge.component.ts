import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-beta-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="beta-badge" title="This feature is still in progress">Beta</div>`,
  styles: `
    :host {
      display: inline-flex;
    }

    .beta-badge {
      padding: 0.2rem 0.6rem;
      border-radius: 0.5rem;
      background-color: #dbeafe;
      color: #1e40af;
      font-size: 0.75rem;
      font-weight: 600;
      line-height: 1.5;
    }
  `,
})
export class BetaBadgeComponent {}
