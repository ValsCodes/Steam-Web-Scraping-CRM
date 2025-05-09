import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ManualModeEnum } from '../../common/enums/index';
import { CONSTANTS } from '../../common/constants';


@Component({
  selector: 'steam-manual-mode',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './manual-mode.component.html',
  styleUrl: './manual-mode.component.scss',
})
export class ManualModeComponent {

  private _constants = CONSTANTS;

  private readonly _hatUrlPartial: string = this._constants.SEARCH_URL_PARTIAL + this._constants.PRODUCT_QUERY_PARAMS_EXTENDED;

  private currentUrl: string = this._hatUrlPartial;


constructor(private http: HttpClient) {  this.getWeaponNames(); }

  public readonly weaponUrlPartial: string = this._constants.LISTING_URL_PARTIAL + this._constants.WEAPON_URL_QUERY_PARAMS;;
  public readonly weaponNames: string[] = [];

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
        : this.weaponNames.length > this.currentPage
        ? `${this.currentUrl}${this.weaponNames[this.currentPage - 1]}`
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
    const url = `${this._constants.LOCAL_HOST}item/get/all`;
    this.http.get<any[]>(url).subscribe({
      next: (data: any[]) => {

      const filteredData = data.reduce((acc:string[], item:any) => {
        if(item.isActive === true)
        {
          acc.push(item.name);
        }

        return acc;
      },[]);

      this.weaponNames.push(...filteredData);

      },
      error: (error) => {
        console.error('Error fetching weapon names:', error);
      }
    });
  }

  hatsButtonClicked(): void {
    if (!this.isHatsSearch) {
      this.currentUrl = this._hatUrlPartial;
      this.isHatsSearch = true;

      this.resetButtonClicked();
    }
  }

  weaponsButtonClicked(): void {
    if (this.isHatsSearch) {
      this.currentUrl = this.weaponUrlPartial;
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
