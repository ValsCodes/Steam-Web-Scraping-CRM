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
          question: 'What does SteamApp do?',
          answer:
            'SteamApp is a Steam market intelligence and workflow automation platform. It connects catalog data, reusable matching rules, live checks, automated queues, monitoring targets, and run history.',
        },
        {
          question: 'Is SteamApp a traditional CRM?',
          answer:
            'No. It does not manage customer relationships or a sales pipeline. It is built for operators and analysts who need repeatable Steam market research instead of scattered links, spreadsheets, and one-off checks.',
        },
      ],
    },
    {
      title: 'Checks And Matching Rules',
      items: [
        {
          question: 'What is a manual check?',
          answer:
            'A manual check evaluates Steam listings from a selected game source against reusable criteria. You can follow progress live, filter outcomes, pause or continue eligible runs, and revisit the saved setup later.',
        },
        {
          question: 'What are presets and preset combinations?',
          answer:
            'A preset stores one criteria expression. A preset combination applies two or more unique presets for the same game with top-level AND or OR operators while leaving the saved presets unchanged.',
        },
        {
          question: 'Can I filter results while a check is running?',
          answer:
            'Yes. Live results can be filtered by Pending, Matched, No match, and Failed without waiting for the whole run to finish.',
        },
      ],
    },
    {
      title: 'Automation And History',
      items: [
        {
          question: 'What does the Automated Queue do?',
          answer:
            'It turns saved presets, preset combinations, private check templates, and delays into an ordered recipe. Each run records its resolved setup so its results remain understandable later.',
        },
        {
          question: 'Do queue runs use the latest presets?',
          answer:
            'Queue definitions retain references to their presets and resolve the latest accessible versions when a new run starts. The resolved setup is then frozen for that run and historical reruns.',
        },
      ],
    },
    {
      title: 'Catalog, Accounts, And Links',
      items: [
        {
          question: 'What market context can I manage?',
          answer:
            'You can organize games, Steam market URLs, products, tags, item groups, stock history, wish-list conditions, and watch-list targets, then use that context in checks and reporting.',
        },
        {
          question: 'Do I need an account to use the workspace?',
          answer:
            'Yes. Public pages explain the product, while catalog, checks, queues, monitoring, and profile workflows are protected behind sign-in.',
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
