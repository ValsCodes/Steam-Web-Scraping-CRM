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

type ArchitectureCard = {
  title: string;
  subtitle: string;
  description: string;
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
      title: 'Web Scraper',
      route: '/web-scraper',
      category: 'Scraping and Analysis',
      description:
        'Run listing and market scraping workflows, review outputs, and move quickly from source configuration to execution.',
      cta: 'Open Scraper',
      accent: 'scrape',
    },
    {
      title: 'Wish List Monitoring',
      route: '/wishlist',
      category: 'Operational Monitoring',
      description:
        'Track target price conditions, run checks, and manage the worker-driven monitoring scenarios described in the platform docs.',
      cta: 'Review Wish List',
      accent: 'ops',
    },
    {
      title: 'Catalog Workspace',
      route: '/games',
      category: 'Catalog and Configuration',
      description:
        'Maintain the core metadata model for games, game URLs, products, pixels, and tags from a single administrative surface.',
      cta: 'Manage Catalog',
      accent: 'catalog',
    },
  ];

  readonly workspaces: readonly HomeSectionCard[] = [
    {
      title: 'Games',
      route: '/games',
      category: 'Catalog',
      description: 'Create the main game contexts that anchor URLs, products, pixels, and tags.',
      cta: 'Open Games',
      accent: 'catalog',
    },
    {
      title: 'Game URLs',
      route: '/game-urls',
      category: 'Configuration',
      description: 'Control paging, public API mode, and pixel-based scraping settings for each market source.',
      cta: 'Open Game URLs',
      accent: 'catalog',
    },
    {
      title: 'Products',
      route: '/products',
      category: 'Catalog',
      description: 'Maintain monitored items, ratings, tags, and active state for operational filtering.',
      cta: 'Open Products',
      accent: 'catalog',
    },
    {
      title: 'Tags and Pixels',
      route: '/tags',
      category: 'Relations',
      description: 'Use taxonomy and color signatures to enrich categorization and pixel validation workflows.',
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
      title: 'Manual Mode',
      route: '/manual-mode-v2',
      category: 'Scraping',
      description: 'Handle more hands-on scenarios when you need operator control around scraping steps.',
      cta: 'Open Manual Mode',
      accent: 'scrape',
    },
    {
      title: 'Pixels',
      route: '/pixels',
      category: 'Validation',
      description: 'Review RGB signatures used to validate paint and visual attributes during scraping.',
      cta: 'Open Pixels',
      accent: 'scrape',
    },
  ];

  readonly workflows: readonly Workflow[] = [
    {
      title: 'Onboard a New Game',
      summary: 'Start with the core data model, then connect the supporting entities around it.',
      steps: [
        'Create a game entry as the main context.',
        'Add a Game URL with paging, API, and pixel settings.',
        'Create products and pixels for that game.',
        'Link Game URL to products and pixels, then apply tags.',
      ],
    },
    {
      title: 'Configure a Scraping Scenario',
      summary: 'Move from source selection to execution and export with a filter-first workflow.',
      steps: [
        'Select the game and source configuration.',
        'Run scraping with page or public API mode.',
        'Review the resulting data in table views.',
        'Export the current slice to Excel for analysis.',
      ],
    },
    {
      title: 'Maintain Monitoring',
      summary: 'Keep wish and watch conditions current so the platform can reduce repetitive manual checks.',
      steps: [
        'Create or adjust wish list thresholds.',
        'Update watch list targets as market focus changes.',
        'Run checks, review results, and tune thresholds.',
        'Use the worker-backed flow for periodic monitoring.',
      ],
    },
  ];

  readonly architectureCards: readonly ArchitectureCard[] = [
    {
      title: 'Angular Client',
      subtitle: 'Route-level workspaces and reusable UI',
      description:
        'The frontend organizes operations into pages, services, models, and components so operators can move quickly between catalog, monitoring, and scraping screens.',
    },
    {
      title: '.NET Web API',
      subtitle: 'Protected application and scraping endpoints',
      description:
        'The backend provides CRUD APIs, relation endpoints, JWT-protected routes, and Steam scraping workflows behind a single operational layer.',
    },
    {
      title: 'SQL Server and Workers',
      subtitle: 'Persistence with background automation',
      description:
        'EF Core stores the domain model, while hosted workers support recurring wish list checks and other operational automation.',
    },
  ];
}
