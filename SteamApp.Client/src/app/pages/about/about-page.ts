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
      title: 'Steam market sources',
      description:
        'Configure game URLs, paging, source modes, and pixel validation inputs from one workspace.',
    },
    {
      title: 'Catalog metadata',
      description:
        'Maintain games, products, tags, and visual signatures so scraping scenarios start with reliable context.',
    },
    {
      title: 'Monitoring targets',
      description:
        'Track wish list thresholds and watch list priorities without repeating the same manual checks every day.',
    },
  ];

  readonly audiences: readonly AboutAudience[] = [
    {
      role: 'Operators',
      summary:
        'Run scraping workflows, review filtered results, and export the current working slice for analysis.',
    },
    {
      role: 'Analysts',
      summary:
        'Use structured catalog data and monitoring rules to understand market movement with less spreadsheet setup.',
    },
    {
      role: 'Administrators',
      summary:
        'Keep source configuration, account access, monitoring rules, and operational routines aligned.',
    },
  ];

  readonly capabilities: readonly string[] = [
    'Source configuration for listing page, alternate source, and pixel-based scraping scenarios.',
    'Relation-driven administration for games, URLs, products, pixels, and tags.',
    'Wish list and watch list workflows for price and target monitoring.',
    'Filter-first review screens with Excel export for offline reporting.',
  ];
}
