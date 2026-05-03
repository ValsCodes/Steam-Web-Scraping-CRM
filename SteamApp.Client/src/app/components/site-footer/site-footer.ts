import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'steam-site-footer',
  imports: [CommonModule, RouterModule],
  templateUrl: './site-footer.html',
  styleUrl: './site-footer.scss',
  standalone: true,
})
export class SiteFooter {

}
