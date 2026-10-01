import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

type HomeSectionCard = {
  title: string;
  route: string;
  category: string;
  description: string;
  cta: string;
  accent: 'catalog' | 'ops' | 'scrape';
};

type Workflow = {
  title: string;
  summary: string;
  steps: readonly string[];
};

@Component({
  selector: 'steam-home-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home-page.html',
  styleUrl: './home-page.scss',
})
export class HomePage {
  readonly spotlightCards: readonly HomeSectionCard[] = [
    {
      title: 'Manual Checks',
      route: '/manual-mode-v2',
      category: 'Rules and Analysis',
      description:
        'Run reusable criteria against Steam listings, combine presets, and review outcomes while the check is still running.',
      cta: 'Run a Check',
      accent: 'scrape',
    },
    {
      title: 'Automated Queue',
      route: '/automatic-queue',
      category: 'Workflow Automation',
      description:
        'Arrange checks and delays into repeatable queues, then follow each block through a frozen run history.',
      cta: 'Open Queue',
      accent: 'ops',
    },
    {
      title: 'Catalog and Stock',
      route: '/products',
      category: 'Market Context',
      description:
        'Keep games, market sources, products, tags, item groups, and stock history connected to the rules that use them.',
      cta: 'Open Products',
      accent: 'catalog',
    },
  ];

  readonly workspaces: readonly HomeSectionCard[] = [
    {
      title: 'Games',
      route: '/games',
      category: 'Catalog',
      description: 'Create the game contexts that anchor market sources, products, tags, and checks.',
      cta: 'Open Games',
      accent: 'catalog',
    },
    {
      title: 'Game URLs',
      route: '/game-urls',
      category: 'Configuration',
      description: 'Maintain the Steam listing sources used by catalog records and market checks.',
      cta: 'Open Game URLs',
      accent: 'catalog',
    },
    {
      title: 'Products',
      route: '/products',
      category: 'Catalog',
      description: 'Maintain tracked items, ratings, tags, active state, and current-stock history.',
      cta: 'Open Products',
      accent: 'catalog',
    },
    {
      title: 'Tags and Item Groups',
      route: '/tags',
      category: 'Relations',
      description: 'Build reusable taxonomy and item collections for catalog filtering and matching criteria.',
      cta: 'Open Tags',
      accent: 'catalog',
    },
    {
      title: 'Wish List',
      route: '/wishlist',
      category: 'Monitoring',
      description: 'Manage target thresholds for automated checking and operator-driven validation.',
      cta: 'Open Wish List',
      accent: 'ops',
    },
    {
      title: 'Watch List',
      route: '/watch-list',
      category: 'Monitoring',
      description: 'Keep active watch targets visible so operators can maintain market tracking coverage.',
      cta: 'Open Watch List',
      accent: 'ops',
    },
    {
      title: 'Manual Checks',
      route: '/manual-mode-v2',
      category: 'Analysis',
      description: 'Run presets or preset combinations with live progress, outcome filters, pause, and rerun controls.',
      cta: 'Open Manual Checks',
      accent: 'scrape',
    },
    {
      title: 'Automated Queue',
      route: '/automatic-queue',
      category: 'Automation',
      description: 'Build ordered check-and-delay recipes and inspect their execution history.',
      cta: 'Open Automated Queue',
      accent: 'ops',
    },
  ];

  readonly workflows: readonly Workflow[] = [
    {
      title: 'Build Market Context',
      summary: 'Create the catalog foundation that gives every rule and result a stable context.',
      steps: [
        'Create a game entry as the main context.',
        'Add the Steam market URL used for listing checks.',
        'Create products and connect their tags and item groups.',
        'Maintain current stock and monitoring targets as the catalog evolves.',
      ],
    },
    {
      title: 'Run Repeatable Listing Analysis',
      summary: 'Turn a matching idea into a reusable check with observable results.',
      steps: [
        'Create criteria and save them as a preset.',
        'Run one preset or combine multiple same-game presets.',
        'Filter live results by pending, matched, no match, or failed.',
        'Review the frozen setup later or rerun it from history.',
      ],
    },
    {
      title: 'Automate a Check Sequence',
      summary: 'Compose repeatable operations without losing the detail of each individual check.',
      steps: [
        'Create an automated queue for a game and source.',
        'Add preset, preset-combination, private-template, or delay blocks.',
        'Start the queue and follow progress block by block.',
        'Use run snapshots to understand exactly what was evaluated.',
      ],
    },
  ];
}
