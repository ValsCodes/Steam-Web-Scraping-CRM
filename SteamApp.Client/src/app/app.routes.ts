import { Routes } from '@angular/router';

import { AuthGuard } from './services/auth/auth.guard';

const PUBLIC_ROBOTS = 'index,follow';
const LOGIN_ROBOTS = 'noindex,follow';
const PRIVATE_ROBOTS = 'noindex,nofollow';

const defaultPrivateDescription =
  'Authenticated SteamApp workspace for catalog context, market checks, monitoring, and workflow automation.';

const aboutPageStructuredData = {
  '@context': 'https://schema.org',
  '@type': 'AboutPage',
  name: 'About SteamApp',
  description:
    'About SteamApp, a market intelligence workspace for connected catalog context, reusable matching rules, live checks, and automated queues.',
  url: '/about',
  mainEntity: {
    '@type': 'WebApplication',
    name: 'SteamApp',
    applicationCategory: 'BusinessApplication',
    operatingSystem: 'Web',
  },
};

const faqPageStructuredData = {
  '@context': 'https://schema.org',
  '@type': 'FAQPage',
  name: 'SteamApp FAQ',
  url: '/faq',
  mainEntity: [
    {
      '@type': 'Question',
      name: 'What does SteamApp do?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'SteamApp connects Steam market catalog data, reusable matching rules, live checks, automated queues, monitoring targets, and run history.',
      },
    },
    {
      '@type': 'Question',
      name: 'Is SteamApp a traditional CRM?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'No. SteamApp is built for operators and analysts who need repeatable Steam market research, not customer-relationship or sales-pipeline management.',
      },
    },
    {
      '@type': 'Question',
      name: 'What is a manual check?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'A manual check evaluates Steam listings against reusable criteria and exposes live progress, outcome filters, run controls, and history.',
      },
    },
    {
      '@type': 'Question',
      name: 'What are presets and preset combinations?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'A preset stores one criteria expression. A preset combination applies two or more unique same-game presets with top-level AND or OR operators without changing the saved presets.',
      },
    },
    {
      '@type': 'Question',
      name: 'Can I filter results while a check is running?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'Yes. Live results can be filtered by Pending, Matched, No match, and Failed before the run finishes.',
      },
    },
    {
      '@type': 'Question',
      name: 'What does the Automated Queue do?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'It arranges saved presets, preset combinations, private check templates, and delays into an ordered recipe with block-level progress and frozen run history.',
      },
    },
    {
      '@type': 'Question',
      name: 'Do queue runs use the latest presets?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'A queue resolves its latest accessible preset versions when a run starts, then freezes that resolved setup for the run and its historical reruns.',
      },
    },
    {
      '@type': 'Question',
      name: 'What market context can I manage?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'You can organize games, Steam market URLs, products, tags, item groups, stock history, wish-list conditions, and watch-list targets.',
      },
    },
    {
      '@type': 'Question',
      name: 'Do I need an account to use the workspace?',
      acceptedAnswer: {
        '@type': 'Answer',
        text: 'Yes. Public pages explain the product, while catalog, checks, queues, monitoring, and profile workflows are protected behind sign-in.',
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
    title: 'SteamApp',
    data: {
      seo: {
        title: 'SteamApp',
        description:
          'Connect Steam market catalog context, reusable matching rules, live checks, and automated queues in one operational workspace.',
        canonicalPath: '/home',
        robots: PUBLIC_ROBOTS,
        imagePath: '/assets/brand/steam-app-social.svg',
        type: 'website',
        structuredData: {
          '@context': 'https://schema.org',
          '@type': 'WebApplication',
          name: 'SteamApp',
          applicationCategory: 'BusinessApplication',
          operatingSystem: 'Web',
          description:
            'A web application for Steam market intelligence, reusable matching rules, live checks, queue automation, and monitoring.',
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
          'Compare SteamApp plans for catalog context, market checks, monitoring, history, and workflow automation.',
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
          'Learn how SteamApp helps teams connect market context, matching rules, live checks, automated queues, and monitoring.',
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
          'Answers to common questions about SteamApp market context, matching rules, live checks, automated queues, accounts, and external links.',
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
        description: 'Sign in to the SteamApp workspace.',
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
          'External link responsibility disclosure for SteamApp users.',
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
        description: 'Your SteamApp session has expired.',
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
        path: 'manual-mode-v2',
        loadComponent: () =>
          import('./pages/manual-mode-v2/manual-mode-v2').then((m) => m.ManualModeV2),
        title: 'Manual Mode',
        data: { seo: { title: 'Manual Mode', canonicalPath: '/manual-mode-v2' } },
      },
      {
        path: 'automatic-queue',
        loadComponent: () =>
          import('./pages/automatic-queue/automatic-queue-page').then((m) => m.AutomaticQueuePage),
        title: 'Automatic Queue',
        data: { seo: { title: 'Automatic Queue', canonicalPath: '/automatic-queue' } },
      },
      {
        path: 'profile',
        loadComponent: () => import('./pages/profile/profile-page').then((m) => m.ProfilePage),
        title: 'Profile',
        data: { seo: { title: 'Profile', canonicalPath: '/profile' } },
      },
      {
        path: 'admin/users',
        loadComponent: () =>
          import('./pages/admin/users/admin-users-page').then((m) => m.AdminUsersPage),
        title: 'Users',
        data: {
          roles: ['Admin'],
          seo: { title: 'Users', canonicalPath: '/admin/users' },
        },
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
        description: 'The requested SteamApp page could not be found.',
        canonicalPath: '/404',
        robots: PRIVATE_ROBOTS,
      },
    },
  },
];
