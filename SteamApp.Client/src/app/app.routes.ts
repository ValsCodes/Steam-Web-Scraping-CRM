import { Routes } from '@angular/router';

import { AuthGuard } from './services/auth/auth.guard';

const PUBLIC_ROBOTS = 'index,follow';
const LOGIN_ROBOTS = 'noindex,follow';
const PRIVATE_ROBOTS = 'noindex,nofollow';

const defaultPrivateDescription =
  'Authenticated Steam Web Scraping CRM workspace for managing catalog, monitoring, and scraping operations.';

const aboutPageStructuredData = {
  '@context': 'https://schema.org',
  '@type': 'AboutPage',
  name: 'About Steam Web Scraping CRM',
  description:
    'About Steam Web Scraping CRM, a workspace for Steam market source configuration, catalog metadata, scraping workflows, monitoring, and exports.',
  url: '/about',
  mainEntity: {
    '@type': 'WebApplication',
    name: 'Steam Web Scraping CRM',
    applicationCategory: 'BusinessApplication',
    operatingSystem: 'Web',
  },
};

const faqPageStructuredData = {
  '@context': 'https://schema.org',
  '@type': 'FAQPage',
  name: 'Steam Web Scraping CRM FAQ',
  url: '/faq',
  mainEntity: [
    {
      '@type': 'Question',
      name: 'What does Steam Web Scraping CRM do?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'It helps teams configure Steam market sources, manage catalog data, run scraping workflows, monitor target items, and export results for analysis.',
      },
    },
    {
      '@type': 'Question',
      name: 'Who is the CRM built for?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'It is built for operators, analysts, and administrators who need a repeatable workspace for Steam market intelligence instead of scattered manual checks.',
      },
    },
    {
      '@type': 'Question',
      name: 'What catalog data can I manage?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'You can manage games, game URLs, products, pixels, and tags, including the relationships that connect sources to monitored products and visual checks.',
      },
    },
    {
      '@type': 'Question',
      name: 'How do game URLs work?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'Game URLs define source settings such as page ranges, source mode, linked products, linked pixels, and the details needed for a scraping scenario.',
      },
    },
    {
      '@type': 'Question',
      name: 'Which scraping modes are supported?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'The app supports listing page workflows, alternate source workflows, and pixel-based validation scenarios for checking visual item attributes.',
      },
    },
    {
      '@type': 'Question',
      name: 'What is the difference between wish list and watch list monitoring?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'Wish list items focus on target conditions such as price thresholds, while watch list items keep priority market targets visible for ongoing review.',
      },
    },
    {
      '@type': 'Question',
      name: 'Can I export data for offline analysis?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'Yes. Key table workflows are designed around filtering first, then exporting the current slice to Excel for review, reporting, or deeper analysis.',
      },
    },
    {
      '@type': 'Question',
      name: 'Do I need an account to use the workspace?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'Yes. Public pages explain the product, but catalog, scraping, monitoring, and profile workflows are protected behind login.',
      },
    },
    {
      '@type': 'Question',
      name: 'Is there a free tier?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'The pricing page presents a Free Tier for trying the core workflow, plus paid tiers for higher catalog, scraping, monitoring, and automation needs.',
      },
    },
    {
      '@type': 'Question',
      name: 'Does the app open external Steam or third-party links?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'Some workflows can open third-party destinations such as Steam Community links. The app includes an external link disclosure so users can review responsibility before opening those links.',
      },
    },
  ],
};

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full',
  },
  {
    path: 'home',
    loadComponent: () => import('./pages/home/home-page').then((m) => m.HomePage),
    title: 'Steam Web Scraping CRM',
    data: {
      seo: {
        title: 'Steam Web Scraping CRM',
        description:
          'Operate Steam market intelligence from one control surface for scraping, catalog metadata, monitoring, and operational workflows.',
        canonicalPath: '/home',
        robots: PUBLIC_ROBOTS,
        imagePath: '/assets/brand/steam-app-social.svg',
        type: 'website',
        structuredData: {
          '@context': 'https://schema.org',
          '@type': 'WebApplication',
          name: 'Steam Web Scraping CRM',
          applicationCategory: 'BusinessApplication',
          operatingSystem: 'Web',
          description:
            'A web application for Steam market source configuration, scraping workflow control, catalog metadata, and monitoring.',
          url: '/home',
        },
      },
    },
  },
  {
    path: 'pricing',
    loadComponent: () => import('./pages/pricing/pricing-page').then((m) => m.PricingPage),
    title: 'Subscription Pricing',
    data: {
      seo: {
        title: 'Subscription Pricing',
        description:
          'Compare Steam Web Scraping CRM plans for catalog setup, scraping workflows, monitoring, exports, and automation.',
        canonicalPath: '/pricing',
        robots: PUBLIC_ROBOTS,
        imagePath: '/assets/brand/steam-app-social.svg',
        type: 'website',
      },
    },
  },
  {
    path: 'about',
    loadComponent: () => import('./pages/about/about-page').then((m) => m.AboutPage),
    title: 'About',
    data: {
      seo: {
        title: 'About',
        description:
          'Learn how Steam Web Scraping CRM helps teams manage Steam market sources, catalog metadata, scraping workflows, monitoring, and exports.',
        canonicalPath: '/about',
        robots: PUBLIC_ROBOTS,
        imagePath: '/assets/brand/steam-app-social.svg',
        type: 'website',
        structuredData: aboutPageStructuredData,
      },
    },
  },
  {
    path: 'faq',
    loadComponent: () => import('./pages/faq/faq-page').then((m) => m.FaqPage),
    title: 'FAQ',
    data: {
      seo: {
        title: 'FAQ',
        description:
          'Answers to common questions about Steam Web Scraping CRM catalog setup, scraping workflows, monitoring, exports, accounts, pricing, and external links.',
        canonicalPath: '/faq',
        robots: PUBLIC_ROBOTS,
        imagePath: '/assets/brand/steam-app-social.svg',
        type: 'website',
        structuredData: faqPageStructuredData,
      },
    },
  },
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then((m) => m.LoginComponent),
    title: 'Login',
    data: {
      seo: {
        title: 'Login',
        description: 'Sign in to the Steam Web Scraping CRM workspace.',
        canonicalPath: '/login',
        robots: LOGIN_ROBOTS,
      },
    },
  },
  {
    path: 'external-link-disclosure',
    loadComponent: () =>
      import('./pages/external-link-disclosure/external-link-disclosure-page').then(
        (m) => m.ExternalLinkDisclosurePage,
      ),
    title: 'External Link Disclosure',
    data: {
      seo: {
        title: 'External Link Disclosure',
        description:
          'External link responsibility disclosure for Steam Web Scraping CRM users.',
        canonicalPath: '/external-link-disclosure',
        robots: LOGIN_ROBOTS,
      },
    },
  },
  {
    path: 'session-expired',
    loadComponent: () =>
      import('./pages/session-expired/session-expired-page').then((m) => m.SessionExpiredPage),
    title: 'Session Expired',
    data: {
      seo: {
        title: 'Session Expired',
        description: 'Your Steam Web Scraping CRM session has expired.',
        canonicalPath: '/session-expired',
        robots: PRIVATE_ROBOTS,
      },
    },
  },
  {
    path: '',
    canActivateChild: [AuthGuard],
    data: {
      seo: {
        description: defaultPrivateDescription,
        robots: PRIVATE_ROBOTS,
      },
    },
    children: [
      {
        path: 'web-scraper',
        loadComponent: () =>
          import('./pages/web-scraper/web-scraper.component').then((m) => m.WebScraperComponent),
        title: 'Web Scraper',
        data: { seo: { title: 'Web Scraper', canonicalPath: '/web-scraper' } },
      },
      {
        path: 'manual-mode-v2',
        loadComponent: () =>
          import('./pages/manual-mode-v2/manual-mode-v2').then((m) => m.ManualModeV2),
        title: 'Manual Mode',
        data: { seo: { title: 'Manual Mode', canonicalPath: '/manual-mode-v2' } },
      },
      {
        path: 'profile',
        loadComponent: () => import('./pages/profile/profile-page').then((m) => m.ProfilePage),
        title: 'Profile',
        data: { seo: { title: 'Profile', canonicalPath: '/profile' } },
      },
      {
        path: 'feedback',
        loadComponent: () =>
          import('./pages/feedback/feedback-requests-view/feedback-requests-view').then(
            (m) => m.FeedbackRequestsView,
          ),
        title: 'Feedback Requests',
        data: { seo: { title: 'Feedback Requests', canonicalPath: '/feedback' } },
      },
      {
        path: 'feedback/create',
        loadComponent: () =>
          import('./pages/feedback/feedback-request-form/feedback-request-form').then(
            (m) => m.FeedbackRequestForm,
          ),
        title: 'Create Feedback Request',
        data: { seo: { title: 'Create Feedback Request', canonicalPath: '/feedback/create' } },
      },
      {
        path: 'feedback/edit/:id',
        loadComponent: () =>
          import('./pages/feedback/feedback-request-form/feedback-request-form').then(
            (m) => m.FeedbackRequestForm,
          ),
        title: 'Edit Feedback Request',
        data: { seo: { title: 'Edit Feedback Request', canonicalPath: '/feedback/edit' } },
      },
      {
        path: 'feedback/send',
        loadComponent: () =>
          import('./pages/feedback/feedback-send-page/feedback-send-page').then(
            (m) => m.FeedbackSendPage,
          ),
        title: 'Send Feedback',
        data: { seo: { title: 'Send Feedback', canonicalPath: '/feedback/send' } },
      },
      {
        path: 'games',
        loadComponent: () => import('./pages/game/games-view/games-view').then((m) => m.GamesView),
        title: 'Games',
        data: { seo: { title: 'Games', canonicalPath: '/games' } },
      },
      {
        path: 'games/create',
        loadComponent: () => import('./pages/game/game-form/game-form').then((m) => m.GameForm),
        title: 'Create Game',
        data: { seo: { title: 'Create Game', canonicalPath: '/games/create' } },
      },
      {
        path: 'games/edit/:id',
        loadComponent: () => import('./pages/game/game-form/game-form').then((m) => m.GameForm),
        title: 'Edit Game',
        data: { seo: { title: 'Edit Game', canonicalPath: '/games/edit' } },
      },
      {
        path: 'game-urls',
        loadComponent: () =>
          import('./pages/game-url/game-urls-view/game-urls-view').then((m) => m.GameUrlsView),
        title: 'Game URLs',
        data: { seo: { title: 'Game URLs', canonicalPath: '/game-urls' } },
      },
      {
        path: 'game-urls/create',
        loadComponent: () =>
          import('./pages/game-url/game-url-form/game-url-form').then((m) => m.GameUrlForm),
        title: 'Create Game URL',
        data: { seo: { title: 'Create Game URL', canonicalPath: '/game-urls/create' } },
      },
      {
        path: 'game-urls/edit/:id',
        loadComponent: () =>
          import('./pages/game-url/game-url-form/game-url-form').then((m) => m.GameUrlForm),
        title: 'Edit Game URL',
        data: { seo: { title: 'Edit Game URL', canonicalPath: '/game-urls/edit' } },
      },
      {
        path: 'products',
        loadComponent: () =>
          import('./pages/product/products-view/products-view').then((m) => m.ProductsView),
        title: 'Products',
        data: { seo: { title: 'Products', canonicalPath: '/products' } },
      },
      {
        path: 'products/create',
        loadComponent: () =>
          import('./pages/product/product-form/product-form').then((m) => m.ProductForm),
        title: 'Create Product',
        data: { seo: { title: 'Create Product', canonicalPath: '/products/create' } },
      },
      {
        path: 'products/edit/:id',
        loadComponent: () =>
          import('./pages/product/product-form/product-form').then((m) => m.ProductForm),
        title: 'Edit Product',
        data: { seo: { title: 'Edit Product', canonicalPath: '/products/edit' } },
      },
      {
        path: 'pixels',
        loadComponent: () =>
          import('./pages/pixel/pixels-view/pixels-view').then((m) => m.PixelsView),
        title: 'Pixels',
        data: { seo: { title: 'Pixels', canonicalPath: '/pixels' } },
      },
      {
        path: 'pixels/create',
        loadComponent: () => import('./pages/pixel/pixel-form/pixel-form').then((m) => m.PixelForm),
        title: 'Create Pixel',
        data: { seo: { title: 'Create Pixel', canonicalPath: '/pixels/create' } },
      },
      {
        path: 'pixels/edit/:id',
        loadComponent: () => import('./pages/pixel/pixel-form/pixel-form').then((m) => m.PixelForm),
        title: 'Edit Pixel',
        data: { seo: { title: 'Edit Pixel', canonicalPath: '/pixels/edit' } },
      },
      {
        path: 'wishlist',
        loadComponent: () =>
          import('./pages/wish-list/wish-lists-view/wish-lists-view').then((m) => m.WishListsView),
        title: 'Wish List',
        data: { seo: { title: 'Wish List', canonicalPath: '/wishlist' } },
      },
      {
        path: 'wishlist/create',
        loadComponent: () =>
          import('./pages/wish-list/wish-list-form/wish-list-form').then((m) => m.WishListForm),
        title: 'Create Wish List Item',
        data: { seo: { title: 'Create Wish List Item', canonicalPath: '/wishlist/create' } },
      },
      {
        path: 'wishlist/edit/:id',
        loadComponent: () =>
          import('./pages/wish-list/wish-list-form/wish-list-form').then((m) => m.WishListForm),
        title: 'Edit Wish List Item',
        data: { seo: { title: 'Edit Wish List Item', canonicalPath: '/wishlist/edit' } },
      },
      {
        path: 'watch-list',
        loadComponent: () =>
          import('./pages/watch-list/watch-lists-view/watch-lists-view').then((m) => m.WatchListsView),
        title: 'Watch List',
        data: { seo: { title: 'Watch List', canonicalPath: '/watch-list' } },
      },
      {
        path: 'watch-list/create',
        loadComponent: () =>
          import('./pages/watch-list/watch-list-form/watch-list-form').then((m) => m.WatchListForm),
        title: 'Create Watch List Item',
        data: { seo: { title: 'Create Watch List Item', canonicalPath: '/watch-list/create' } },
      },
      {
        path: 'watch-list/edit/:id',
        loadComponent: () =>
          import('./pages/watch-list/watch-list-form/watch-list-form').then((m) => m.WatchListForm),
        title: 'Edit Watch List Item',
        data: { seo: { title: 'Edit Watch List Item', canonicalPath: '/watch-list/edit' } },
      },
      {
        path: 'tags',
        loadComponent: () => import('./pages/tag/tags-view/tags-view').then((m) => m.TagsView),
        title: 'Tags',
        data: { seo: { title: 'Tags', canonicalPath: '/tags' } },
      },
      {
        path: 'tags/create',
        loadComponent: () => import('./pages/tag/tag-form/tag-form').then((m) => m.TagForm),
        title: 'Create Tag',
        data: { seo: { title: 'Create Tag', canonicalPath: '/tags/create' } },
      },
      {
        path: 'tags/edit/:id',
        loadComponent: () => import('./pages/tag/tag-form/tag-form').then((m) => m.TagForm),
        title: 'Edit Tag',
        data: { seo: { title: 'Edit Tag', canonicalPath: '/tags/edit' } },
      },
    ],
  },
  {
    path: '**',
    loadComponent: () =>
      import('./pages/not-found/not-found-page').then((m) => m.NotFoundPage),
    title: 'Page Not Found',
    data: {
      seo: {
        title: 'Page Not Found',
        description: 'The requested Steam Web Scraping CRM page could not be found.',
        canonicalPath: '/404',
        robots: PRIVATE_ROBOTS,
      },
    },
  },
];
