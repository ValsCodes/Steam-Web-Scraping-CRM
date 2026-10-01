import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

type AboutHighlight = {
  readonly title: string;
  readonly description: string;
};

type AboutAudience = {
  readonly role: string;
  readonly summary: string;
};

@Component({
  selector: 'steam-about-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './about-page.html',
  styleUrl: './about-page.scss',
})
export class AboutPage {
  readonly managementAreas: readonly AboutHighlight[] = [
    {
      title: 'Connected market context',
      description:
        'Keep games, Steam market URLs, products, tags, item groups, and stock history in one operational model.',
    },
    {
      title: 'Reusable matching rules',
      description:
        'Capture criteria as presets and combine same-game presets with clear top-level AND or OR logic.',
    },
    {
      title: 'Observable automation',
      description:
        'Run checks directly or arrange them into queues with live outcomes, controls, and frozen history.',
    },
  ];

  readonly audiences: readonly AboutAudience[] = [
    {
      role: 'Operators',
      summary:
        'Run checks, filter live outcomes, manage queues, and keep repeatable workflows moving.',
    },
    {
      role: 'Analysts',
      summary:
        'Use structured catalog context, explicit criteria, and historical snapshots to explain market results.',
    },
    {
      role: 'Administrators',
      summary:
        'Keep account access, market sources, shared catalog data, and operational routines aligned.',
    },
  ];

  readonly capabilities: readonly string[] = [
    'Manual checks with reusable criteria, presets, preset combinations, and product selection.',
    'Automated queues with ordered check and delay blocks plus frozen run snapshots.',
    'Relation-driven catalog management for games, URLs, products, tags, item groups, and stock.',
    'Wish-list and watch-list workflows for price conditions and priority market targets.',
  ];
}
