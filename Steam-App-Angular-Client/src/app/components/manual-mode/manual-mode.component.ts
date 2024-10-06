import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SellListingComponent } from '../sell-listing/sell-listing.component';

@Component({
  selector: 'steam-manual-mode',
  standalone: true,
  imports: [SellListingComponent, FormsModule],
  templateUrl: './manual-mode.component.html',
  styleUrl: './manual-mode.component.scss',
})
export class ManualModeComponent {
  private readonly HATS_URL: string =
    'https://steamcommunity.com/market/search?q=&category_440_Collection%5B%5D=any&category_440_Type%5B%5D=tag_misc&category_440_Quality%5B%5D=tag_Unique&category_440_Quality%5B%5D=tag_strange&appid=440#';
  private readonly WEAPONS_URL: string =
    'https://steamcommunity.com/market/search?q=&category_440_Collection%5B%5D=any&category_440_Type%5B%5D=tag_misc&category_440_Quality%5B%5D=tag_Unique&category_440_Quality%5B%5D=tag_strange&appid=440#';

  private currentUrl: string = this.HATS_URL;
  private isHatsSearch: boolean = true;

  public batchSize: number = 7;
  public currentPage: number = 76;
  public statusLabel: string = '';

  startButtonClicked(): void {
    let toPage = this.currentPage + this.batchSize;
    let blocked = false;

    for (; this.currentPage < toPage; this.currentPage++) {
      let page = `p${this.currentPage}_price_asc`;
      let newWindow = window.open(`${this.currentUrl}${page}`, '_blank');

      if (
        !newWindow ||
        newWindow.closed ||
        typeof newWindow.closed === 'undefined'
      ) {
        blocked = true;
      }
    }

    if (blocked) {
      this.statusLabel = 'Bad Batch!';
      alert(
        'Pop-ups were blocked. Please enable pop-ups for this website to open all pages.'
      );
    } else {
      this.statusLabel = 'Successful Batch!';
    }
  }

  hatsButtonClicked() {
    if (!this.isHatsSearch) {
      this.currentUrl = this.HATS_URL;
      this.isHatsSearch = true;

      this.resetButtonClicked();
    }
  }

  weaponsButtonClicked() {
    if (this.isHatsSearch) {
      this.currentUrl = this.WEAPONS_URL;
      this.isHatsSearch = false;

      this.resetButtonClicked();
    }
  }

  resetButtonClicked(): void {
    if (this.isHatsSearch) {
      this.currentPage = 76;
      this.batchSize = 7;
    } else {
      this.currentPage = 1;
      this.batchSize = 3;
    }

    this.statusLabel = '';
  }
}
