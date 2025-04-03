import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { catchError } from 'rxjs';
import { ManualModesEnum } from '../../models/enums/manual-modes.enum';

@Component({
  selector: 'steam-manual-mode',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './manual-mode.component.html',
  styleUrl: './manual-mode.component.scss',
})
export class ManualModeComponent {
  private readonly HATS_URL: string =
    'https://steamcommunity.com/market/search?q=&category_440_Collection%5B%5D=any&category_440_Type%5B%5D=tag_misc&category_440_Quality%5B%5D=tag_Unique&category_440_Quality%5B%5D=tag_strange&category_440_Rarity%5B%5D=tag_Rarity_Rare&category_440_Rarity%5B%5D=tag_Rarity_Mythical&category_440_Rarity%5B%5D=tag_Rarity_Legendary&category_440_Rarity%5B%5D=tag_Rarity_Ancient&appid=440#';
  // 'https://steamcommunity.com/market/search?q=&category_440_Collection%5B%5D=any&category_440_Type%5B%5D=tag_misc&category_440_Quality%5B%5D=tag_Unique&category_440_Quality%5B%5D=tag_strange&appid=440#';
  public readonly WEAPONS_URL: string =
    'https://steamcommunity.com/market/listings/440/Strange%20Specialized%20Killstreak%20';

  public readonly WEAPON_NAMES: string[] = [];
  private readonly localHost:string = 'https://localhost:44347/';

  private currentUrl: string = this.HATS_URL;

constructor(private http: HttpClient) {

    this.getWeaponNames();
  }

  public isHatsSearch: boolean = true;
  public batchSize: number = 7;
  public currentPage: number = 76;
  public statusLabel: string = '';

  startButtonClicked(): void {
    let toPage = this.currentPage + this.batchSize;
    let blocked = false;

    for (; this.currentPage < toPage; this.currentPage++) {
      let url = this.isHatsSearch
        ? `${this.currentUrl}p${this.currentPage}_price_asc`
        : this.WEAPON_NAMES.length > this.currentPage
        ? `${this.currentUrl}${this.WEAPON_NAMES[this.currentPage - 1]}`
        : null;

      if (url == null) {
        this.statusLabel = 'No More Weapons!';
        return;
      }

      let newWindow = window.open(url, '_blank');

      if (
        !newWindow ||
        newWindow.closed ||
        typeof newWindow.closed === 'undefined'
      ) {
        blocked = true;
      }
    }

    this.statusLabel = blocked ? 'Bad Batch!' : 'Successful Batch!';

    if (blocked) {
      alert(
        'Pop-ups were blocked. Please enable pop-ups for this website to open all pages.'
      );
    }
  }

  getWeaponNames(): void {
    const url = `${this.localHost}item/get/all`;
    this.http.get<any[]>(url).subscribe({
      next: (data: any[]) => {

      const filteredData = data.reduce((acc:string[], item:any) => {
        if(item.isActive === true)
        {
          acc.push(item.name);
        }

        return acc;
      },[]);

      this.WEAPON_NAMES.push(...filteredData);

      },
      error: (error) => {
        console.error('Error fetching weapon names:', error);
      }
    });
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
