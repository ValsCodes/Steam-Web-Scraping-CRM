import { Routes } from '@angular/router';
import { GameForm, GamesView, GameUrlForm, GameUrlProductForm, GameUrlProductsView, GameUrlsView, LoginComponent, ManualModeComponent, ManualModeV2, PixelForm, PixelsView, ProductForm, ProductsView, WatchListForm, WatchListsView, WebScraperComponent, WishListForm, WishListsView } from './pages';
import { AuthGuard } from './services/auth/auth.guard';


export const routes: Routes = [
  {
    path: 'web-scraper',
    component: WebScraperComponent,
    title: 'Web Scraper',
    canActivate: [AuthGuard],
  },
  {
    path: 'manual-mode',
    component: ManualModeComponent,
    title: 'Manual Mode',
    canActivate: [AuthGuard],
  },
  { path: 'login', component: LoginComponent, title: 'Login' },
  { path: 'games', component: GamesView, title: 'Games' },
  { path: 'games/create', component: GameForm },
  { path: 'games/edit/:id', component: GameForm },
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'game-urls',
    component: GameUrlsView,
  },
  {
    path: 'game-urls/create',
    component: GameUrlForm,
  },
  {
    path: 'game-urls/edit/:id',
    component: GameUrlForm,
  },
  {
    path: 'products',
    component: ProductsView,
  },
  {
    path: 'products/create',
    component: ProductForm,
  },
  {
    path: 'products/edit/:id',
    component: ProductForm,
  },
  {
    path: 'pixels',
    component: PixelsView,
  },
  {
    path: 'pixels/create',
    component: PixelForm,
  },
  {
    path: 'pixels/edit/:id',
    component: PixelForm,
  },
  {
    path: 'wishlist',
    component: WishListsView,
  },
  {
    path: 'wishlist/create',
    component: WishListForm,
  },
  {
    path: 'wishlist/edit/:id',
    component: WishListForm,
  },
  {
    path: 'watch-list',
    component: WatchListsView,
  },
  {
    path: 'watch-list/create',
    component: WatchListForm,
  },
  {
    path: 'watch-list/edit/:id',
    component: WatchListForm,
  },
  {
    path: 'game-url-products',
    component: GameUrlProductsView,
  },
  {
    path: 'game-url-products/create',
    component: GameUrlProductForm,
  },
  {
    path: 'game-url-products/edit/:productId/:gameUrlId',
    component: GameUrlProductForm,
  } ,
  {
    path: 'manual-mode-v2',
    component: ManualModeV2,
  }
];
