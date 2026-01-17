import { Routes } from '@angular/router';
import { WebScraperComponent } from "./pages/web-scraper/web-scraper.component";
import { ManualModeComponent } from "./pages/manual-mode/manual-mode.component";
import { LoginComponent } from './pages/login/login.component';
import { AuthGuard } from './services/auth/auth.guard';
import { GamesView } from './pages/game/games-view/games-view';
import { GameForm } from './pages/game/game-form/game-form';

export const routes: Routes = [
    { path: 'web-scraper', component: WebScraperComponent, title: "Web Scraper", canActivate: [AuthGuard]    },
    { path: 'manual-mode', component: ManualModeComponent, title: "Manual Mode", canActivate: [AuthGuard]   },
    { path: 'login', component: LoginComponent, title: "Login" },
    { path: 'games', component: GamesView, title: "Games" },
    { path: 'games/create',    component: GameForm  },
    { path: 'games/edit/:id',    component: GameForm  },
    { path: '', redirectTo: 'login', pathMatch: 'full' },
];
