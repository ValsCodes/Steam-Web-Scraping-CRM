import { Routes } from '@angular/router';
import { WebScraperComponent } from './pages/web-scraper/web-scraper.component';
import { ManualModeComponent } from './pages/manual-mode/manual-mode.component';
import { LoginComponent } from './pages/login/login.component';
import { AuthGuard } from './services/auth/auth.guard';
import { GamesView } from './pages/game/games-view/games-view';
import { GameForm } from './pages/game/game-form/game-form';
import { GameUrlForm } from './pages/game-url/game-url-form/game-url-form';
import { GameUrlsView } from './pages/game-url/game-urls-view/game-urls-view';
import { ProductForm } from './pages/product/product-form/product-form';
import { ProductsView } from './pages/product/products-view/products-view';
import { PixelForm } from './pages/pixel/pixel-form/pixel-form';
import { PixelsView } from './pages/pixel/pixels-view/pixels-view';
import { WishListForm } from './pages/wish-list/wish-list-form/wish-list-form';
import { WishListsView } from './pages/wish-list/wish-lists-view/wish-lists-view';
import { WatchListForm } from './pages/watch-list/watch-list-form/watch-list-form';
import { WatchListsView } from './pages/watch-list/watch-lists-view/watch-lists-view';

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
];
