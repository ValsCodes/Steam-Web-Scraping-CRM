import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

type FaqItem = {
  readonly question: string;
  readonly answer: string;
};

type FaqGroup = {
  readonly title: string;
  readonly items: readonly FaqItem[];
};

@Component({
  selector: 'steam-faq-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './faq-page.html',
  styleUrl: './faq-page.scss',
})
export class FaqPage {
  readonly groups: readonly FaqGroup[] = [
    {
      title: 'Platform Basics',
      items: [
        {
          question: 'What does Steam Web Scraping CRM do?',
          answer:
            'It helps teams configure Steam market sources, manage catalog data, run scraping workflows, monitor target items, and export results for analysis.',
        },
        {
          question: 'Who is the CRM built for?',
          answer:
            'It is built for operators, analysts, and administrators who need a repeatable workspace for Steam market intelligence instead of scattered manual checks.',
        },
      ],
    },
    {
      title: 'Catalog And Scraping',
      items: [
        {
          question: 'What catalog data can I manage?',
          answer:
            'You can manage games, game URLs, products, pixels, and tags, including the relationships that connect sources to monitored products and visual checks.',
        },
        {
          question: 'How do game URLs work?',
          answer:
            'Game URLs define source settings such as page ranges, source mode, linked products, linked pixels, and the details needed for a scraping scenario.',
        },
        {
          question: 'Which scraping modes are supported?',
          answer:
            'The app supports listing page workflows, alternate source workflows, and pixel-based validation scenarios for checking visual item attributes.',
        },
      ],
    },
    {
      title: 'Monitoring And Reporting',
      items: [
        {
          question: 'What is the difference between wish list and watch list monitoring?',
          answer:
            'Wish list items focus on target conditions such as price thresholds, while watch list items keep priority market targets visible for ongoing review.',
        },
        {
          question: 'Can I export data for offline analysis?',
          answer:
            'Yes. Key table workflows are designed around filtering first, then exporting the current slice to Excel for review, reporting, or deeper analysis.',
        },
      ],
    },
    {
      title: 'Accounts, Pricing, And Links',
      items: [
        {
          question: 'Do I need an account to use the workspace?',
          answer:
            'Yes. Public pages explain the product, but catalog, scraping, monitoring, and profile workflows are protected behind login.',
        },
        {
          question: 'Is there a free tier?',
          answer:
            'The pricing page presents a Free Tier for trying the core workflow, plus paid tiers for higher catalog, scraping, monitoring, and automation needs.',
        },
        {
          question: 'Does the app open external Steam or third-party links?',
          answer:
            'Some workflows can open third-party destinations such as Steam Community links. The app includes an external link disclosure so users can review responsibility before opening those links.',
        },
      ],
    },
  ];
}
