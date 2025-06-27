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

  selectedClasses: number[] = [];

  // list out your classes with their IDs
  classes = [
    { id: 1,  name: 'Scout'    },
    { id: 2,  name: 'Soldier'  },
    { id: 3,  name: 'Pyro'     },
    { id: 4,  name: 'Demoman'  },
    { id: 5,  name: 'Heavy'    },
    { id: 6,  name: 'Engineer' },
    { id: 7,  name: 'Medic'    },
    { id: 8,  name: 'Sniper'   },
    { id: 9,  name: 'Spy'      }
  ];

    selectedSlots: number[] = [];

  slots = [
    { id: 1,  name: 'Primary'    },
    { id: 2,  name: 'Secondary'  },
    { id: 3,  name: 'Melle'     },
    { id: 4,  name: 'Other'  },
  ];

  private _constants = CONSTANTS;

  private readonly _hatUrlPartial: string = this._constants.SEARCH_URL_PARTIAL + this._constants.PRODUCT_QUERY_PARAMS_EXTENDED;

  private currentUrl: string = this._hatUrlPartial;


constructor(private http: HttpClient) {  this.getWeaponNames(); }

  public readonly weaponUrlPartial: string = this._constants.LISTING_URL_PARTIAL + this._constants.WEAPON_URL_QUERY_PARAMS;;
  public weaponNames: string[] = [];

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

  onClassCheckboxChange(event: Event, classId: number) {
    const checkbox = event.target as HTMLInputElement;
    if (checkbox.checked) {
      if (!this.selectedClasses.includes(classId)) {
        this.selectedClasses.push(classId);
      }
    } else {
      this.selectedClasses = this.selectedClasses.filter(id => id !== classId);
    }
    console.log('Selected class IDs:', this.selectedClasses);

    this.getWeaponNames();
  }

   onSlotCheckboxChange(event: Event, slotId: number) {
    const checkbox = event.target as HTMLInputElement;
    if (checkbox.checked) {
      if (!this.selectedSlots.includes(slotId)) {
        this.selectedSlots.push(slotId);
      }
    } else {
      this.selectedSlots = this.selectedSlots.filter(id => id !== slotId);
    }
    console.log('Selected slot IDs:', this.selectedSlots);

    this.getWeaponNames();
  }

  getWeaponNames(): void {
    const classFilters = this.selectedClasses.map(id => `classFilters=${encodeURIComponent(id)}`);
    const slotFilters = this.selectedSlots.map(id => `slotFilters=${encodeURIComponent(id)}`);
    const filters = [...classFilters, ...slotFilters].join('&');
    
    const url = `${this._constants.LOCAL_HOST}item?${filters}`;
    console.log(url);
    this.http.get<any[]>(url).subscribe({
      next: (data: any[]) => {

 const filteredData = data
      .filter(item => item.isActive)
      .sort((a, b) => (a.classId - b.classId) ||  (a.slotId   - b.slotId))
      .map(item => item.name);

      this.weaponNames.length = 0;
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

  clearSlotFilters(): void {
      this.selectedSlots.length = 0

    document.querySelectorAll('.slot-filters input[type="checkbox"]')
    .forEach((el: Element) => (el as HTMLInputElement).checked = false);

      this.getWeaponNames();
  }

  clearClassFilters(): void {
      this.selectedClasses.length = 0

    document.querySelectorAll('.class-filters input[type="checkbox"]')
    .forEach((el: Element) => (el as HTMLInputElement).checked = false);

      this.getWeaponNames();
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
