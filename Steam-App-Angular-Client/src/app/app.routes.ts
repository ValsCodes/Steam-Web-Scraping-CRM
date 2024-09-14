import { Routes } from '@angular/router';

import { HomeComponent } from "./home/home.component";
import { ManualModeComponent } from "./manual-mode/manual-mode.component";

export const routes: Routes = [
    { path: 'home', component: HomeComponent, title: "Home" },
    { path: 'manual-mode', component: ManualModeComponent, title: "Manual Mode" },
];
