import { Routes } from '@angular/router';
import {
  GameForm,
  GamesView,
  GameUrlForm,
  GameUrlsView,
  LoginComponent,
  ManualModeV2,
  PixelForm,
  PixelsView,
  ProductForm,
  ProductsView,
  TagForm,
  TagsView,
  WatchListForm,
  WatchListsView,
  WebScraperComponent,
  WishListForm,
  WishListsView
} from './pages';
import { AuthGuard } from './services/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent,
    title: 'Login',
  },
  {
    path: '',
    canActivateChild: [AuthGuard],
    children: [
      { path: '', redirectTo: 'web-scraper', pathMatch: 'full' },

      { path: 'web-scraper', component: WebScraperComponent, title: 'Web Scraper' },
      { path: 'manual-mode-v2', component: ManualModeV2, title: 'Manual Mode' },

      { path: 'games', component: GamesView, title: 'Games' },
      { path: 'games/create', component: GameForm, title: 'Create Game' },
      { path: 'games/edit/:id', component: GameForm, title: 'Edit Game' },

      { path: 'game-urls', component: GameUrlsView, title: 'Game URLs' },
      { path: 'game-urls/create', component: GameUrlForm, title: 'Create Game URL' },
      { path: 'game-urls/edit/:id', component: GameUrlForm, title: 'Edit Game URL' },

      { path: 'products', component: ProductsView, title: 'Products' },
      { path: 'products/create', component: ProductForm, title: 'Create Product' },
      { path: 'products/edit/:id', component: ProductForm, title: 'Edit Product' },

      { path: 'pixels', component: PixelsView, title: 'Pixels' },
      { path: 'pixels/create', component: PixelForm, title: 'Create Pixel' },
      { path: 'pixels/edit/:id', component: PixelForm, title: 'Edit Pixel' },

      { path: 'wishlist', component: WishListsView, title: 'Wish List' },
      { path: 'wishlist/create', component: WishListForm, title: 'Create Wish List Item' },
      { path: 'wishlist/edit/:id', component: WishListForm, title: 'Edit Wish List Item' },

      { path: 'watch-list', component: WatchListsView, title: 'Watch List' },
      { path: 'watch-list/create', component: WatchListForm, title: 'Create Watch List Item' },
      { path: 'watch-list/edit/:id', component: WatchListForm, title: 'Edit Watch List Item' },
      { path: 'tags', component: TagsView, title: 'Tags' },
      { path: 'tags/create', component: TagForm, title: 'Create Tag' },
      { path: 'tags/edit/:id', component: TagForm, title: 'Edit Tag' },
    ],
  },
];
