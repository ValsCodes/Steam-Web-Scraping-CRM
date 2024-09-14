import { Component } from '@angular/core';

@Component({
  selector: 'steam-manual-mode',
  standalone: true,
  imports: [],
  templateUrl: './manual-mode.component.html',
  styleUrl: './manual-mode.component.scss',
})
export class ManualModeComponent {
  private readonly MANUAL_URL: string ='https://steamcommunity.com/market/search?q=&category_440_Collection%5B%5D=any&category_440_Type%5B%5D=tag_misc&category_440_Quality%5B%5D=tag_Unique&category_440_Quality%5B%5D=tag_strange&appid=440#';
  public batchSize: number = 0;
  public currentPage: number = 76;

  async manualBatchButtonClicked(): Promise<void> {
    let toPage = this.currentPage + this.batchSize;

    for (; this.currentPage < toPage; this.currentPage++) {
      const page = `p${this.currentPage}_price_asc`;

      await this.sleep(200);
      window.open(`${this.MANUAL_URL}${page}`, '_blank');
    }

    this.currentPage += this.batchSize;
  }

  private sleep(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
}
