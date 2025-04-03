import { Routes } from '@angular/router';

import { HomeComponent } from "./components/home/home.component";
import { WebScraperComponent } from "./components/web-scraper/web-scraper.component";
import { ManualModeComponent } from "./components/manual-mode/manual-mode.component";
import { SellListingsComponent } from "./components/sell-listings/sell-listings.component";

export const routes: Routes = [
    { path: 'home', component: HomeComponent, title: "Home" },
    { path: 'web-scraper', component: WebScraperComponent, title: "Web Scraper" },
    { path: 'manual-mode', component: ManualModeComponent, title: "Manual Mode" },
    { path: 'sell-listings', component: SellListingsComponent, title: "Sell Listings" },
    { path: '', redirectTo: 'home', pathMatch: 'full' },
];
