import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

type PricingAccent = 'free' | 'tier1' | 'tier2';

interface PricingPlan {
  readonly name: string;
  readonly description: string;
  readonly benefits: readonly string[];
  readonly cta: string;
  readonly accent: PricingAccent;
  readonly recommended?: boolean;
}

@Component({
  selector: 'steam-pricing-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './pricing-page.html',
  styleUrl: './pricing-page.scss',
})
export class PricingPage {
  readonly plans: readonly PricingPlan[] = [
    {
      name: 'Free Tier',
      description: 'For trying the workflow and managing a small Steam market setup.',
      benefits: [
        'Core catalog setup for a focused game and product set.',
        'Basic game, source, product, tag, and item-group organization.',
        'Manual checks for exploring reusable matching criteria.',
        'Limited wish and watch tracking for priority market targets.',
        'Basic filters and Excel exports for review.',
      ],
      cta: 'Start Free',
      accent: 'free',
    },
    {
      name: 'Tier 1',
      description: 'For active operators running regular market tracking and analysis.',
      benefits: [
        'Expanded catalog coverage across active games, URLs, and products.',
        'Saved presets and preset combinations for repeatable market checks.',
        'Live outcome filters plus pause, continue, cancel, and rerun controls.',
        'Excel export for filtered operational slices.',
        'Standard worker-backed wish and watch checks.',
      ],
      cta: 'Choose Tier 1',
      accent: 'tier1',
      recommended: true,
    },
    {
      name: 'Tier 2',
      description: 'For advanced monitoring, higher automation, and heavier operational usage.',
      benefits: [
        'Highest catalog and monitoring capacity for larger market operations.',
        'Automated queues with preset, combination, private-template, and delay blocks.',
        'Frozen queue-run snapshots for operational review and auditability.',
        'Faster worker-backed checks for wish and watch scenarios.',
        'Priority support and deployment assistance.',
      ],
      cta: 'Request Tier 2',
      accent: 'tier2',
    },
  ];
}
