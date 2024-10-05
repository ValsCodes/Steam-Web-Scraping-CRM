import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { ManualModeComponent } from './components/manual-mode/manual-mode.component';
import { FormsModule } from '@angular/forms';
import { SiteHeaderComponent } from './components/site-header/site-header.component';
import { MatPaginatorModule } from '@angular/material/paginator';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    HomeComponent,
    ManualModeComponent,
    FormsModule,
    SiteHeaderComponent,
    MatPaginatorModule,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  title = 'steam-app-angular-client';
}
