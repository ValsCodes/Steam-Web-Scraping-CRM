import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SiteHeaderComponent } from './components/site-header/site-header.component';
import { MatPaginatorModule } from '@angular/material/paginator';
import { SiteFooter } from "./components/site-footer/site-footer";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    FormsModule,
    SiteHeaderComponent,
    MatPaginatorModule,
    SiteFooter
],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {

  title = 'steam-app-angular-client';
}
