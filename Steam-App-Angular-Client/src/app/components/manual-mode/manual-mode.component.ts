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
  private readonly MANUAL_URL: string = 'https://steamcommunity.com/market/search?q=&category_440_Collection%5B%5D=any&category_440_Type%5B%5D=tag_misc&category_440_Quality%5B%5D=tag_Unique&category_440_Quality%5B%5D=tag_strange&appid=440#';
  
  public batchSize: number = 7;
  public currentPage: number = 76;
  public statusLabel: string = "";

  resetButtonClicked(): void {
    this.currentPage = 76;
    this.batchSize = 7;
    this.statusLabel = "";
  }

  startButtonClicked(): void {
    let toPage = this.currentPage + this.batchSize;
    let blocked = false;

    for (; this.currentPage < toPage; this.currentPage++) 
    {
      let page = `p${this.currentPage}_price_asc`;
      let newWindow = window.open(`${this.MANUAL_URL}${page}`, '_blank');

      if (!newWindow || newWindow.closed || typeof newWindow.closed === 'undefined' ) 
      {     
        blocked = true;
      }
    }

    if (blocked) 
    {
      this.statusLabel = "Bad Batch!"
      alert('Pop-ups were blocked. Please enable pop-ups for this website to open all pages.');
    }
    else
    {
      this.statusLabel = "Successful Batch!"
    }
  }
}
